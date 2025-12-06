using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Core.Services;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly HotelDbContext _context;

        public RegisterModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == Email))
            {
                ModelState.AddModelError(nameof(Email), "Bu e-posta adresi zaten kullanılıyor.");
                return Page();
            }

            try
            {
                var user = new AppUser
                {
                    FullName = FullName,
                    Email = Email,
                    PasswordHash = PasswordHasher.HashPassword(Password),
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                SuccessMessage = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToPage("/Account/Login", new { message = "Kayıt başarılı! Giriş yapabilirsiniz." });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Kayıt sırasında bir hata oluştu: " + ex.Message;
                return Page();
            }
        }
    }
}

