using System;
using System.Collections.Generic;
using System.Linq;
using Trianz.Enterprise.Operations.Controllers;

namespace Trianz.Enterprise.Operations.Models
{
    public class BillingLoss
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }

        public string Practice { get; set; }
        public string ProjectName { get; set; }
        public string AccountName { get; set; }
        public string RequestStatus { get; set; }
        public Nullable<System.DateTime> BillingDate { get; set; }
        public Nullable<decimal> BILLRATE { get; set; }
        public string Criticality { get; set; }
        public Nullable<int> LeakageDays { get; set; }       
        public Nullable<decimal> LeakageRevenue { get; set; }

        public Nullable<System.DateTime> CurrentDate { get; set; }
    }
}