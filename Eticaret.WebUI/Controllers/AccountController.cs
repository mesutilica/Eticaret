using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Eticaret.WebUI.Utils;
using Microsoft.AspNetCore.Authentication; // login
using Microsoft.AspNetCore.Authorization; // login
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims; // login

namespace Eticaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        //private readonly DatabaseContext _context;

        //public AccountController(DatabaseContext context)
        //{
        //    _context = context;
        //}
        private readonly IService<AppUser> _service;
        private readonly IService<Order> _serviceOrder;

        public AccountController(IService<AppUser> service, IService<Order> serviceOrder)
        {
            _service = service;
            _serviceOrder = serviceOrder;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (user is null)
            {
                return NotFound();
            }
            var model = new UserEditViewModel()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                Password = user.Password,
                Phone = user.Phone,
                Surname = user.Surname
            };
            return View(model);
        }
        [HttpPost, Authorize]
        public async Task<IActionResult> IndexAsync(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
                    if (user is not null)
                    {
                        user.Surname = model.Surname;
                        user.Phone = model.Phone;
                        user.Name = model.Name;
                        user.Password = model.Password;
                        user.Email = model.Email;
                        _service.Update(user);
                        var sonuc = _service.SaveChanges();

                        if (sonuc > 0)
                        {
                            TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                        <strong>Bilgileriniz Güncellenmiştir!</strong>
    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
    </div>";
                            // await MailHelper.SendMailAsync(contact);
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Hata Oluştu!");
                }
            }
            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (user is null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("SignIn");
            }
            var model = _serviceOrder.GetQueryable().Where(s => s.AppUserId == user.Id).Include(o => o.OrderLines).ThenInclude(p => p.Product);
            return View(model);
        }
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignInAsync(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var account = await _service.GetAsync(x => x.Email == loginViewModel.Email & x.Password == loginViewModel.Password & x.IsActive);
                    if (account == null)
                    {
                        ModelState.AddModelError("", "Giriş Başarısız!");
                    }
                    else
                    {
                        var claims = new List<Claim>()
                        {
                            new(ClaimTypes.Name, account.Name),
                            new(ClaimTypes.Role, account.IsAdmin ? "Admin" : "Customer"),
                            new(ClaimTypes.Email, account.Email),
                            new("UserId", account.Id.ToString()),
                            new("UserGuid", account.UserGuid.ToString()),
                        };
                        var userIdentity = new ClaimsIdentity(claims, "Login");
                        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);
                        await HttpContext.SignInAsync(userPrincipal);
                        return Redirect(string.IsNullOrEmpty(loginViewModel.ReturnUrl) ? "/" : loginViewModel.ReturnUrl);
                    }
                }
                catch (Exception hata)
                {
                    // loglama
                    ModelState.AddModelError("", "Hata Oluştu!");
                }
            }
            return View(loginViewModel);
        }
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUpAsync(AppUser appUser)
        {
            appUser.IsAdmin = false;
            appUser.IsActive = true;
            if (ModelState.IsValid)
            {
                await _service.AddAsync(appUser);
                await _service.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignIn");
        }
        public IActionResult PasswordRenew()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PasswordRenewAsync(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Email Boş Geçilemez!");
                return View();
            }
            AppUser user = await _service.GetAsync(x => x.Email == Email);
            if (user is null)
            {
                ModelState.AddModelError("", "Girdiğiniz Email Bulunamadı!");
                return View();
            }
            string mesaj = $"Sayın {user.Name} {user.Surname} <br> Şifrenizi Yenilemek İçin Lütfen <a href='https://localhost:7239/Account/PasswordChange?user={user.UserGuid.ToString()}'>Buraya Tıklayınız</a>";
            var sonuc = await MailHelper.SendMailAsync(Email, "Şifremi Yenile", mesaj);
            if (sonuc)
            {
                TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                        <strong>Şifre Sıfırlama Bağlantınız Mail Adresinize Gönderilmiştir!</strong>
    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
    </div>";
            }
            else
            {
                TempData["Message"] = @"<div class=""alert alert-danger alert-dismissible fade show"" role=""alert"">
                        <strong>Şifre Sıfırlama Bağlantınız Mail Adresinize Gönderilemedi!</strong>
    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
    </div>";
            }
            return View();
        }
        public async Task<IActionResult> PasswordChangeAsync(string user)
        {
            if (user is null)
            {
                return BadRequest("Geçersiz İstek!");
            }
            AppUser appUser = await _service.GetAsync(x => x.UserGuid.ToString() == user);
            if (appUser is null)
            {
                return NotFound("Geçersiz Değer!");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PasswordChange(string user, string Password)
        {
            if (user is null)
            {
                return BadRequest("Geçersiz İstek!");
            }
            AppUser appUser = await _service.GetAsync(x => x.UserGuid.ToString() == user);
            if (appUser is null)
            {
                ModelState.AddModelError("", "Geçersiz Değer!");
                return View();
            }
            appUser.Password = Password;
            var sonuc = await _service.SaveChangesAsync();
            if (sonuc > 0)
            {
                TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                        <strong>Şifreniz Güncellenmiştir! Giriş Ekranından Oturum Açabilirsiniz.</strong>
    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
    </div>";
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme Başarısız!");
            }
            return View();
        }
    }
}
