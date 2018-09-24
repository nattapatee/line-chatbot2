using Line.Messaging;
using Line.Messaging.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using centralloggerbot.CloudStorage;
using centralloggerbot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace centralloggerbot
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private TableStorage<EventSourceState> sourceState { get; }
        private BlobStorage blobStorage { get; }
        private string text;
        private string userId;

        public LineBotApp(string text, LineMessagingClient lineMessagingClient, TableStorage<EventSourceState> tableStorage, BlobStorage blobStorage)
        {
            this.text = text;
            this.messagingClient = lineMessagingClient;
            this.sourceState = tableStorage;
            this.blobStorage = blobStorage;
        }
        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            ISendMessage replyMessage = new TextMessage("");

            switch (ev.Postback.Data)
            {
                case "Date":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the date: " + ev.Postback.Params.Date);
                    break;
                case "Time":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the time: " + ev.Postback.Params.Time);
                    break;
                case "DateTime":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the date-time: " + ev.Postback.Params.DateTime);
                    break;
                default:
                    await SendLineDb(userId, ev.Postback.Data);
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        $"ขอบคุณที่สมัครข้อมูล{ev.Postback.Data} เมื่อเราตรวจพบ Critical เราแจ้งเตือนหาท่านให้เร็วที่สุด ขอบคุณครับ");
                    break;
            }
        }
        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
                case EventMessageType.Sticker:
                    await ReplyRandomStickerAsync(ev.ReplyToken);
                    break;
            }

        }
        private async Task ReplyRandomStickerAsync(string replyToken)
        {
            //Sticker ID of bssic stickers (packge ID =1)
            //see https://devdocs.line.me/files/sticker_list.pdf
            var stickerids = Enumerable.Range(1, 17)
                .Concat(Enumerable.Range(21, 1))
                .Concat(Enumerable.Range(100, 139 - 100 + 1))
                .Concat(Enumerable.Range(401, 430 - 400 + 1)).ToArray();

            var rand = new Random(Guid.NewGuid().GetHashCode());
            var stickerId = stickerids[rand.Next(stickerids.Length - 1)].ToString();
            await messagingClient.ReplyMessageAsync(replyToken, new[] {
                        new StickerMessage("1", stickerId)
                    });
        }
        private async Task SendLineDb(string userId, string application)
        {
            var message = new
            {
                LineId = userId,
                ApplicationName = application
            };
            var client = new HttpClient();
            var data = JsonConvert.SerializeObject(message);
            var fullUrl = $"https://centralloggerazure.azurewebsites.net/api/line/AddLine";

            var response = await client.PostAsync(fullUrl, new StringContent(data, Encoding.UTF8, "application/json"));
            this.userId = null;
        }
        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            ISendMessage replyMessage = new TextMessage("ขอบคุณสำหรับข้อความ! ขออภัย เราไม่สามารถตอบกลับผู้ใช้ เป็นส่วนตัวได้จากบัญชีนี้้ ถ้าคุณต้องการติดตาม log!");

            if (userMessage.ToLower() == "hello")
            {
                replyMessage = new TextMessage("Hi!!");
            }
            if (userMessage.ToLower() == "sublist")
            {
                replyMessage = new TemplateMessage("menu", new ButtonsTemplate(
                        title: "\"Application List\"",
                        text: "คุณต้องการติดตามแอปพลิเคชั่นไหน?",
                        actions: new List<ITemplateAction>(){
                            new MessageTemplateAction("Cheese Burger", "cheese"),
                            new MessageTemplateAction("Plain Burger","plain"),
                            new MessageTemplateAction("Vegi Burger","vegi"),
                            new MessageTemplateAction("Awesome Burger","awesome"),
                        }));
            }
            if (userMessage.ToLower() == "message")
            {
                replyMessage = new TextMessage($"You say{userMessage}");
            }
            if (userMessage.ToLower() == "sub app.dll")
            {
                var text = userMessage.Split(' ')[1];
                replyMessage = new TextMessage($"You say {text}");
            }
            if (userMessage.ToLower() == "applist")
            {
                var url = "http://centralloggerazure.azurewebsites.net/api/Logger/GetAllApp";
                var client = new HttpClient();

                var response = await client.GetAsync(url);
                var returnJson = await response.Content.ReadAsByteArrayAsync();
                var type = returnJson.GetType().ToString();

                replyMessage = new TextMessage(type + "\n" + returnJson);
            }
            if (userMessage.ToLower() == "หวัดดี" || userMessage.ToLower() == "สวัสดี")
            {
                replyMessage = new TextMessage("หวัดเด้");
            }
            if (userMessage.ToLower() == "subtest")
            {
                try
                {
                    await SendLineDb(userId, null);
                    replyMessage = new TextMessage($"ขอบคุณที่สมัครข้อมูล เมื่อเราตรวจพบ Critical เราแจ้งเตือนหาท่านให้เร็วที่สุด ขอบคุณครับ");
                }
                catch (Exception)
                {
                    replyMessage = new TextMessage($"พบข้อผิดพลาดในการสมัครข้อมูล กรุณาติดต่อผู้ดูแล");
                }
            }
            if (userMessage == "sub")
            {
                List<ITemplateAction> actions2 = new List<ITemplateAction>();

                var url = "http://centralloggerazure.azurewebsites.net/api/Logger/GetAllApp";
                var client = new HttpClient();
                this.userId = userId;
                var response = await client.GetAsync(url);
                var data = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<string[]>(data);

                foreach (var appName in json)
                {
                    actions2.Add(new PostbackTemplateAction(appName, appName));
                }

                replyMessage = new TemplateMessage("Button Template",
                    new CarouselTemplate(new List<CarouselColumn> {
                        new CarouselColumn("กรุณาเลือกแอปพลิเคชั่นที่ต้องการติดตาม", "https://its.unl.edu/images/services/icons/AppDevelopmentD_Icon-01_0.png",
                        "Choose application", actions2)
                    }));
            }

            if (userMessage.ToLower() == "unsub")
            {
                try
                {
                    var message = new
                    {
                        LineId = userId
                    };
                    var client = new HttpClient();
                    var data = JsonConvert.SerializeObject(message);
                    var fullUrl = $"https://centralloggerazure.azurewebsites.net/api/line/DeleteLine";
                    var response = await client.PostAsync(fullUrl, new StringContent(data, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        replyMessage = new TextMessage("เราได้ยกเลิกการแจ้งเตือน log เรียบร้อยแล้ว ขอบคุณครับ");
                    }
                    else if ((int)response.StatusCode == 500)
                    {
                        replyMessage = new TextMessage("พบข้อผิดพลาดในการลบ");
                    }
                }
                catch (Exception)
                {
                    replyMessage = new TextMessage($"พบข้อผิดพลาดในการลบ กรุณาติดต่อผู้ดูแล");
                }

            }
            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
        }
    }
}