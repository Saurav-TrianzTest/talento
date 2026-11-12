using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class HRRFExportToExcel
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string OldHRRFNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Ageing { get; set; }
        public string AgeingBucket { get; set; }
        public string Purpose { get; set; }
        public string ProjectNameWithCode { get; set; }
        public string OpportunityCode { get; set; }
        public string OpportunityName { get; set; }
        public string AccountName { get; set; }
        public string ServiceLine { get; set; }
        public string CostCenter { get; set; }
        public string RequestType { get; set; }
        public string SkillCategory { get; set; }
        public string PrimarySkillSet { get; set; }
        public string JobDescription { get; set; }
        public int Grade { get; set; }
        public string RequestStatus { get; set; }
        public DateTime? FulfillmentDate { get; set; }
        public string FulfillmentRemarks { get; set; }
        public string LocationType { get; set; }
        public string LocationName { get; set; }
        public int? Positions { get; set; }
        public string ResourceName { get; set; }
        public DateTime? AssignmentStartDate { get; set; }
        public string OverDue { get; set; }
        public string DemandType { get; set; }
        public string RequestReason { get; set; }
        public string RoleRequired { get; set; }
        public int? HRRFCreatedById { get; set; }
        public string HRRFCreatedByName { get; set; }
        public DateTime? DOJ { get; set; }
        public string JoiningMonth { get; set; }
        public DateTime? DateFromIntToExt { get; set; }
        public DateTime? DateOfHold { get; set; }
        public DateTime? CancelDate { get; set; }
        public string CancelRemarks { get; set; }
        public string EmployeeId { get; set; }
        public string RecruiterName { get; set; }
        public DateTime? ExpectedFulfilmentDate { get; set; }
        public DateTime? BillingDate { get; set; }
    }
}