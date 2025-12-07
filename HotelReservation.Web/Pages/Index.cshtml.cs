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
        public List<Tour> FeaturedTours { get; set; } = new List<Tour>();

        public async Task OnGetAsync()
        {
            // Sistemde kayıtlı, aktif ve onaylanmış otellerin ve turların şehirlerini al
            var hotelCities = await _context.Hotels
                .Where(h => h.IsActive && h.IsApproved)
                .Select(h => h.City)
                .Distinct()
                .ToListAsync();

            var tourCities = await _context.Tours
                .Where(t => t.IsActive && t.IsApproved)
                .Select(t => t.City)
                .Distinct()
                .ToListAsync();

            Cities = hotelCities.Union(tourCities).Distinct().OrderBy(c => c).ToList();

            // Önerilen oteller: Admin tarafından önerilen olarak işaretlenmiş oteller
            var featuredHotelsQuery = await _context.Hotels
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Include(h => h.Rooms)
                .Where(h => h.IsActive && h.IsApproved && h.IsFeatured)
                .ToListAsync();

            // Eğer önerilen otel yoksa, en yüksek puanlı otelleri göster
            if (featuredHotelsQuery.Any())
            {
                FeaturedHotels = featuredHotelsQuery
                    .OrderByDescending(h => h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0)
                    .ThenByDescending(h => h.Reviews.Count)
                    .Take(6)
                    .ToList();
            }
            else
            {
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

            // Önerilen turlar: Admin tarafından önerilen olarak işaretlenmiş turlar
            var featuredToursQuery = await _context.Tours
                .Include(t => t.Reviews)
                .Include(t => t.Images)
                .Where(t => t.IsActive && t.IsApproved && t.IsFeatured && t.StartDate >= DateTime.UtcNow && t.CurrentBookings < t.Capacity)
                .ToListAsync();

            // Eğer önerilen tur yoksa, yakın tarihli ve müsait turları göster
            if (featuredToursQuery.Any())
            {
                FeaturedTours = featuredToursQuery
                    .OrderBy(t => t.StartDate)
                    .Take(6)
                    .ToList();
            }
            else
            {
                FeaturedTours = await _context.Tours
                    .Include(t => t.Reviews)
                    .Include(t => t.Images)
                    .Where(t => t.IsActive && t.IsApproved && t.StartDate >= DateTime.UtcNow && t.CurrentBookings < t.Capacity)
                    .OrderBy(t => t.StartDate)
                    .Take(6)
                    .ToListAsync();
            }
        }

        public double? GetAverageRating(Hotel hotel)
        {
            if (hotel.Reviews == null || !hotel.Reviews.Any())
                return null;
            return hotel.Reviews.Average(r => r.Rating);
        }

        public double? GetTourAverageRating(Tour tour)
        {
            if (tour.Reviews == null || !tour.Reviews.Any())
                return null;
            return tour.Reviews.Average(r => r.Rating);
        }
    }
}
