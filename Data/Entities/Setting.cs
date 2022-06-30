using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class Setting : BaseEntity
    {
        public string Key { get; set; }
        public string Data { get; set; }
    }
}
