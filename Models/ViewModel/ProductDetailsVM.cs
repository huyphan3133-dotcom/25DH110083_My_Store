using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110083_My_Store.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }

        public int quantity { get; set; } = 1;

        // Tạm tính = quantity * product.ProductPrice
        public decimal estimatedValue { get; set; }

        // Phân trang cho danh sách bán chạy
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 3;

        // 8 sản phẩm cùng danh mục
        public List<Product> RelatedProducts { get; set; }

        // Sản phẩm bán chạy nhất cùng danh mục, đã phân trang
        public PagedList.IPagedList<Product> TopProducts { get; set; }
    }
}