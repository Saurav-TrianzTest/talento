using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class LookupMaster
    {

        public string ApplicationCode { get; set; }
        public string Lookupcode { get; set; }
        public string LookupName { get; set; }
        public string Description { get; set; }
        public int? SeqNumber { get; set; }
        public int? ParentCode { get; set; }
        public string ParentName { get; set; }
        public string Active { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateMdified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}