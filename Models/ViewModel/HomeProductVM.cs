using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110083_My_Store.Models.ViewModel
{
    public class HomeProductVM
    {
        public string SearchTerm { get; set; }

        // Thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        // 10 sản phẩm bán chạy nhất
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới đã phân trang
        public IPagedList<Product> NewProducts { get; set; }
    }
}