using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class BenchResourceStatusReport
    {
        public string EMPLOYEENAME { get; set; }
        public int GRADE { get; set; }
        public int EMPLOYEEID { get; set; }
        public DateTime ACTUALSTARTDATE { get; set; }
        public Nullable<DateTime> STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public DateTime CURRENTDATE { get; set; }
        public Nullable<int> TOTALDAYS { get; set; }
        public Nullable<int> TOTALHOURS { get; set; }
        public int UTILIZATION { get; set; }
        public bool ISACTIVE { get; set; }
        public String PROJECTCODE { get; set; }
        //public Double TOTALCOST { get; set; }
      
        public Nullable<Double> TOTALCOST { get; set; }
        public string CATEGORY { get; set; }
    }
    public class BenchTotalCostReport
    {
        public string PROJECTCODE { get; set; }
        public string PROJECTNAME { get; set; }
        public Nullable<DateTime> STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public Nullable<int> DEPLOYABLECOUNT { get; set; }
        public Nullable<int> NONDEPLOYABLECOUNT { get; set; }
        public Nullable<int> RESERVEDCOUNT { get; set; }
        public Nullable<int> HEADCOUNT { get; set; }
        public Nullable<int> TOTALDAYS { get; set; }
        public Nullable<int> TOTALHOURS { get; set; }
      
        public Double TOTALCOST { get; set; }        
    }
    public class BenchCostReport
    {
        public string PROJECTCODE { get; set; }
        public string PROJECTNAME { get; set; }
        public Nullable<DateTime> STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string CATEGORY { get; set; }
        public Nullable<int> HEADCOUNT { get; set; }
        public Nullable<int> TOTALDAYS { get; set; }
        public Nullable<int> TOTALHOURS { get; set; }
      
        public Double TOTALCOST { get; set; }
    }
}