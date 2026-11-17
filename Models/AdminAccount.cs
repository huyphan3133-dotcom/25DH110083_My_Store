using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace _25DH110083_My_Store.Models
{
    public class AdminAccount
    {
        [Key]
        public int AdminID { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        public string AdminName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string AdminPassword { get; set; }

        [Display(Name = "Email")]
        public string AdminEmail { get; set; }

        [Display(Name = "Quyền hạn")]
        public string AdminRole { get; set; }
    }
}