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
        private const int PageSize = 12;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? SearchCity { get; set; }

        public async Task OnGetAsync(int page = 1, string? city = null)
        {
            CurrentPage = page;
            SearchCity = city;

            var query = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Where(h => h.IsActive && h.IsApproved);

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(h => h.City.Contains(city));
            }

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Hotels = await query
                .OrderBy(h => h.City)
                .ThenBy(h => h.Name)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Ortalama puan hesaplamak için küçük yardımcı metot
        public double? GetAverageRating(Hotel hotel)
        {
            if (hotel.Reviews == null || !hotel.Reviews.Any())
                return null;

            return hotel.Reviews.Average(r => r.Rating);
        }
    }
}
