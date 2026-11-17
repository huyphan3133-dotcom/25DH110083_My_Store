using _25DH110083_My_Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110083_My_Store.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Home()
        {
            // Thống kê tổng số sản phẩm
            ViewBag.TotalProducts = db.Products.Count();

            // Thống kê danh mục
            ViewBag.TotalCategories = db.Categories.Count();

            // Thống kê khách hàng
            ViewBag.TotalCustomers = db.Customers.Count();

            // Thống kê đơn hàng
            ViewBag.TotalOrders = db.Orders.Count();

            // Tính doanh thu từ đơn hàng đã giao
            decimal? totalRevenue = db.Orders
                .Where(o => o.OrderStatus == "Đã giao")
                .Sum(o => (decimal?)o.TotalAmount);

            ViewBag.TotalRevenue = totalRevenue ?? 0m;

            // Lấy 5 đơn hàng mới nhất
            var latestOrders = db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View(latestOrders);
        }

        // Hỗ trợ đường dẫn Index cũ
        public ActionResult Index()
        {
            return RedirectToAction("Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}