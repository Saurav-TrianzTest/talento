using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class RecruiterLedDetails
    {
        public string HRRFNumber { get; set; }

        public string AccountName { get; set; }
        public string Criticality { get; set; }
        public string Purpose { get; set; }
        public string Practice { get; set; }
        public string Skill { get; set; }
        public string SkillCode { get; set; }
        public Nullable<int> Ageing { get; set; }
        public string RecruiterName { get; set; }
        public string TAleadName { get; set; }

    }
}