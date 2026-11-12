using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class HRRFRemarksinfo
    {
        public string HrrfNo { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> Submitteddate { get; set; }
        public string RemarkEmp { get; set; }
    }

    public class HRRFStatusHistoryDetails
    {
        public string HrrfNo { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> Submitteddate { get; set; }
        public string SubmittedBy { get; set; }
    }
}