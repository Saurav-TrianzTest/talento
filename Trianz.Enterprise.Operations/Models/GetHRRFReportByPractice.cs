using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class GetHRRFReportByPractice
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string OldHRRFNumber { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Ageing { get; set; }
        public string AgeingBucket { get; set; }
        public string Purpose { get; set; }
        public string BillingStatus { get; set; }
        public string YearsOfExperience { get; set; }
        public string ProjectNameWithCode { get; set; }

        public string ProjectNameOrOppurtunityName { get; set; }
        public string ProjectCodeOrOppurtunityCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        
        public string OpportunityCode { get; set; }
        public string OpportunityName { get; set; }
        public string AccountName { get; set; }
        public string ServiceLine { get; set; }
        public string CostCenter { get; set; }
        public string RequestType { get; set; }
        public string SkillCategory { get; set; }
        public string PrimarySkillSet { get; set; }
        public string JobDescription { get; set; }
        public Nullable<int> Grade { get; set; }
        public string RequestStatus { get; set; }
        public Nullable<System.DateTime> FulfillmentDate { get; set; }
        public string FulfillmentRemarks { get; set; }
        public string LocationType { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> Positions { get; set; }
        public string ResourceName { get; set; }
        public Nullable<System.DateTime> AssignmentStartDate { get; set; }
        public string DemandType { get; set; }
        public string RequestReason { get; set; }
        public string RoleRequired { get; set; }
        public Nullable<int> HRRFCreatedById { get; set; }
        public string HRRFCreatedByName { get; set; }
        public Nullable<System.DateTime> DOJ { get; set; }
        public Nullable<System.DateTime> DateFromIntToExt { get; set; }
        public Nullable<System.DateTime> DateOfHold { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
        public string CancelRemarks { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string RecruiterName { get; set; }
        public Nullable<System.DateTime> ExpectedFulfilmentDate { get; set; }
        public string OverDue { get; set; }
        public string JoiningMonth { get; set; }
        public Nullable<System.DateTime> BillingDate { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> ReplacementEmpID { get; set; }
        public string ReplacementName { get; set; }
        public string QualifySl { get; set; }
        public string ExappSL { get; set; }
        public string SourcngSL { get; set; }
        public string FirstSL { get; set; }
        public string SceondSL { get; set; }
        public string ClientSL { get; set; }
        public string HRSl { get; set; }
        public string Contracting { get; set; }
        public string Impact { get; set; }
        public string Criticality { get; set; }
        public string SkillCluster { get; set; }
        public string CSkill { get; set; }
        public string SkillCode { get; set; }
        public string CLSTRDESC { get; set; }
        public string CLSTRJD { get; set; }
        public Nullable<int> ExternalConverstionAgeing { get; set; }
        public string ExternalConverstionAgeingBucket { get; set; }
        public Nullable<System.DateTime> DHApprovaerDate { get; set; }
        public Nullable<int> InternalConverstionAgeing { get; set; }
        public string InternalConverstionAgeingBucket { get; set; }
        public string ClientInterview { get; set; }
        public string TECHPANEL { get; set; }
        public string SECONDTECHPANEL { get; set; }

        public string Trained { get; set; }
        public string Experienced { get; set; }
        public string Proficient { get; set; }
        public string Expert { get; set; }
        public string RequisitionID { get; set; }
        public string Stage { get; set; }

        public string CoreSkill { get; set; }
        public string IntermediateSkill { get; set; }
        public string AdvancedSkill { get; set; }
        public string SpecificPlatform { get; set; }
        public string Discipline { get; set; }
        public string RoleGroup { get; set; }
        public string Certifications { get; set; }

        public Nullable<decimal> Billrate { get; set; }

        public Nullable<double> Maxsal { get; set; }

        public string OrganizationGroup { get; set; }
        public string OrganizationSubGroup { get; set; }

        public string HrrfStatus { get; set; }
        public string ReasonRaisingTR { get; set; }

    }

    public class GetHRRFReportByPracticeSub
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string OldHRRFNumber { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Ageing { get; set; }
        public string AgeingBucket { get; set; }
        public string Purpose { get; set; }
        public string BillingStatus { get; set; }
        public string YearsOfExperience { get; set; }
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
        public Nullable<int> Grade { get; set; }
        public string RequestStatus { get; set; }
        public Nullable<System.DateTime> FulfillmentDate { get; set; }
        public string FulfillmentRemarks { get; set; }
        public string LocationType { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> Positions { get; set; }
        public string ResourceName { get; set; }
        public Nullable<System.DateTime> AssignmentStartDate { get; set; }
        public string DemandType { get; set; }
        public string RequestReason { get; set; }
        public string RoleRequired { get; set; }
        public Nullable<int> HRRFCreatedById { get; set; }
        public string HRRFCreatedByName { get; set; }
        public Nullable<System.DateTime> DOJ { get; set; }
        public Nullable<System.DateTime> DateFromIntToExt { get; set; }
        public Nullable<System.DateTime> DateOfHold { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
        public string CancelRemarks { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string RecruiterName { get; set; }
        public Nullable<System.DateTime> ExpectedFulfilmentDate { get; set; }
        public string OverDue { get; set; }
        public string JoiningMonth { get; set; }
        public Nullable<System.DateTime> BillingDate { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> ReplacementEmpID { get; set; }
        public string ReplacementName { get; set; }
        public string QualifySl { get; set; }
        public string ExappSL { get; set; }
        public string SourcngSL { get; set; }
        public string FirstSL { get; set; }
        public string SceondSL { get; set; }
        public string ClientSL { get; set; }
        public string HRSl { get; set; }
        public string Contracting { get; set; }
        public string Impact { get; set; }
        public string Criticality { get; set; }
        public string SkillCluster { get; set; }
        public string CSkill { get; set; }
        public string SkillCode { get; set; }
        public string CLSTRDESC { get; set; }
        public string CLSTRJD { get; set; }
        public Nullable<int> ExternalConverstionAgeing { get; set; }
        public string ExternalConverstionAgeingBucket { get; set; }
        public Nullable<System.DateTime> DHApprovaerDate { get; set; }
        public Nullable<int> InternalConverstionAgeing { get; set; }
        public string InternalConverstionAgeingBucket { get; set; }
        public string ClientInterview { get; set; }
        public string TECHPANEL { get; set; }
        public string SECONDTECHPANEL { get; set; }

        public string Trained { get; set; }
        public string Experienced { get; set; }
        public string Proficient { get; set; }
        public string Expert { get; set; }
        public string RequisitionID { get; set; }
        public string Stage { get; set; }

        public string CoreSkill { get; set; }
        public string IntermediateSkill { get; set; }
        public string AdvancedSkill { get; set; }
        public string SpecificPlatform { get; set; }
        public string Discipline { get; set; }
        public string RoleGroup { get; set; }
        public string Certifications { get; set; }
        public Nullable<decimal> Billrate { get; set; }
        public Nullable<double> Maxsal { get; set; }
        public string OrganizationGroup { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string Talead { get; set; }
        public string Rectr { get; set; }
        public string BoSpoc { get; set; }
        public string SalesStage { get; set; }
        public string Organization_SubGroup { get; set; }
        public string HrrfStatus { get; set; }
        public string Practice { set; get; }
        public string HrrfRemarks { get; set; }
        public string PracticeA { get; set; }
    }
}