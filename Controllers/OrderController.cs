using _25DH110083_My_Store.Models;
using _25DH110083_My_Store.Models.ViewModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110083_My_Store.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        [HttpGet]
        [Authorize]
        public ActionResult Checkout()
        {
            // Kiểm tra giỏ hàng trong session, nếu rỗng thì chuyển về trang chủ
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Home");

            // Xác thực người dùng đã đăng nhập chưa
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin khách hàng từ CSDL
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Home");

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
                return RedirectToAction("Login", "Account");
            

            // Tính lại dữ liệu ở server, không tin giá trị từ form
            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalValue();
            model.OrderDate = DateTime.Now;
            model.CustomerID = customer.CustomerID;
            model.Username = customer.Username;

            if (!ModelState.IsValid)
                return View(model);

            // Nếu chọn PayPal thì chuyển sang trang thanh toán PayPal
            // (cần làm thêm PayPalController; dữ liệu đơn hàng được giữ trong TempData)
            if (model.PaymentMethod == "Paypal")
            {
                TempData["Checkout"] = model;
                return RedirectToAction("PaymentWithPaypal", "PayPal");
            }

            // Thiết lập PaymentStatus dựa trên PaymentMethod
            string paymentStatus;
            switch (model.PaymentMethod)
            {
                case "Tiền mặt":
                    paymentStatus = "Thanh toán tiền mặt";
                    break;
                case "Paypal":
                    paymentStatus = "Thanh toán paypal";
                    break;
                case "Mua trước trả sau":
                    paymentStatus = "Trả góp";
                    break;
                default:
                    paymentStatus = "Chưa thanh toán";
                    break;
            }

            // Tạo đơn hàng và chi tiết đơn hàng
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = model.OrderDate,
                TotalAmount = model.TotalAmount,
                PaymentStatus = paymentStatus,
                PaymentMethod = model.PaymentMethod,
                ShippingMethod = model.ShippingMethod,
                ShippingAddress = model.ShippingAddress
            };

            // TotalPrice là cột tự động tính trong CSDL, không cần gán ở đây
            foreach (var item in cart.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            // Lưu đơn hàng vào CSDL
            db.Orders.Add(order);
            db.SaveChanges();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            Session["Cart"] = null;

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails")
                                 .SingleOrDefault(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // ================= LỊCH SỬ MUA HÀNG =================

        // Các trạng thái đơn hàng hiển thị thành tab (khớp với trạng thái Admin cập nhật)
        private static readonly string[] OrderStatuses =
            { "Chờ xử lý", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };

        // GET: Order/MyOrder?tab=all&q=...&more=...
        [Authorize]
        public ActionResult MyOrder(string tab = "all", string q = null, string more = null)
        {
            var username = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Index", "Home");

            var query = db.Orders.Include("OrderDetails.Product")
                                 .Where(o => o.CustomerID == customer.CustomerID);

            // Tìm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                int orderId;
                if (int.TryParse(q, out orderId))
                    query = query.Where(o => o.OrderID == orderId ||
                                             o.OrderDetails.Any(d => d.Product.ProductName.Contains(q)));
                else
                    query = query.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(q)));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            // Đơn có trạng thái null hoặc lạ thì coi như "Chờ xử lý"
            Func<Order, string> statusOf = o =>
                OrderStatuses.Contains(o.OrderStatus) ? o.OrderStatus : "Chờ xử lý";

            // Tab "all": hiện cả 5 trạng thái. Tab khác: chỉ hiện trạng thái đó
            var statusesToShow = (tab == "all" || !OrderStatuses.Contains(tab))
                ? OrderStatuses
                : new[] { tab };

            var groups = new List<OrderGroupVM>();
            foreach (var st in statusesToShow)
            {
                var inStatus = orders.Where(o => statusOf(o) == st).ToList();
                if (!inStatus.Any()) continue;

                bool expanded = (more == st);
                groups.Add(new OrderGroupVM
                {
                    Status = st,
                    Total = inStatus.Count,
                    Expanded = expanded,
                    // Mặc định 2 đơn gần nhất; bấm "Xem thêm" thì tối đa 50 đơn
                    Orders = inStatus.Take(expanded ? 50 : 2).ToList()
                });
            }

            return View(new MyOrderVM
            {
                Tab = OrderStatuses.Contains(tab) ? tab : "all",
                Query = q,
                Statuses = OrderStatuses.ToList(),
                Groups = groups
            });
        }

        // POST: Order/BuyAgain/5 (thêm lại các sản phẩm của đơn cũ vào giỏ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult BuyAgain(int id)
        {
            var username = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return HttpNotFound();

            var order = db.Orders.Include("OrderDetails.Product.Category")
                          .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
                return HttpNotFound();

            var cart = new CartService(Session).GetCart();
            foreach (var d in order.OrderDetails)
            {
                // Chỉ thêm khi còn hàng; lấy giá hiện tại của sản phẩm
                if (d.Product.StockQuantity >= 1)
                {
                    cart.AddItem(d.ProductID,
                                 d.Product.ProductImage,
                                 d.Product.ProductName,
                                 d.Product.ProductPrice,
                                 1,
                                 d.Product.Category.CategoryName);
                }
            }

            return RedirectToAction("Index2", "Cart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}