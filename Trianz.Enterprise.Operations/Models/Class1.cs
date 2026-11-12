using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class EmployeeMaster
    {
        public Employee employee { get; set; }
        public ProjectAssignment projectAssignment { get; set; }
        public Project project { get; set; }
        public EmployeeDoc empdoc { get; set; }
        public HRRF HRRF { get; set; }
        public EmployeeSkill empskill { get; set; }   
    }
}