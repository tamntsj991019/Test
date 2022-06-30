using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ConversationViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public string CustomerId { get; set; }
        public UserFullnameViewModel Customer { get; set; }

        public string EmployeeId { get; set; }
        public UserFullnameViewModel Employee { get; set; }

        public List<MessageViewModel> Messages { get; set; }
    }

    public class ConversationPushingViewModel
    {
        public Guid Id { get; set; }
        public string LastMessage { get; set; }
        public bool YourMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
    }

}
