using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelReservation.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly HotelDbContext _context;

        public LoginModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Sadece formu gösteriyoruz
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "E-posta veya şifre hatalı.";
                return Page();
            }

            // Şifre doğrulama
            bool isPasswordValid = HotelReservation.Core.Services.PasswordHasher.VerifyPassword(Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                ErrorMessage = "E-posta veya şifre hatalı.";
                return Page();
            }

            // Eğer şifre eski formatta (plain text) ise, hash'le ve güncelle
            if (!HotelReservation.Core.Services.PasswordHasher.IsBcryptHash(user.PasswordHash))
            {
                user.PasswordHash = HotelReservation.Core.Services.PasswordHasher.HashPassword(Password);
                await _context.SaveChangesAsync();
            }

            // Rol string'ini enum'dan üretelim (UserRole enum'ının ToString() değeri: Admin / HotelManager / Customer)
            var roleName = user.Role.ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Role'ye göre yönlendirme
            return roleName switch
            {
                "Admin" => RedirectToPage("/Admin/Hotels/Index"),
                "HotelManager" => RedirectToPage("/Manager/Hotels/Index"),
                "Customer" => RedirectToPage("/Index"),
                _ => RedirectToPage("/Index")
            };
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}
