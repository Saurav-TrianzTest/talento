using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
     public partial class SkillMasterinfo
        {
            public int SkillId { get; set; }
            public string SkillCategory { get; set; }
            public string Skillset { get; set; }
            public string Practice { get; set; }
            public Nullable<bool> IsActive { get; set; }
        }
    }
