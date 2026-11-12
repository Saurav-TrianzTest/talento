using System;
using System.Collections.Generic;
using System.Linq;
using Trianz.Enterprise.Operations.Controllers;
namespace Trianz.Enterprise.Operations.Models
{
    public class BillingLossList
    {

        public List<BillingLoss> BillingLossData { get; set; }

        public List<BillingLoss> PractiseList { get; set; }

        public List<BillingLoss> AccountNameList { get; set; }

        public decimal? CountPractiseList { get; set; }

        public decimal? CountAccountNameList { get; set; }
    }
}