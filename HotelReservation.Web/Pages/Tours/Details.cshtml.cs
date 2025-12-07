using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Tours
{
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        public Tour? Tour { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Tour = await _context.Tours
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.Customer)
                .Include(t => t.Images)
                .Include(t => t.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive && t.IsApproved);

            if (Tour == null)
            {
                return NotFound();
            }

            return Page();
        }

        public double? GetAverageRating()
        {
            if (Tour == null || Tour.Reviews == null || !Tour.Reviews.Any())
                return null;

            return Tour.Reviews.Average(r => r.Rating);
        }

        public bool IsAvailable()
        {
            return Tour != null && Tour.CurrentBookings < Tour.Capacity;
        }
    }
}

