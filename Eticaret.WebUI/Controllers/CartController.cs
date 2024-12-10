using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Eticaret.WebUI.ExtensionMethods;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _serviceProduct;
        private readonly IService<Address> _serviceAddress;
        private readonly IService<AppUser> _serviceAppUser;
        private readonly IService<Order> _serviceOrder;

        public CartController(IService<Product> serviceProduct, IService<Address> serviceAddress, IService<AppUser> serviceAppUser, IService<Order> serviceOrder)
        {
            _serviceProduct = serviceProduct;
            _serviceAddress = serviceAddress;
            _serviceAppUser = serviceAppUser;
            _serviceOrder = serviceOrder;
        }
        public IActionResult Index()
        {
            var cart = GetCart();
            var model = new CartViewModel()
            {
                CartLines = cart.CartLines,
                TotalPrice = cart.TotalPrice()
            };
            return View(model);
        }
        public IActionResult Add(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            return RedirectToAction("Index");
        }
        public IActionResult Update(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.UpdateProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int ProductId)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.RemoveProduct(product);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckoutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Addresses = addresses
            };
            return View(model);
        }
        [Authorize, HttpPost]
        public async Task<IActionResult> Checkout(string CardNumber, string CardMonth, string CardYear, string CVV, string DeliveryAddress, string BillingAddress)
        {
            var cart = GetCart();
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckoutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Addresses = addresses
            };
            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardMonth) || string.IsNullOrWhiteSpace(CardYear) || string.IsNullOrWhiteSpace(CVV) || string.IsNullOrWhiteSpace(DeliveryAddress) || string.IsNullOrWhiteSpace(BillingAddress))
            {
                return View(model);
            }
            var faturaAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == BillingAddress);
            var teslimatAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == DeliveryAddress);

            // Ödeme çekme

            var siparis = new Order
            {
                AppUserId = appUser.Id,
                BillingAddress = $"{faturaAdresi.OpenAddress} {faturaAdresi.District} {faturaAdresi.City}",// BillingAddress,
                DeliveryAddress = $"{faturaAdresi.OpenAddress} {faturaAdresi.District} {faturaAdresi.City}",//DeliveryAddress,
                CustomerId = appUser.UserGuid.ToString(),
                OrderDate = DateTime.Now,
                TotalPrice = cart.TotalPrice(),
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = 0,
                OrderLines = []
            };

            foreach (var item in cart.CartLines)
            {
                siparis.OrderLines.Add(new OrderLine
                {
                    ProductId = item.Product.Id,
                    OrderId = siparis.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });
            }

            try
            {
                await _serviceOrder.AddAsync(siparis);
                var sonuc = await _serviceOrder.SaveChangesAsync();
                if (sonuc > 0)
                {
                    HttpContext.Session.Remove("Cart");
                    return RedirectToAction("Thanks");
                }
            }
            catch (Exception)
            {
                TempData["Message"] = "Hata Oluştu!";
            }

            return View(model);
        }
        public IActionResult Thanks()
        {
            return View();
        }
        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }

    }
}
