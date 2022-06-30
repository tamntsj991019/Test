using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class BaseStringModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public class BaseGuidViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class BaseCreateModel
    {
        public string Description { get; set; }
    }
    public class BaseUpdateModel
    {
        public string Description { get; set; }
    }

}
