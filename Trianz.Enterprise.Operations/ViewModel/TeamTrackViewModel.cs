using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class TeamTrackViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Location { get; set; }
        public string Grade { get; set; }
        public string Designation { get; set; }
        public string Track { get; set; }
        public string Practice { get; set; }
        public string Projectcode { get; set; }

    }

    public class TrackData
    {
        public string EmployeeID { get; set; }
        public string TrackOldData { get; set; }
        public string TrackNewData { get; set; }

    }

    public partial class TeamTrackHistoryViewModel
    {
        public int TrackHistoryID { get; set; }
        public int EmployeeID { get; set; }
        public string Track_Old { get; set; }
        public string Track_New { get; set; }
        public string  ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    }
    public partial class ProjectList
    {

        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
    }
}