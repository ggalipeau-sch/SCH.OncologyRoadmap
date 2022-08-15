using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCH.OncologyRoadmapWeb.Models
{

    public class AuditHistory
    {
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public List<AuditRecord> Record { get; set; }
    }

    public class AuditRecord
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}