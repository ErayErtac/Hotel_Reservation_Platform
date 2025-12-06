using HotelReservation.Core.Entities;
using HotelReservation.Core.Services;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Customer
{
    public class ProfileModel : PageModel
    {
        private readonly HotelDbContext _context;

        public ProfileModel(HotelDbContext context)
        {
            _context = context;
        }

        public AppUser? UserProfile { get; set; }

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
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Değiştirmek istemiyorsanız boş bırakın)")]
        public string? NewPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Yeni Şifre Tekrar")]
        public string? ConfirmNewPassword { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> OnGetAsync(string? message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            UserProfile = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

            if (UserProfile == null)
            {
                return NotFound();
            }

            FullName = UserProfile.FullName;
            Email = UserProfile.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                UserProfile = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == CurrentUserId);
                return Page();
            }

            UserProfile = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

            if (UserProfile == null)
            {
                return NotFound();
            }

            // Check if email is already taken by another user
            if (Email != UserProfile.Email && await _context.Users.AnyAsync(u => u.Email == Email && u.Id != CurrentUserId))
            {
                ModelState.AddModelError(nameof(Email), "Bu e-posta adresi zaten kullanılıyor.");
                return Page();
            }

            try
            {
                UserProfile.FullName = FullName;
                UserProfile.Email = Email;

                // Update password if provided
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    UserProfile.PasswordHash = PasswordHasher.HashPassword(NewPassword);
                }

                await _context.SaveChangesAsync();

                SuccessMessage = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToPage(new { message = "Profil bilgileriniz başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Profil güncellenirken bir hata oluştu: " + ex.Message;
                return Page();
            }
        }
    }
}

