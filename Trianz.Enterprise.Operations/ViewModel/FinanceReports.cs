using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class FinanceReports
    {
        public List<FinanceBenchDetails> BenchReports { get; set; }
        public List<FinanceShadowDetails> ShadowReports { get; set; }
        public List<FinanceInternalDetails> InternalReports { get; set; }
        public List<FinanceContractorsDetails> ContactorReports { get; set; }
        public List<FinanceInternalspecificDetails> InternalspcReports { get; set; }
        public List<FinanceRawDataDetails> RawDataReports { get; set; }
        
        public List<InternalSpecificCodes> InternalSpecificDetails { get; set; }
        public List<FinanceUtilizationDetails> UtizationReports { get; set; }
        //public List<FinanceUtiliExceptFresher> ExceptFreshers { get; set; }
        //public List<FinanceUtiliExceptFresherManagement> ExceptFreshersManage { get; set; }
        //public List<FinanceUtiliExceptManagement> ExceptManage { get; set; }

    }
    public class FinanceBenchDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Worklocation { get; set; }
        public string site { get; set; }
        public int Grade { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string EmployeeType { get; set; }
        public System.Nullable<DateTime> StartDate { get; set; }
        public decimal HC { get; set; }
        public int Aging { get; set; }
        public string Deployable { get; set; }
        public string CustomerName { get; set; }
        public System.Nullable<DateTime> ExpectedBillingDate { get; set; }
        public System.Nullable<DateTime> MappedDate { get; set; }
    }
    public class FinanceShadowDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Worklocation { get; set; }
        public string site { get; set; }
        public int Grade { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string EmployeeType { get; set; }
        public System.Nullable<DateTime> StartDate { get; set; }
        public decimal HC { get; set; }
        public int Aging { get; set; }
    }
    public class FinanceInternalDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Worklocation { get; set; }
        public string site { get; set; }
        public int Grade { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string EmployeeType { get; set; }
        public int StandaredHours { get; set; }
        public System.Nullable<DateTime> StartDate { get; set; }
        public decimal HC { get; set; }
        public int Aging { get; set; }
    }
    public class FinanceContractorsDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Worklocation { get; set; }
        public string site { get; set; }
        public int Grade { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string BillingStatus { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string ABillable { get; set; }
        public string ANBillable { get; set; }
        public string UABillable { get; set; }
        public string UANBillable { get; set; }
        public string TotalBillable { get; set; }
        public string TotalNonBillable { get; set; }
        public string TotalHours { get; set; }
        public string Utilization { get; set; }
        public int Aging { get; set; }
    }
    public class FinanceInternalspecificDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Worklocation { get; set; }
        public string site { get; set; }
        public int Grade { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string EmployeeType { get; set; }
        public int StandaredHours { get; set; }
        public System.Nullable<DateTime> StartDate { get; set; }
        public decimal HC { get; set; }
        public int Aging { get; set; }
    }

    public class FinanceRawDataDetails
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int Grade { get; set; }
        public string EmployeeType { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string ClientName { get; set; }
        public string BusinessGroup { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string BillingType { get; set; }
        public decimal HC { get; set; }
        public string BillingStatus { get; set; }
        public System.Nullable<DateTime> DOJ { get; set; }
        public System.Nullable<DateTime> RelievingDate { get; set; }
        public string CostCenter { get; set; }
        public string ServiceLine { get; set; }
        
        public string BEorGTS { get; set; }
        public System.Nullable<DateTime> StartDate { get; set; }
        public System.Nullable<DateTime> EndDate { get; set; }
        public System.Nullable<DateTime> ProjectStartDate { get; set; }
        public System.Nullable<DateTime> ProjectEndDate { get; set; }
        public string ProjectSL { get; set; }
        public System.Nullable<DateTime> MappedDate { get; set; }
        public System.Nullable<DateTime> ExpectedBillingDate { get; set; }
        public string ABillable { get; set; }
        public string ANBillable { get; set; }
        public string UABillable { get; set; }
        public string UANBillable { get; set; }
        public string TotalBillable { get; set; }
        public string TotalNonBillable { get; set; }
        public string TotalHours { get; set; }
    }

    public class FinanceUtilizationDetails
    {

        public string BillingStatus { get; set; }
        public decimal ItemCount { get; set; }
        public int Seq { get; set; }
        public int ttype { get; set; }
    }
    public class FinanceUtiliExceptFresher
    {
        public string BillingStatus { get; set; }
        public decimal ItemCount { get; set; }
        public string Seq { get; set; }
    }
    public class FinanceUtiliExceptFresherManagement
    {
        public string BillingStatus { get; set; }
        public decimal ItemCount { get; set; }
        public string Seq { get; set; }
    }
    public class FinanceUtiliExceptManagement
    {
        public string BillingStatus { get; set; }
        public decimal ItemCount { get; set; }
        public int Seq { get; set; }
        public int ttype { get; set; }
    }
    public class InternalSpecificCodes
    {
        public int SNo { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
    }
}