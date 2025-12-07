using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Admin.Featured
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();
        public IList<Tour> Tours { get; set; } = new List<Tour>();
        public IList<Hotel> FeaturedHotels { get; set; } = new List<Hotel>();
        public IList<Tour> FeaturedTours { get; set; } = new List<Tour>();

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            // Tüm aktif ve onaylı oteller
            Hotels = await _context.Hotels
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Where(h => h.IsActive && h.IsApproved)
                .OrderBy(h => h.Name)
                .ToListAsync();

            // Tüm aktif ve onaylı turlar
            Tours = await _context.Tours
                .Include(t => t.Reviews)
                .Include(t => t.Images)
                .Where(t => t.IsActive && t.IsApproved)
                .OrderBy(t => t.Name)
                .ToListAsync();

            // Önerilen oteller
            FeaturedHotels = await _context.Hotels
                .Include(h => h.Reviews)
                .Include(h => h.Images)
                .Where(h => h.IsFeatured && h.IsActive && h.IsApproved)
                .OrderBy(h => h.Name)
                .ToListAsync();

            // Önerilen turlar
            FeaturedTours = await _context.Tours
                .Include(t => t.Reviews)
                .Include(t => t.Images)
                .Where(t => t.IsFeatured && t.IsActive && t.IsApproved)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleHotelAsync(int hotelId, bool isFeatured)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId);

            if (hotel == null)
            {
                return NotFound();
            }

            hotel.IsFeatured = isFeatured;
            await _context.SaveChangesAsync();

            Message = isFeatured ? "Otel önerilenler listesine eklendi." : "Otel önerilenler listesinden çıkarıldı.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleTourAsync(int tourId, bool isFeatured)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null)
            {
                return NotFound();
            }

            tour.IsFeatured = isFeatured;
            await _context.SaveChangesAsync();

            Message = isFeatured ? "Tur önerilenler listesine eklendi." : "Tur önerilenler listesinden çıkarıldı.";
            return RedirectToPage();
        }

        public double? GetAverageRating(Hotel hotel)
        {
            if (hotel.Reviews == null || !hotel.Reviews.Any())
                return null;
            return hotel.Reviews.Average(r => r.Rating);
        }

        public double? GetAverageRating(Tour tour)
        {
            if (tour.Reviews == null || !tour.Reviews.Any())
                return null;
            return tour.Reviews.Average(r => r.Rating);
        }
    }
}

