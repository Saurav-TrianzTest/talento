using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Trianz.Enterprise.Operations.Models
{
    [MetadataType(typeof(MetaData))]
    public partial class DesignationMaster { }
    
        public class MetaData
        {
            public long DesignationID { get; set; }
            [Required(ErrorMessage = "Required Field")]
            [RegularExpression("^[a-zA-Z][a-zA-Z0-9\\-_]{0,20}$", ErrorMessage ="Please Enter AlphaNumeric characters Only")]
            public string DesignationCode { get; set; }
            [Required(ErrorMessage = "Required Field")]
            //[RegularExpression("^[a-zA-Z][a-zA-Z\\s]*$", ErrorMessage = "Please Enter Alphabetic letters Only")]
            public string DesignationName { get; set; }
            [Required(ErrorMessage = "Required Field")]
            public string Practice { get; set; }
            [Required(ErrorMessage = "Required Field")]
            public int Grade { get; set; }
            public string JobDescription { get; set; }
            public Nullable<int> CreatedBy { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }
            public Nullable<int> ModifiedBy { get; set; }
            public Nullable<System.DateTime> ModifiedDate { get; set; }
        }

}