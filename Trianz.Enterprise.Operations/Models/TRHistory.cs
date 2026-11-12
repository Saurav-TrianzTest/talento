using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class TRHistory
    {
        public long AuditID { get; set; }
        public string HRRFNumber { get; set; }
        public long HRRFID { get; set; }
        public string HistoryDescription { get; set; }
        public string Remarks { get; set; }
        public string ModifiedEmpName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}