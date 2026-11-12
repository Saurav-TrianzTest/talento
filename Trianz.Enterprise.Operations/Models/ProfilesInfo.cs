using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ProfilesInfo
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


        public long ProfilesInfoID { get; set; }
        public Nullable<int> Profiles { get; set; }
        public Nullable<int> L1 { get; set; }
        public Nullable<int> L2 { get; set; }
        public Nullable<int> HR { get; set; }
        public Nullable<int> PendingInterviews { get; set; }
        public Nullable<System.DateTime> ProfilesDate { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

    }
}