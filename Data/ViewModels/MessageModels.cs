using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class MessageCreateModel
    {
        public Guid ConversationId { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }

    public class ChatViewModel
    {
        public Guid? ConversationId { get; set; }
        public UserFullnameAvatarViewModel Receiver { get; set; }
        public List<MessageViewModel> ListMessage { get; set; }
    }

    public class MessageViewModel
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid BookingId { get; set; }
        public string Description { get; set; }
        public bool IsFile { get; set; }
        public string FileName { get; set; }
        public bool YourMessage { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
