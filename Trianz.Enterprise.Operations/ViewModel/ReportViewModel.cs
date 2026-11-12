using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class ReportViewModel
    {
        public List<Training> TrainingList { get; set; }      
        public string ProgramName { get; set; }
        public string ProgramId { get; set; }
    }
}