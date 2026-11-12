using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class BenchEmployees : Employees
    {
        public int BenchEmployeeId { get; set; }
        public int Assignment_ID { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Practice { get; set; }
        public string SPOC { get; set; }
        public string PotentialAccount { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string AssignmentStatus { get; set; }
        public string Comments { get; set; }
        public string IsActive { get; set; }
        public string ChangedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Utilization { get; set; }
        public int? Rating { get; set; }
        public string NonBillableCountDays { get; set; }
        public string ProjectManager { get; set; }
        public string Supervisor { get; set; }
        public string Ageing { get; set; }
        public int? Grade { get; set; }
        public string Customer { get; set; }
        public string Category { get; set; }
        public string BenchStatus { get; set; }
        public string Remarks { get; set; }
        public DateTime? ExpectedBillingDate { get; set; }
        public DateTime? MappedDate { get; set; }
        public string HRRFNumber { get; set; }
        public DateTime? ActionReminder { get; set; }
        public string ReservedProjectCode { get; set; }

        //StartDate = be.StartDate,
        // EndDate = be.EndDate,
        // Utilization = be.Utilization,
    }
}