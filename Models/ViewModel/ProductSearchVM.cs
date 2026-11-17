using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110083_My_Store.Models.ViewModel
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }

        public decimal ? MinPrice { get; set; }

        public string SortOrder { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số sản phẩm mỗi trang

        // Danh sách Sản phẩm đã phân trang
        public IPagedList<Product> Products { get; set; }


    }
}