using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Customer
{
    public class BecomeManagerModel : PageModel
    {
        private readonly HotelDbContext _context;

        public BecomeManagerModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Notes { get; set; }

        public bool HasPendingApplication { get; set; }
        public bool IsAlreadyManager { get; set; }
        public ManagerApplication? ExistingApplication { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task OnGetAsync()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

            if (user == null)
            {
                return;
            }

            // Zaten manager mı?
            if (user.Role == UserRole.HotelManager)
            {
                IsAlreadyManager = true;
                return;
            }

            // Bekleyen başvuru var mı?
            ExistingApplication = await _context.ManagerApplications
                .Include(ma => ma.ApprovedByUser)
                .FirstOrDefaultAsync(ma => ma.UserId == CurrentUserId && ma.Status == ApplicationStatus.Pending);

            HasPendingApplication = ExistingApplication != null;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

            if (user == null)
            {
                return NotFound();
            }

            // Zaten manager mı?
            if (user.Role == UserRole.HotelManager)
            {
                return RedirectToPage("/Manager/Hotels/Index");
            }

            // Bekleyen başvuru var mı?
            var existingApplication = await _context.ManagerApplications
                .FirstOrDefaultAsync(ma => ma.UserId == CurrentUserId && ma.Status == ApplicationStatus.Pending);

            if (existingApplication != null)
            {
                TempData["ErrorMessage"] = "Zaten bekleyen bir başvurunuz var.";
                return RedirectToPage();
            }

            // Yeni başvuru oluştur
            var application = new ManagerApplication
            {
                UserId = CurrentUserId,
                Status = ApplicationStatus.Pending,
                ApplicationDate = DateTime.UtcNow,
                Notes = Notes
            };

            _context.ManagerApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Başvurunuz başarıyla gönderildi. Admin onayı bekleniyor.";
            return RedirectToPage();
        }
    }
}

