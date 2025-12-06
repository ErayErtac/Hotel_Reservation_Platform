using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;


namespace HotelReservation.Web.Pages.Hotels
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

        public void OnGet()
        {
            Hotels = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Where(h => h.IsActive && h.IsApproved)
                .OrderBy(h => h.City)
                .ThenBy(h => h.Name)
                .ToList();
        }

        // Ortalama puan hesaplamak için küçük yardýmcý metot
        public double? GetAverageRating(Hotel hotel)
        {
            if (hotel.Reviews == null || !hotel.Reviews.Any())
                return null;

            return hotel.Reviews.Average(r => r.Rating);
        }
    }
}
