using System.Linq;
using System.Web.Mvc;
using _25DH110083_My_Store.Models;
using PagedList; // Bắt buộc phải có namespace này

namespace _25DH110083_My_Store.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities(); // Hoặc MyStoreDbContext tùy tên DbContext của bạn

        // GET: Admin/Orders
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SearchString = searchString;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";

            var orders = db.Orders.AsQueryable();

            // Lọc theo từ khóa tìm kiếm (Tên khách hàng hoặc Mã đơn)
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(searchString)
                                        || o.OrderID.ToString().Contains(searchString));
            }

            // Sắp xếp dữ liệu (PagedList bắt buộc phải OrderBy trước)
            switch (sortOrder)
            {
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                default:
                    orders = orders.OrderBy(o => o.OrderID);
                    break;
            }

            int pageSize = 10; // Số lượng dòng trên 1 trang
            int pageNumber = (page ?? 1);

            // Đổi từ .ToList() thành .ToPagedList(pageNumber, pageSize)
            return View(orders.ToPagedList(pageNumber, pageSize));
        }
    }
}