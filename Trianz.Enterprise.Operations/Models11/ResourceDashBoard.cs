using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{

    public class Practice
    { 
        public int Demandno { get; set; }
        public string practiceName { get; set; } 
    } 
    
        public class ChartData
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
            public static List<ChartData> GetData()
            {
                var data = new List<ChartData>();

                data.Add(new ChartData("JAN", 46, 78));
                data.Add(new ChartData("FEB", 35, 72));
                data.Add(new ChartData("MAR", 68, 86));
                data.Add(new ChartData("APR", 30, 23));
                data.Add(new ChartData("MAY", 27, 70));
                data.Add(new ChartData("JUNE", 85, 60));
                data.Add(new ChartData("JULY", 43, 88));
                data.Add(new ChartData("AUG", 29, 22));

                return data;
            }

            public static List<ChartData> GetLineAreaChartData()
            {
                var data = new List<ChartData>();

                data.Add(new ChartData("Practice Name", 56, 62));
                data.Add(new ChartData("Demand", 30, 70));
                data.Add(new ChartData("C", 58, 68));
                data.Add(new ChartData("D", 65, 54));
                data.Add(new ChartData("E", 40, 52));
                data.Add(new ChartData("F", 36, 60));
                data.Add(new ChartData("D", 70, 48));

                return data;
            }

            public static List<ChartData> GetPieChartData(int EmployeeCount, int BenchCount, int DemandCount)
            {
                var data = new List<ChartData>();

                data.Add(new ChartData("Demand", DemandCount));
                data.Add(new ChartData("Total No of Employees", EmployeeCount));
                data.Add(new ChartData("Employees on Bench", BenchCount));


                return data;
            }


            public ChartData(string label, double value1)
            {
                this.Label = label;
                this.Value1 = value1;
            }
            public ChartData(double value2)
            {
            this.Value2 = value2;
            }

        internal static object GetLineAreaChartData(List<IGrouping<string, HRRF>> practiceNames, List<IGrouping<string, Employee>> EmployeePracticeNames)
        {
            var data = new List<ChartData>();
            var EmployeeData = new List<ChartData>();

            for (int i = 0; i < practiceNames.Count; i++)
            {
                data.Add(new ChartData(practiceNames[i].Key,practiceNames[i].ToList().Count,EmployeePracticeNames[i].ToList().Count));

            }
           
            

            //data.Add(new ChartData(practiceNames[0].Key, practiceNames[0])));


            //data.Add(new ChartData("Practice Name", 56, 62));
            //data.Add(new ChartData("Demand", 30, 70));
            //data.Add(new ChartData("C", 58, 68));
            //data.Add(new ChartData("D", 65, 54));
            //data.Add(new ChartData("E", 40, 52));
            //data.Add(new ChartData("F", 36, 60));
            //data.Add(new ChartData("D", 70, 48));

            return data;
        }

        internal static object GetFulFilMentPieChartData(List<IGrouping<string, HRRF>> requestStatus)
        {
            var data = new List<ChartData>();
            var EmployeeData = new List<ChartData>();

            for (int i = 0; i < requestStatus.Count; i++)
            {
                data.Add(new ChartData(requestStatus[i].Key, requestStatus[i].ToList().Count));

            }
            return data;
        }



        public ChartData(string label, double value1, double value2)
            {
                this.Label = label;
                this.Value1 = value1;
                this.Value2 = value2;
            }

            public string Label { get; set; }
            public double Value1 { get; set; }
            public double Value2 { get; set; }
        }

 
  
        // Redis-backed distributed storage for cloud scalability
}