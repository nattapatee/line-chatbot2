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
                    await SendLineDb(ev.Source.UserId, ev.Postback.Data);
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        $"ขอบคุณที่สมัครแจ้งเตือนแอปพลิเคชั่น {ev.Postback.Data} เมื่อเราตรวจพบ Critical เราแจ้งเตือนหาท่านให้เร็วที่สุด หากท่านต้องการยกเลิกติดตามให้พิมพ์ว่า \"unsub\" ขอบคุณครับ");
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
        }
        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            ISendMessage replyMessage = new TextMessage("ขอบคุณสำหรับข้อความ! ขออภัย เราไม่สามารถตอบกลับผู้ใช้ เป็นส่วนตัวได้จากบัญชีนี้้ ถ้าคุณต้องการติดตาม log กรุณาพิมพ์คำว่า\"sub\"เพื่อเลือกแอปพลิเคชั่นที่ต้องการติดตาม หากท่านไม่ต้องการติดตามแล้วให้พิมพ์คำว่า \"unsub\" เพื่อยกเลิกการติดตาม");

            if (userMessage.ToLower() == "hello")
            {
                replyMessage = new TextMessage("Hi!!");
            }
            if (userMessage.ToLower() == "วันนี้ฝนตกไหม")
            {
                replyMessage = new TextMessage("เปิดทีวีดูซิ ถามบอทบอทจะรู้ไหม");
            }
            if (userMessage.ToLower() == "วันนี้วันไร" || userMessage.ToLower() == "กี่โมงแล้ว")
            {
                replyMessage = new TextMessage("นี้บอท logger คิดว่าจะรู้ไหมน้อง");
            }
            if (userMessage.ToLower() == "วันนี้ฝนตกไหม")
            {
                replyMessage = new TextMessage("เปิดทีวีดูซิ ถามบอทบอทจะรู้ไหม");
            }
            if (userMessage.ToLower() == "จุฬ")
            {
                replyMessage = new TextMessage("อ้อ แฟนเก่าแอม");
            }
            if (userMessage.ToLower() == "ง่วง" || userMessage.ToLower() == "ง่วงจัง")
            {
                replyMessage = new TextMessage("มึงก็ไปนอนซะ!");
            }
            if (userMessage.ToLower() == "เหงา")
            {
                replyMessage = new TextMessage("หนูก็เหงา");
            }
            if (userMessage.ToLower() == "หิว" || userMessage.ToLower() == "หิวจัง")
            {
                replyMessage = new TextMessage("ก็แดกสิไอ้สัส");
            }
            if (userMessage.ToLower() == "ฝ้าย" || userMessage.ToLower() == "ฝ้ายคนสวย")
            {
                replyMessage = new TextMessage("ดูนมน่อย");
            }
            if (userMessage.ToLower() == "เคยตายไหม")
            {
                replyMessage = new TextMessage("แค่ ddos มาหนูก็ม๊องละค่ะ");
            }
            if (userMessage.ToLower() == "เสือก" || userMessage.ToLower() == "ไม่เสือก")
            {
                replyMessage = new TextMessage("มึงแหละเสือกมาคุยกับกูทำไม!");
            }
            if (userMessage.ToLower() == "ไอ้สัส" || userMessage.ToLower() == "อีสัส" || userMessage.ToLower() == "สัส" || userMessage.ToLower() == "เหี้ย")
            {
                replyMessage = new TextMessage("ด่าหนูทำไม หนูทำไรผิด");
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
            if (userMessage.ToLower() == "ชื่อไรอะ" || userMessage.ToLower() == "ชื่อไร" || userMessage.ToLower() == "ชื่อไรสัส" || userMessage.ToLower() == "มึงใคร" || userMessage.ToLower() == "มึงเป็นใคร")
            {
                replyMessage = new TextMessage("เซนทรัลลลล ล็อคเกอรรร์ ละเตงชื่อไร");
            }
            if (userMessage.ToLower() == "พีพี")
            {
                replyMessage = new TextMessage("อ้อ อีโง่ ดักดาน เอาแต่ตัวเองนะอีแก่ ตักน้ำใส่กะโหลก ชะโงกดูเงาหัวตัวเองมั้ง สติลูก รู้จักไหม หืมมม");
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
            if (userMessage.ToLower() == "sub")
            {
                List<ITemplateAction> actions2 = new List<ITemplateAction>();

                var url = "http://centralloggerazure.azurewebsites.net/api/Logger/GetAllApp";
                var client = new HttpClient();
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
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri("https://centralloggerazure.azurewebsites.net/api/line/DeleteLine"),
                        Content = new StringContent(data, Encoding.UTF8, "application/json")
                    };
                    var response = await client.SendAsync(request);
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