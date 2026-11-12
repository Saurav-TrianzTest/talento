using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.ViewModel
{
   
    
    public class ResourceDashboardViewModel
    {
        public static List<ProjectAssignments> ProjectAssignments;
       
  

        public ResourceDashboardViewModel(string label, double value1)
        {
            this.Label = label;
            this.Value1 = value1;
        }
        public ResourceDashboardViewModel(double value2)
        {
            this.Value2 = value2;
        }

        internal static object GetLineAreaChartData(List<IGrouping<string, ProjectAssignment>> ProjectAssignment)
        {
            var data = new List<ResourceDashboardViewModel>();
            var EmployeeData = new List<ResourceDashboardViewModel>();

            for (int i = 0; i < ProjectAssignment.Count; i++)
            {
                data.Add(new ResourceDashboardViewModel(ProjectAssignment[i].Key, ProjectAssignment[i].ToList().Count));

            }

            return data;
        }



        public ResourceDashboardViewModel(string label, double value1, double value2)
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
