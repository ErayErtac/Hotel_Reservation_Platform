using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.TourBookings
{
    public class ConfirmationModel : PageModel
    {
        private readonly HotelDbContext _context;

        public ConfirmationModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public TourBooking? Booking { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            Booking = await _context.TourBookings
                .Include(tb => tb.Tour)
                .Include(tb => tb.Customer)
                .FirstOrDefaultAsync(tb => tb.Id == Id && tb.CustomerId == userId);

            if (Booking == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

