using Line.Messaging;
using Line.Messaging.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using centralloggerbot.CloudStorage;
using centralloggerbot.Models;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace centralloggerbot
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private TableStorage<EventSourceState> sourceState { get; }
        private BlobStorage blobStorage { get; }
        public LineBotApp(LineMessagingClient lineMessagingClient, TableStorage<EventSourceState> tableStorage, BlobStorage blobStorage)
        {
            this.messagingClient = lineMessagingClient;
            this.sourceState = tableStorage;
            this.blobStorage = blobStorage;
        }
        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
            }
        }

        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            var replyMessage = new TextMessage($"You said: {userMessage}");
            if (userMessage.ToLower() == "hello")
            {
                replyMessage.Text = "Hi!!";
            }
            if (userMessage.ToLower() == "register")
            {
                var message = new
                {
                    LineId = userId
                };
                using (var client = new HttpClient())
                {
                    var data = JsonConvert.SerializeObject(message);
                    var fullUrl = $"htps://centralloggerazure.azurewebsites.net/api/logger/addLine";
                    var response = await client.PostAsync(fullUrl, new StringContent(data, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        replyMessage.Text = "ขอบคุณที่สมัครข้อมูล เมื่อเราตรวจพบ Critical เราแจ้งเตือนหาท่านให้เร็วที่สุด ขอบคุณครับ";
                    }
                    else
                    {
                        replyMessage.Text = "พบปัญหาในการสมัครรับข้อมูล";
                    }
                }
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
        }
    }
}
