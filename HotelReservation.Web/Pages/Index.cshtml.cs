using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public List<string> Cities { get; set; } = new List<string>();
        public List<Hotel> FeaturedHotels { get; set; } = new List<Hotel>();

        public async Task OnGetAsync()
        {
            // Sistemde kayıtlı, aktif ve onaylanmış otellerin şehirlerini al
            Cities = await _context.Hotels
                .Where(h => h.IsActive && h.IsApproved)
                .Select(h => h.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Önerilen oteller: En yüksek puanlı ve en çok yorum alan oteller
            FeaturedHotels = await _context.Hotels
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Include(h => h.Rooms)
                .Where(h => h.IsActive && h.IsApproved && h.Reviews.Any())
                .OrderByDescending(h => h.Reviews.Average(r => r.Rating))
                .ThenByDescending(h => h.Reviews.Count)
                .Take(6)
                .ToListAsync();
        }

        public double? GetAverageRating(Hotel hotel)
        {
            if (hotel.Reviews == null || !hotel.Reviews.Any())
                return null;
            return hotel.Reviews.Average(r => r.Rating);
        }
    }
}
