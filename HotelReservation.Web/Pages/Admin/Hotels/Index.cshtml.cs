using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Admin.Hotels
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

        // Kaç otel onay bekliyor?
        public int PendingCount { get; set; }
        
        // Kaç otel yöneticisi başvurusu bekliyor?
        public int PendingManagerApplicationsCount { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        // Filtre: pending / approved / all
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "pending";

        public async Task OnGetAsync()
        {
            // Toplam onay bekleyen sayısı (her zaman lazım)
            PendingCount = await _context.Hotels
                .CountAsync(h => !h.IsApproved);
            
            // Bekleyen otel yöneticisi başvuruları
            PendingManagerApplicationsCount = await _context.ManagerApplications
                .CountAsync(ma => ma.Status == ApplicationStatus.Pending);

            var query = _context.Hotels
                .Include(h => h.Manager)
                .OrderByDescending(h => h.CreatedAt)
                .AsQueryable();

            // Varsayılan: sadece onay bekleyenleri göster
            switch (StatusFilter?.ToLowerInvariant())
            {
                case "approved":
                    query = query.Where(h => h.IsApproved);
                    break;
                case "all":
                    // filtre yok
                    break;
                default: // "pending"
                    StatusFilter = "pending";
                    query = query.Where(h => !h.IsApproved);
                    break;
            }

            Hotels = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsApproved);

            if (hotel == null)
                return NotFound();

            bool hasReservations = await _context.Reservations
                .AnyAsync(r => r.Room.HotelId == id);

            if (hasReservations)
            {
                ErrorMessage = "Bu otel için rezervasyon bulunduğu için silinemez.";
            }
            else
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
                Message = "Otel başarıyla silindi.";
            }

            return RedirectToPage();
        }
    }
}
