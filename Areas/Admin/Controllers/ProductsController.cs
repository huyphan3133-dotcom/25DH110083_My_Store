using _25DH110083_My_Store.Models;
using _25DH110083_My_Store.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25DH110083_My_Store.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products (Danh sách + Tìm kiếm + Phân trang)
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    (p.ProductDescription != null && p.ProductDescription.Contains(searchTerm)) ||
                    (p.Category != null && p.Category.CategoryName.Contains(searchTerm)));
            }

            // Tìm kiếm sản phẩm dựa trên giá tối thiểu
            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice.Value;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }

            // Tìm kiếm sản phẩm dựa trên giá tối đa
            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice.Value;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // Áp dụng sắp xếp dựa trên lựa chọn của người dùng
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default: // Mặc định sắp xếp theo tên
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }
            model.SortOrder = sortOrder;

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = 2; // Số sản phẩm mỗi trang (có thể điều chỉnh tùy chọn)

            // Sử dụng ToPagedList để lấy danh sách đã phân trang
            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            // Mặc định tồn kho 100 (giống DEFAULT trong database)
            return View(new Product { StockQuantity = 100 });
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    // Lấy tên file
                    string fileName = System.IO.Path.GetFileName(product.UploadImg.FileName);
                    // Đường dẫn vật lý để lưu file
                    string folder = Server.MapPath("~/Content/images/");
                    if (!System.IO.Directory.Exists(folder))
                        System.IO.Directory.CreateDirectory(folder);
                    string path = System.IO.Path.Combine(folder, fileName);
                    // Lưu file
                    product.UploadImg.SaveAs(path);
                    // Lưu đường dẫn dạng "/Content/images/xxx.jpg" để thẻ <img src="@product.ProductImage"> hiển thị được
                    product.ProductImage = Url.Content("~/Content/images/" + fileName);
                }
                // Không upload → giữ nguyên link người dùng gõ vào ô "Hình ảnh" (nếu có)

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                // Lấy sản phẩm hiện tại trong DB
                var existing = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);

                // Nếu có upload ảnh mới → lưu file mới
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(product.UploadImg.FileName);
                    string folder = Server.MapPath("~/Content/images/");
                    if (!System.IO.Directory.Exists(folder))
                        System.IO.Directory.CreateDirectory(folder);
                    product.UploadImg.SaveAs(System.IO.Path.Combine(folder, fileName));
                    product.ProductImage = Url.Content("~/Content/images/" + fileName);
                }
                else if (string.IsNullOrWhiteSpace(product.ProductImage) && existing != null)
                {
                    // Ô "Hình ảnh" để trống và không upload → giữ ảnh cũ
                    product.ProductImage = existing.ProductImage;
                }
                // Ngược lại: dùng link mới mà người dùng đã gõ vào ô "Hình ảnh"

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }
        
        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Search(string searchTerm)
        {
            var model = new ProductSearchVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Tìm kiếm sản phẩm dựa trên Tên sản phẩm, Mô tả hoặc Tên danh mục
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    (p.ProductDescription != null && p.ProductDescription.Contains(searchTerm)) ||
                    (p.Category != null && p.Category.CategoryName.Contains(searchTerm)));
            }

            model.SearchTerm = searchTerm;
            model.Products = (IPagedList<Product>)products.ToList();

            return View(model);
        }
    }
}
