using Microsoft.WindowsAzure.Storage.Table;

namespace centrallogerbot.Models
{
    public class EventSourceLocation : EventSourceState
    {
        public string Location { get; set; }

        public EventSourceLocation() { }
    }
}