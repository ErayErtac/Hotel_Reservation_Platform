using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Admin.Tours
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Tour> Tours { get; set; } = new List<Tour>();

        // Kaç tur onay bekliyor?
        public int PendingCount { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        // Filtre: pending / approved / all
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "pending";

        public async Task OnGetAsync()
        {
            // Toplam onay bekleyen sayısı
            PendingCount = await _context.Tours
                .CountAsync(t => !t.IsApproved);

            var query = _context.Tours
                .Include(t => t.Manager)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            // Varsayılan: sadece onay bekleyenleri göster
            switch (StatusFilter?.ToLowerInvariant())
            {
                case "approved":
                    query = query.Where(t => t.IsApproved);
                    break;
                case "all":
                    // filtre yok
                    break;
                default: // "pending"
                    StatusFilter = "pending";
                    query = query.Where(t => !t.IsApproved);
                    break;
            }

            Tours = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            tour.IsApproved = true;
            await _context.SaveChangesAsync();

            Message = "Tur başarıyla onaylandı.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsApproved);

            if (tour == null)
                return NotFound();

            bool hasBookings = await _context.TourBookings
                .AnyAsync(tb => tb.TourId == id);

            if (hasBookings)
            {
                ErrorMessage = "Bu tur için rezervasyon bulunduğu için silinemez.";
            }
            else
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
                Message = "Tur başarıyla reddedildi ve silindi.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsApproved);

            if (tour == null)
                return NotFound();

            bool hasBookings = await _context.TourBookings
                .AnyAsync(tb => tb.TourId == id);

            if (hasBookings)
            {
                ErrorMessage = "Bu tur için rezervasyon bulunduğu için silinemez.";
            }
            else
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
                Message = "Tur başarıyla silindi.";
            }

            return RedirectToPage();
        }
    }
}

