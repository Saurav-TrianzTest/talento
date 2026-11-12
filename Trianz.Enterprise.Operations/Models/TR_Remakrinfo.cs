using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class TR_Remakrinfo
    {
        public string TRNumber { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> RemarkDate { get; set; }
        public string RemarkEmp { get; set; }
    }
}