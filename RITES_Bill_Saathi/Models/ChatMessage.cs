using System;
namespace RITES_Bill_Saathi.Models
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string User { get; set; } // "User" for user messages, "Bot" for bot responses
    }

}

