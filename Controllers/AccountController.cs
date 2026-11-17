using _25DH110083_My_Store.Models;
using _25DH110083_My_Store.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110083_My_Store.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // ================= REGISTER =================

        // GET: Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                return View(model);
            }

            // Tạo bản ghi trong bảng User
            // Lưu ý: nên mã hóa mật khẩu trước khi lưu (xem ghi chú cuối)
            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                UserRole = "C"   // cột UserRole là NCHAR(1): A = admin, C = customer
            };
            db.Users.Add(user);

            // Tạo bản ghi trong bảng Customer
            var customer = new Customer
            {
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                CustomerAddress = model.CustomerAddress,
                Username = model.Username
            };
            db.Customers.Add(customer);

            // BẮT BUỘC: lưu vào database
            db.SaveChanges();

            // Đăng nhập luôn sau khi đăng ký
            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole;
            FormsAuthentication.SetAuthCookie(user.Username, false);

            return RedirectToAction("ProfileInfo", "Account");
        }

        // ================= LOGIN =================

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.CustomerPassword
                                                      && u.UserRole == "C");
                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    // Lưu thông tin xác thực vào cookie (false = không lưu cookie lâu dài)
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("ProfileInfo", "Account");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        // ================= LOGOUT =================

        // GET: Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ================= PROFILE INFO =================

        // GET: Account/ProfileInfo
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var username = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return HttpNotFound();

            var vm = new ProfileVM
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            };
            return View(vm);
        }

        // POST: Account/UpdateProfile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            var username = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return HttpNotFound();

            // Tên đăng nhập không được sửa từ form nên bỏ qua validation của trường này
            ModelState.Remove("Username");

            if (!ModelState.IsValid)
            {
                model.Username = username;
                return View("ProfileInfo", model);
            }

            customer.CustomerName = model.CustomerName;
            customer.CustomerPhone = model.CustomerPhone;
            customer.CustomerEmail = model.CustomerEmail;
            customer.CustomerAddress = model.CustomerAddress;
            db.SaveChanges();

            TempData["Message"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {

            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return HttpNotFound();

            // Cột Password là NCHAR(50) nên bị đệm khoảng trắng phía sau -> phải Trim()
            if (user.Password.Trim() != oldPassword)
            {
                TempData["Error"] = "Mật khẩu cũ không đúng.";
            }
            else if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
            }
            else if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Xác nhận mật khẩu không khớp.";
            }
            else
            {
                user.Password = newPassword;
                db.SaveChanges();
                TempData["Message"] = "Đổi mật khẩu thành công!";
            }


            return RedirectToAction("ProfileInfo");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}