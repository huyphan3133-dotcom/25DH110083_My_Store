using System.ComponentModel.DataAnnotations;

namespace _25DH110083_My_Store.Models
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product { }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1.000 đến 1.000.000.000")]
        public decimal ProductPrice { get; set; }

        [Range(0, 100000, ErrorMessage = "Số lượng tồn phải từ 0 trở lên")]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryID { get; set; }
    }
}