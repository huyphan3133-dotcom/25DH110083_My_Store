using _25DH110083_My_Store.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace _25DH110083_My_Store.Areas.Admin.Controllers
{
    public class AdminAccountsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        

        // GET: Admin/AdminAccounts/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Admin/AdminAccounts/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                // Tìm tài khoản trong DB
                var admin = db.AdminAccounts
                    .FirstOrDefault(a => a.AdminName == username && a.AdminPassword == password);

                if (admin != null)
                {
                    // Lưu thông tin vào Session
                    Session["AdminID"] = admin.AdminID;
                    Session["AdminName"] = admin.AdminName;
                    Session["AdminRole"] = admin.AdminRole;

                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            return View();
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Login", "AdminAccounts", new { area = "Admin" });
        }

        // =========================================================
        // 2. CRUD TÀI KHOẢN ADMIN
        // =========================================================

        // Danh sách (Tìm kiếm + Phân trang)
        public ActionResult Index(string searchString, int? page)
        {
            var accounts = db.AdminAccounts.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                accounts = accounts.Where(a => a.AdminName.Contains(searchString)
                                            || a.AdminEmail.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            const int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(accounts.OrderBy(a => a.AdminName).ToPagedList(pageNumber, pageSize));
        }

        // Xem chi tiết
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            AdminAccount adminAccount = db.AdminAccounts.Find(id);
            if (adminAccount == null) return HttpNotFound();
            return View(adminAccount);
        }

        // Tạo mới (GET)
        public ActionResult Create()
        {
            return View();
        }

        // Tạo mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdminID,AdminName,AdminPassword,AdminEmail,AdminRole")] AdminAccount adminAccount)
        {
            if (ModelState.IsValid)
            {
                db.AdminAccounts.Add(adminAccount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(adminAccount);
        }

        // Cập nhật (GET) - Tối ưu gọi lại Details
        public ActionResult Edit(int? id)
        {
            return View(Details(id));
        }

        // Cập nhật (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AdminID,AdminName,AdminPassword,AdminEmail,AdminRole")] AdminAccount adminAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(adminAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(adminAccount);
        }

        // Xóa (GET) - Tối ưu gọi lại Details
        public ActionResult Delete(int? id)
        {
            return View(Details(id));
        }

        // Xóa (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AdminAccount adminAccount = db.AdminAccounts.Find(id);
            db.AdminAccounts.Remove(adminAccount);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

}