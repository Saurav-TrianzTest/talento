using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Trianz.Enterprise.Operations.Models
{
   

    public class RoleDetails
    {
        public List<RoleMasters> RoleModel { get; set; }
    }

    public class RoleMasters
    {  
        [Key]
        public long? RoleId { get; set; }
        [Required(ErrorMessage = "*Please Enter Employee ID")]
        [Display(Name = "Employee ID")]
        public long? EmployeeId { get; set; }
        [Required(ErrorMessage = "Required"),MaxLength(2)]
        public string Role { get; set; }
        public string EmpName { get; set; }
        public string EmpName1 { get; set; }
        public string Practice { get; set; }
        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CreatedDate { get; set; }  
             
    }
}