using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Search
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? City { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckIn { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckOut { get; set; }

        [BindProperty(SupportsGet = true)]
        public int GuestCount { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinRating { get; set; }

        public IList<AvailableRoomResult> Results { get; set; } = new List<AvailableRoomResult>();

        public async Task OnGetAsync()
        {
            // İlk açılışta parametre yoksa arama yapma
            if (!CheckIn.HasValue || !CheckOut.HasValue || GuestCount <= 0)
                return;

            // Validation
            if (CheckIn.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(CheckIn), "Giriş tarihi bugünden önce olamaz.");
                return;
            }

            if (CheckOut.Value.Date <= CheckIn.Value.Date)
            {
                ModelState.AddModelError(nameof(CheckOut), "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
                return;
            }

            var cityParam = string.IsNullOrWhiteSpace(City) ? null : City;

            var results = await _context.AvailableRoomResults
                .FromSqlRaw(
                    "EXEC dbo.sp_SearchAvailableRooms @City = {0}, @CheckIn = {1}, @CheckOut = {2}, @GuestCount = {3}",
                    cityParam,
                    CheckIn.Value.Date,
                    CheckOut.Value.Date,
                    GuestCount
                )
                .ToListAsync();

            // Client-side filtering for price
            if (MinPrice.HasValue)
            {
                results = results.Where(r => r.PricePerNight >= MinPrice.Value).ToList();
            }

            if (MaxPrice.HasValue)
            {
                results = results.Where(r => r.PricePerNight <= MaxPrice.Value).ToList();
            }

            Results = results;
        }
    }
}
