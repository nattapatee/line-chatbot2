using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using centralloggerbot.CloudStorage;
using centralloggerbot.Models;
using Microsoft.EntityFrameworkCore;

namespace centralloggerbot.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineBotController : Controller
    {
        private static LineMessagingClient lineMessagingClient;
        private readonly DbCreateContext db;
        AppSettings appsettings;
        public LineBotController(IOptions<AppSettings> options, DbCreateContext db)
        {
            this.db = db;
            appsettings = options.Value;
            lineMessagingClient = new LineMessagingClient(appsettings.LineSettings.ChannelAccessToken);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JToken req)
        {
            var events = WebhookEventParser.Parse(req.ToString());
            var connectionString = appsettings.LineSettings.StorageConnectionString;
            var blobStorage = await BlobStorage.CreateAsync(connectionString, "linebotcontainer");
            var eventSourceState = await TableStorage<EventSourceState>.CreateAsync(connectionString, "eventsourcestate");

            var app = new LineBotApp(db, lineMessagingClient, eventSourceState, blobStorage);
            await app.RunAsync(events);
            return new OkResult();
        }
    }
}
