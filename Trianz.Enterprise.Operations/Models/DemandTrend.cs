using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class DemandData
    {
        public static List<HRRF> allHRRFS;
        public static List<Employee> Employees;
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Practice { get; set; }
        public string HRRFNumber { get; set; }
        public string JobDescription { get; set; }
        public string SubmittedDate { get; set; }


        public static List<DemandData> GetData()
        {
            var data = new List<DemandData>();

            data.Add(new DemandData("JAN", 10,9));
            data.Add(new DemandData("FEB", 15,10));
            data.Add(new DemandData("MAR", 18,14));
            data.Add(new DemandData("APR", 20,15));
            data.Add(new DemandData("MAY", 22,18));
            data.Add(new DemandData("JUNE", 25,10));
            data.Add(new DemandData("JUL", 30,15));
            data.Add(new DemandData("AUG", 33,18));
            data.Add(new DemandData("SEP", 15,14));
            data.Add(new DemandData("OCT", 40,29));
            data.Add(new DemandData("NOV", 32,22));
            data.Add(new DemandData("DEC", 25,27));

            return data;
        }

        

        public static List<DemandData> GetPieChartData(int TotalDemands, int InternalDemands, int ExternalContractDemands, int ExternalContracttoHireDemands, int ExternalRecruitDemands)
        {
            var data = new List<DemandData>();

            //data.Add(new DemandData("Draft", intdraft));
            //data.Add(new DemandData("Submitted", intSubmitted));
            //data.Add(new DemandData("Request More Info", Moreinfo));
            //data.Add(new DemandData("Qualified", intQualified));
            //data.Add(new DemandData("Converted to External", intExternal));

            data.Add(new DemandData("Total No Of Demands", TotalDemands));
            data.Add(new DemandData("Internal Fullfilled", InternalDemands));
            data.Add(new DemandData("External Fullfilled", ExternalContractDemands));
            data.Add(new DemandData("External-Contract to Hire", ExternalContracttoHireDemands));
            data.Add(new DemandData("External-Recruit", ExternalRecruitDemands));

            return data;
        }

        public DemandData(string label, double value1)
        {
            this.Label = label;
            this.Value1 = value1;
            
        }
        public DemandData(double value2)
        {
            this.Value2 = value2;
        }

        public DemandData(string label, double value1, double value2)
        {
            this.Label = label;
            this.Value1 = value1;
            this.Value2 = value2;
        }
        public DemandData(string label, double value1, double value2, double value3, double value4)
        {
            this.Label = label;
            this.Value1 = value1;
            this.Value2 = value2;
            this.Value3= value3;
            this.Value4 = value4;
            
        }
        
        public class monthdemandtest
        {
            public string Month { get; set; }
            public int FullFilledRequestCount { get; set; }
            public int SubmittedRequestCount { get; set; }
            public int QualifiedRequestCount { get; set; }
            public int DraftRequestCount { get; set; }
        }
        public class RecordList
        {
            public string Month { get; set; }
            public int count { get; set; }
            public string FullFilled { get; set; }
            public string Submitted { get; set; }
            public string Draft { get; set; }
            public string Qualified { get; set; }
        }

        internal static object GetLineAreaChartDataMonth(List<monthdemandtest> monthlyDemand)
        {
           
            var data = new List<DemandData>();
            
            for (int i = 0; i < monthlyDemand.Count; i++)
            {
               
                data.Add(new DemandData(Convert.ToString(monthlyDemand[i].Month), monthlyDemand[i].SubmittedRequestCount,monthlyDemand[i].DraftRequestCount,monthlyDemand[i].QualifiedRequestCount,monthlyDemand[i].FullFilledRequestCount));

            }

            return data;
        }
        public string Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
        public double Value4 { get; set; }
        


    }
}