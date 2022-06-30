using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ServiceGroupViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid? HasImage { get; set; }
        
        public string Type { get; set; } // NORMAL, OPTIONAL, OVERALL
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class ServiceGroupCreateModel
    {
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
        public string Type { get; set; } // NORMAL, OPTIONAL, OVERALL
    }
    
    public class ServiceGroupUpdateModel
    {
        public string Description { get; set; }
        public string Type { get; set; } // NORMAL, OPTIONAL, OVERALL
    }

    public class ServiceGroupWithServiceViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public List<ServiceViewModel> ListService { get; set; }
    }
}
