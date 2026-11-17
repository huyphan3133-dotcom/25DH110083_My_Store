
using _25DH110083_My_Store.Models;
using _25DH110083_My_Store.Models.ViewModel;
using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MaSV_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Hàm lấy dịch vụ giỏ hàng
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart (giỏ hàng cơ bản, không gom nhóm)
        public ActionResult Index()
        {
            var cart = GetCartService().GetCart();
            return View(cart);
        }

        // GET: Cart/Index2 (giỏ hàng nâng cao, gom nhóm theo danh mục)
        public ActionResult Index2(int? page)
        {
            var cart = GetCartService().GetCart();
            var products = db.Products.ToList();
            var similarProducts = new List<Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                similarProducts = products.Where(p => cart.Items.Any(ci => ci.Category == p.Category.CategoryName)
                                                   && !cart.Items.Any(ci => ci.ProductID == p.ProductID))
                                          .ToList();
            }

            // Đoạn code liên quan tới phân trang
            int pageNumber = page ?? 1;
            cart.PageNumber = pageNumber;

            cart.SimilarProducts = similarProducts
                .OrderBy(p => p.ProductID)
                .ToPagedList(pageNumber, cart.PageSize);

            return View(cart);
        }

        // POST: Cart/AddToCart/5?quantity=2
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            if (quantity < 1) quantity = 1;

            // Tính cả số lượng đã có sẵn trong giỏ để không vượt quá tồn kho
            var cart = GetCartService().GetCart();
            var inCart = cart.Items.Where(i => i.ProductID == id).Sum(i => (int?)i.Quantity) ?? 0;

            if (product.StockQuantity < inCart + quantity)
            {
                // Báo lỗi cho người dùng thay vì im lặng không thêm
                TempData["CartError"] = "Không đủ hàng: kho còn " + product.StockQuantity
                                        + " sản phẩm (trong giỏ đã có " + inCart + ").";
                return RedirectToAction("ProductDetails", "Home", new { id = id });
            }

            cart.AddItem(product.ProductID,
                         product.ProductImage,
                         product.ProductName,
                         product.ProductPrice,
                         quantity,
                         product.Category.CategoryName);

            return RedirectToAction("Index2");
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            GetCartService().GetCart().RemoveItem(id);
            return RedirectToAction("Index2");
        }

        // Làm trống giỏ hàng
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index2");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            GetCartService().GetCart().UpdateQuantity(id, quantity);
            return RedirectToAction("Index2");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}