using _25DH110083_My_Store.Models;
using _25DH110083_My_Store.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25DH110083_My_Store.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm theo tên, mô tả hoặc tên danh mục
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                            || p.ProductDescription.Contains(searchTerm)
                                            || p.Category.CategoryName.Contains(searchTerm));
            }

            // Phân trang: mặc định trang 1, 6 sản phẩm mỗi trang
            int pageNumber = page ?? 1;
            int pageSize = 6;
            model.PageNumber = pageNumber;
            model.PageSize = pageSize;

            // Top 10 sản phẩm bán chạy nhất
            model.FeaturedProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .Take(10)
                .ToList();

            // 20 sản phẩm đã phân trang
            model.NewProducts = products
                .OrderBy(p => p.OrderDetails.Count())
                .Take(20)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductList/3  (danh sách sản phẩm theo danh mục)
        public ActionResult ProductList(int id, int? page)
        {
            var category = db.Categories.Find(id);
            if (category == null)
                return HttpNotFound();

            int pageNumber = page ?? 1;
            int pageSize = 6;

            var products = db.Products
                             .Where(p => p.CategoryID == id)
                             .OrderBy(p => p.ProductID)
                             .ToPagedList(pageNumber, pageSize);

            ViewBag.CategoryID = id;
            ViewBag.CategoryName = category.CategoryName;
            return View(products);
        }

        // GET: Home/ProductDetails/5?quantity=2&page=1
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product pro = db.Products.Find(id);
            if (pro == null)
                return HttpNotFound();

            var model = new ProductDetailsVM();
            model.product = pro;

            // Các sản phẩm cùng danh mục, trừ sản phẩm đang xem
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID
                                               && p.ProductID != pro.ProductID);

            // Phân trang cho danh sách bán chạy
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.RelatedProducts = products
                .OrderBy(p => p.ProductID)
                .Take(8)
                .ToList();

            model.TopProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .Take(8)
                .ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue && quantity.Value >= 1)
                model.quantity = quantity.Value;

            model.estimatedValue = model.quantity * pro.ProductPrice;

            return View(model);
        }

        // Action con, dùng cho menu Category trong _CustomerLayout
        [ChildActionOnly]
        public PartialViewResult CategoriesMenu()
        {
            var categories = db.Categories.ToList();
            return PartialView("_CategoriesPV", categories);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}