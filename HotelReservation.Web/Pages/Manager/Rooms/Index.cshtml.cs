using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Rooms
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

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        public Hotel? Hotel { get; set; }
        public IList<Room> Rooms { get; set; } = new List<Room>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int hotelId)
        {
            HotelId = hotelId;

            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.ManagerId == CurrentUserId);

            if (Hotel == null)
            {
                return NotFound();
            }

            Rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return Page();
        }

        // ODA SÝLME
        public async Task<IActionResult> OnPostDeleteAsync(int roomId, int hotelId)
        {
            HotelId = hotelId;

            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == roomId && r.Hotel!.ManagerId == CurrentUserId);

            if (room == null)
                return NotFound();

            // Bu odaya baðlý rezervasyon var mý?
            bool hasReservations = await _context.Reservations
                .AnyAsync(r => r.RoomId == roomId);

            if (hasReservations)
            {
                ErrorMessage = "Bu odaya ait rezervasyonlar bulunduðu için oda silinemez.";
            }
            else
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                Message = "Oda baþarýyla silindi.";
            }

            return RedirectToPage(new { hotelId });
        }
    }
}
