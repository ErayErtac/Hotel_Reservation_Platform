using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Reservations
{
    public class ConfirmationModel : PageModel
    {
        private readonly HotelDbContext _context;

        public ConfirmationModel(HotelDbContext context)
        {
            _context = context;
        }

        public Reservation? Reservation { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room!.Hotel)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == CurrentUserId);

            if (Reservation == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

