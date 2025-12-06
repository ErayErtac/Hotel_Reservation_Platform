using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Admin.ManagerApplications
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<ManagerApplication> Applications { get; set; } = new List<ManagerApplication>();
        public int PendingCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task OnGetAsync()
        {
            PendingCount = await _context.ManagerApplications
                .CountAsync(ma => ma.Status == ApplicationStatus.Pending);

            var query = _context.ManagerApplications
                .Include(ma => ma.User)
                .Include(ma => ma.ApprovedByUser)
                .OrderByDescending(ma => ma.ApplicationDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                switch (StatusFilter.ToLowerInvariant())
                {
                    case "approved":
                        query = query.Where(ma => ma.Status == ApplicationStatus.Approved);
                        break;
                    case "rejected":
                        query = query.Where(ma => ma.Status == ApplicationStatus.Rejected);
                        break;
                    case "pending":
                    default:
                        query = query.Where(ma => ma.Status == ApplicationStatus.Pending);
                        StatusFilter = "pending";
                        break;
                }
            }
            else
            {
                StatusFilter = "pending";
                query = query.Where(ma => ma.Status == ApplicationStatus.Pending);
            }

            Applications = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            try
            {
                var application = await _context.ManagerApplications
                    .Include(ma => ma.User)
                    .FirstOrDefaultAsync(ma => ma.Id == id);

                if (application == null)
                {
                    ErrorMessage = "Başvuru bulunamadı.";
                    return RedirectToPage();
                }

                if (application.Status != ApplicationStatus.Pending)
                {
                    ErrorMessage = "Bu başvuru zaten işleme alınmış.";
                    return RedirectToPage();
                }

                // Kullanıcının rolünü HotelManager yap
                application.User.Role = UserRole.HotelManager;
                application.Status = ApplicationStatus.Approved;
                application.ApprovedDate = DateTime.UtcNow;
                application.ApprovedByUserId = CurrentUserId;

                await _context.SaveChangesAsync();

                Message = $"Başvuru onaylandı. {application.User.FullName} artık otel yöneticisi oldu.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Başvuru onaylanırken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? rejectionReason)
        {
            try
            {
                var application = await _context.ManagerApplications
                    .Include(ma => ma.User)
                    .FirstOrDefaultAsync(ma => ma.Id == id);

                if (application == null)
                {
                    ErrorMessage = "Başvuru bulunamadı.";
                    return RedirectToPage();
                }

                if (application.Status != ApplicationStatus.Pending)
                {
                    ErrorMessage = "Bu başvuru zaten işleme alınmış.";
                    return RedirectToPage();
                }

                application.Status = ApplicationStatus.Rejected;
                application.RejectedDate = DateTime.UtcNow;
                application.ApprovedByUserId = CurrentUserId;
                application.RejectionReason = rejectionReason;

                await _context.SaveChangesAsync();

                Message = $"Başvuru reddedildi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Başvuru reddedilirken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}

