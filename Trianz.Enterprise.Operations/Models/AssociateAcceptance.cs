using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class AssociateAcceptance
    {
        public int HRRFID { get; set; }
        public string ProjectName { get; set; }
        public string Role { get; set; }
        public string AccountName { get; set; }
        public string ResourceExpectedDate { get; set; }
        public string AssignmentStartDate { get; set; }
        public string AssignmentEndDate { get; set; }
        public string LocationType { get; set; }
        public string OffshopreLocation { get; set; }
        public string OnsiteLocation { get; set; }
        public string VisaType { get; set; }
        public string ShiftInfo { get; set; }
        public string TravelInfo { get; set; }
        public string PrimarySkills { get; set; }
        public string SecondarySkills { get; set; }
        public string JobDescription { get; set; }
        public string AssociateComment { get; set; }
    }
}