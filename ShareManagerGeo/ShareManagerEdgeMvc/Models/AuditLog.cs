using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ShareManagerEdgeMvc.Models
{
    public class AuditLog
    {
        [Key]
        public Guid AuditLogID { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserID { get; set; }

        [Required]
        public DateTime EventDateUTC { get; set; }

        [Required]
        [MaxLength(1)]
        public string EventType { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; }

        [Required]
        [MaxLength(100)]
        public string RecordID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ColumnName { get; set; }

        public string OriginalValue { get; set; }

        public string NewValue { get; set; }

    }

}