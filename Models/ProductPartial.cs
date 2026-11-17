using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _25DH110083_My_Store.Models
{
    public partial class Product
    {
        [ImageFile(ErrorMessage = "Chỉ chấp nhận ảnh PNG, JPG, JPEG, GIF, WEBP")]
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}