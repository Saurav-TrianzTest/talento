using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class TRCommentinfo
    {
        public string TRNumber { get; set; }
        public string Comments { get; set; }
        public Nullable<System.DateTime> CommentDate { get; set; }
        public string CommentedEmp { get; set; }
    }
}