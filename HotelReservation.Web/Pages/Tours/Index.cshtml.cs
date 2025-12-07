using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace HotelReservation.Web.Pages.Tours
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;
        private const int PageSize = 12;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Tour> Tours { get; set; } = new List<Tour>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? SearchCity { get; set; }
        public DateTime? SearchDate { get; set; }

        public async Task OnGetAsync(int page = 1, string? city = null, DateTime? tourDate = null)
        {
            CurrentPage = page;
            SearchCity = city;
            SearchDate = tourDate;

            var query = _context.Tours
                .Include(t => t.Reviews)
                .Include(t => t.Images)
                .Where(t => t.IsActive && t.IsApproved && t.StartDate >= DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(t => t.City.Contains(city));
            }

            if (tourDate.HasValue)
            {
                query = query.Where(t => t.StartDate.Date <= tourDate.Value.Date && t.EndDate.Date >= tourDate.Value.Date);
            }

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Tours = await query
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.City)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public double? GetAverageRating(Tour tour)
        {
            if (tour.Reviews == null || !tour.Reviews.Any())
                return null;

            return tour.Reviews.Average(r => r.Rating);
        }
    }
}

