using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Location 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsDisable { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);

        public string ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Location Parent { get; set; }
    }
}
