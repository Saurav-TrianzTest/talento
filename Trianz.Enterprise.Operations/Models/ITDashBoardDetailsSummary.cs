using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ITDashBoardDetailsSummary
    {
        public string OrgGroup { get; set; }
        public string OrgSubGroup { get; set; }
        public int TotalCount { get; set; }
        public int Offshore { get; set; }
        public int Onsite { get; set; }
        public int OnsiteGroupbyTotal { get; set; }
        public int GroupbyTotal { get; set; }
        public int OffshoreeGroupbyTotal { get; set; }

    }
    public class ITDashBoardDetailsChart
    {
        public string OrgGroup { get; set; }
        public int Offshore { get; set; }
        public int Onsite { get; set; }
        public int GroupbyTotal { get; set; }
        public int OnsiteGroupbyTotal { get; set; }
        public int OffshoreGroupbyTotal { get; set; }
        
    }
    public class GraphSummaryPieChart
    {
        public string ReqStatus { get; set; }
        public int ReqStatusCount { get; set; }
    }
    public class GraphSummaryPieChartInternal
    {
        public string ReqStatus { get; set; }
        public int ReqStatusCount { get; set; }
    }
}