using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Hotels
{
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        public Hotel? Hotel { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Hotel = await _context.Hotels
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomType)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Amenities)
                .Include(h => h.Reviews)
                    .ThenInclude(r => r.Customer)
                .Include(h => h.Reviews)
                    .ThenInclude(r => r.Replies)
                        .ThenInclude(reply => reply.Manager)
                .Include(h => h.Images)
                .Include(h => h.Amenities)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id && h.IsActive && h.IsApproved);

            if (Hotel == null)
            {
                return NotFound();
            }

            return Page();
        }

        public double? GetAverageRating()
        {
            if (Hotel == null || Hotel.Reviews == null || !Hotel.Reviews.Any())
                return null;

            return Hotel.Reviews.Average(r => r.Rating);
        }
    }
}
