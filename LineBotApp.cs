using Line.Messaging;
using Line.Messaging.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using centralloggerbot.CloudStorage;
using centralloggerbot.Models;

namespace centralloggerbot
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private TableStorage<EventSourceState> sourceState { get; }
        private BlobStorage blobStorage { get; }
        private readonly DbCreateContext db;

        public LineBotApp(DbCreateContext db, LineMessagingClient lineMessagingClient, TableStorage<EventSourceState> tableStorage, BlobStorage blobStorage)
        {
            this.db = db;
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
                var idlist = db.Users.Where(m => m.LineId == userId).Select(m => m.LineId).FirstOrDefault();
                if (idlist != userId && userId != null)
                {
                    db.Users.Add(new Users()
                    {
                        LineId = userId
                    });
                    db.SaveChanges();
                    replyMessage.Text = "ขอบคุณที่สมัครข้อมูล เมื่อเราตรวจพบ Critical เราแจ้งเตือนหาท่านให้เร็วที่สุด ขอบคุณครับ";
                }
                else
                {
                    replyMessage.Text = "พบข้อผิดพลาด ไอดีนี้การลงทะเบียนอยู่แล้ว";
                }

            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
        }
    }
}