using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ExternalVM : ExternalHire
    {
        public string HireName { get; set; }
        public string HireRequestStatus { get; set; }
        public string HireFulfilmentRemarks { get; set; }
        public DateTime HireDoj { get; set; }
        public DateTime HireFulfilmentDate { get; set; }
        public string HireCancelReason { get; set; }
        //public string HrfNumber { get; set; }
        //public long HrfId { get; set; }
        //public string HrfProjectName { get; set; }
        //public string HrfPratice { get; set; }
        //public string HrfLocation { get; set; }

        public class ExternalHireResult
        {
            public List<ExternalVM> externalhire { get; set; }
        }
    }
}