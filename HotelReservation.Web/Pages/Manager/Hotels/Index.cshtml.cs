using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace HotelReservation.Web.Pages.Manager.Hotels
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

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Hotels = await _context.Hotels
                .Where(h => h.ManagerId == CurrentUserId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }


        // OTEL SÝLME
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == id && h.ManagerId == CurrentUserId);

            if (hotel == null)
                return NotFound();

            // Bu otele baðlý herhangi bir rezervasyon var mý?
            bool hasReservations = await _context.Reservations
                .AnyAsync(r => r.Room.HotelId == id);

            if (hasReservations)
            {
                ErrorMessage = "Bu otele ait rezervasyonlar bulunduðu için otel silinemez.";
            }
            else
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
                Message = "Otel baþarýyla silindi.";
            }

            return RedirectToPage();
        }
    }
}
