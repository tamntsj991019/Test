using Data.ConstData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class RequestCleaningTool : BaseEntity
    {
        public string EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; }

        public Guid CleaningToolId { get; set; }
        [ForeignKey("CleaningToolId")]
        public virtual CleaningTool CleaningTool { get; set; }

        public string RequestStatusId { get; set; } = ConstRequestStatus.PENDING;
        [ForeignKey("RequestStatusId")]
        public virtual RequestStatus RequestStatus { get; set; }
    }
}
