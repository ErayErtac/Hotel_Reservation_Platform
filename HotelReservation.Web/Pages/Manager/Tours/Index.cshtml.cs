using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Manager.Tours
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public IList<Tour> Tours { get; set; } = new List<Tour>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Tours = await _context.Tours
                .Where(t => t.ManagerId == CurrentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // TUR SİLME
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id && t.ManagerId == CurrentUserId);

            if (tour == null)
                return NotFound();

            // Bu tura bağlı herhangi bir rezervasyon var mı?
            bool hasBookings = await _context.TourBookings
                .AnyAsync(tb => tb.TourId == id);

            if (hasBookings)
            {
                ErrorMessage = "Bu tura rezervasyon yapılmış. Silmek için önce rezervasyonları iptal etmelisiniz.";
                return RedirectToPage();
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            Message = "Tur başarıyla silindi.";
            return RedirectToPage();
        }
    }
}

