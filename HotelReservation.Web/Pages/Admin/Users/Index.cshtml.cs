using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<AppUser> Users { get; set; } = new List<AppUser>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public SelectList RoleList { get; set; } = null!;

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task OnGetAsync()
        {
            var query = _context.Users.AsQueryable();

            // Arama
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(u => 
                    u.FullName.Contains(SearchTerm) || 
                    u.Email.Contains(SearchTerm));
            }

            // Rol filtresi
            if (!string.IsNullOrWhiteSpace(RoleFilter) && Enum.TryParse<UserRole>(RoleFilter, out var role))
            {
                query = query.Where(u => u.Role == role);
            }

            // Durum filtresi
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (StatusFilter == "active")
                    query = query.Where(u => u.IsActive);
                else if (StatusFilter == "inactive")
                    query = query.Where(u => !u.IsActive);
            }

            Users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Rol listesi için SelectList
            RoleList = new SelectList(Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new { Value = r.ToString(), Text = GetRoleDisplayName(r) })
                .ToList(), "Value", "Text", RoleFilter);
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            if (id == CurrentUserId)
            {
                ErrorMessage = "Kendi durumunuzu değiştiremezsiniz.";
                return RedirectToPage();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                ErrorMessage = "Kullanıcı bulunamadı.";
                return RedirectToPage();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            Message = $"{user.FullName} kullanıcısının durumu {(user.IsActive ? "aktif" : "pasif")} olarak güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(int id, string newRole)
        {
            if (id == CurrentUserId)
            {
                ErrorMessage = "Kendi rolünüzü değiştiremezsiniz.";
                return RedirectToPage();
            }

            if (!Enum.TryParse<UserRole>(newRole, out var role))
            {
                ErrorMessage = "Geçersiz rol.";
                return RedirectToPage();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                ErrorMessage = "Kullanıcı bulunamadı.";
                return RedirectToPage();
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            Message = $"{user.FullName} kullanıcısının rolü {GetRoleDisplayName(role)} olarak güncellendi.";
            return RedirectToPage();
        }

        private string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Admin",
                UserRole.HotelManager => "Otel Yöneticisi",
                UserRole.Customer => "Müşteri",
                _ => role.ToString()
            };
        }
    }
}

