using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Admin.Users
{
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        public AppUser? AppUser { get; set; }
        public int HotelCount { get; set; }
        public int ReservationCount { get; set; }
        public int ReviewCount { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            AppUser = await _context.Users
                .Include(u => u.ManagedHotels)
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(room => room!.Hotel)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (AppUser == null)
            {
                return NotFound();
            }

            HotelCount = AppUser.ManagedHotels?.Count ?? 0;
            ReservationCount = AppUser.Reservations?.Count ?? 0;
            ReviewCount = AppUser.Reviews?.Count ?? 0;

            return Page();
        }
    }
}

