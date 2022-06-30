using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class User : IdentityUser
    {
        public static ClaimsIdentity Identity { get; internal set; }

        public byte[] Avatar { get; set; }

        [Required]
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public double Balance { get; set; } = 0;
        public string EmployeeCode { get; set; }
        public int? EmployeeCredit { get; set; }

        public string ProvinceId { get; set; }
        [ForeignKey("ProvinceId")]
        public virtual Location Province { get; set; }

        public string DistrictId { get; set; }
        [ForeignKey("DistrictId")]
        public virtual Location District { get; set; }

        public string WardId { get; set; }
        [ForeignKey("WardId")]
        public virtual Location Ward { get; set; }

        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDisable { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);

        public string ActiveCode { get; set; }
        public DateTime? CodeCreateTime { get; set; }

        public string HasAvatar => (Avatar != null) ? Id : null;
    }
}
