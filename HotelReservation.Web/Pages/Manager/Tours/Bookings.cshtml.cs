using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Tours
{
    public class BookingsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public BookingsModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [BindProperty(SupportsGet = true)]
        public int TourId { get; set; }

        public Tour? Tour { get; set; }
        public IList<TourBooking> Bookings { get; set; } = new List<TourBooking>();

        public async Task<IActionResult> OnGetAsync()
        {
            Tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == TourId && t.ManagerId == CurrentUserId);

            if (Tour == null)
            {
                return NotFound();
            }

            Bookings = await _context.TourBookings
                .Include(tb => tb.Customer)
                .Where(tb => tb.TourId == TourId)
                .OrderByDescending(tb => tb.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var booking = await _context.TourBookings
                .Include(tb => tb.Tour)
                .FirstOrDefaultAsync(tb => tb.Id == id && tb.Tour.ManagerId == CurrentUserId);

            if (booking == null)
                return NotFound();

            booking.Status = Core.Enums.ReservationStatus.Confirmed;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { tourId = TourId });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var booking = await _context.TourBookings
                .Include(tb => tb.Tour)
                .FirstOrDefaultAsync(tb => tb.Id == id && tb.Tour.ManagerId == CurrentUserId);

            if (booking == null)
                return NotFound();

            booking.Status = Core.Enums.ReservationStatus.Cancelled;
            booking.CancelledAt = System.DateTime.UtcNow;
            
            // Kapasiteyi güncelle
            booking.Tour.CurrentBookings -= booking.ParticipantCount;
            
            await _context.SaveChangesAsync();

            return RedirectToPage(new { tourId = TourId });
        }
    }
}

