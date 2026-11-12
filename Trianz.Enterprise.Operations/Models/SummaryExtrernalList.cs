using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class SummaryExtrernalList
    {
        public string AccountName { get; set; }
        public int TrCount { get; set; }
    }
    public class SummaryExtrernalByAccount
    {
		public string DesignationName { get; set; }										   
        public string HRRFNumber { get; set; }
        public string SkillCluster { get; set; }
		public string RoleRequired { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }																	   										   
        public string SkillCode { get; set; }
        public string RequestStatus { get; set; }
        public string Criticality { get; set; }
        // public Nullable<System.DateTime> HRRFCreatedDate { get; set; }
        public DateTime HRRFCreatedDate { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityCode { get; set; }
        public Decimal? BILLRATE { get; set; }
        public int Ageing { get; set; }
        public string RequestReason { get; set; }
        public int Grade { get; set; }
		public String Name { get; set; }								
    }
    public class SummaryExtrernalByCriticality
    {
        public string Criticality { get; set; }
        public int TrCount { get; set; }
    }
    public class SummaryExtrernalByRequestStatus
    {
        public string RequestStatus { get; set; }
        public int TrCount { get; set; }
    }

    public class CriticalityByAccountNamelst
    {
        public string AccountName { get; set; }
        public int EP0 { get; set; }
        public int EP1 { get; set; }
        public int EP2 { get; set; }
        public int EP3 { get; set; }
        public int IP0 { get; set; }
        public int IP1 { get; set; }
        public int IP2 { get; set; }
        public int IP3 { get; set; }
    }
}