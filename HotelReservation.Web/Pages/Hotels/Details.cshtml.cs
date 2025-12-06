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

        public IActionResult OnGet(int id)
        {
            Hotel = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .FirstOrDefault(h => h.Id == id && h.IsActive && h.IsApproved);

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
