using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class CleaningToolViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public Guid? HasImage { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
    
    public class CleaningToolRequestViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid? HasImage { get; set; }
    }

    public class CleaningToolCreateModel
    {
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
        public int Quantity { get; set; }
    }

    public class CleaningToolUpdateModel
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
    }
}
