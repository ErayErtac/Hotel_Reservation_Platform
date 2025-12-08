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

        [BindProperty(SupportsGet = true)]
        public string? SearchType { get; set; } = "Hotel";

        [BindProperty(SupportsGet = true)]
        public DateTime? TourDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ParticipantCount { get; set; } = 1;

        public IList<AvailableRoomResult> Results { get; set; } = new List<AvailableRoomResult>();
        public IList<Tour> TourResults { get; set; } = new List<Tour>();
        public List<string> Cities { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            // Şehir listesini al
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

            // Eğer hiçbir arama parametresi yoksa arama yapma
            if (string.IsNullOrWhiteSpace(City) && !CheckIn.HasValue && !CheckOut.HasValue && !TourDate.HasValue)
                return;

            if (SearchType == "Tour")
            {
                // Tur araması
                var query = _context.Tours
                    .Include(t => t.Reviews)
                    .Include(t => t.Images)
                    .Where(t => t.IsActive && t.IsApproved 
                        && t.StartDate >= DateTime.UtcNow
                        && t.CurrentBookings < t.Capacity);

                // Şehir filtresi
                if (!string.IsNullOrWhiteSpace(City))
                {
                    query = query.Where(t => t.City.Contains(City));
                }

                // Tarih filtresi (opsiyonel)
                if (TourDate.HasValue)
                {
                    query = query.Where(t => t.StartDate.Date <= TourDate.Value.Date 
                        && t.EndDate.Date >= TourDate.Value.Date);
                }

                TourResults = await query
                    .OrderBy(t => t.StartDate)
                    .ToListAsync();
            }
            else
            {
                // Otel araması
                // Eğer tarih bilgileri varsa stored procedure kullan
                if (CheckIn.HasValue && CheckOut.HasValue && GuestCount > 0)
                {
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

                    //1. Stored procedure çağrısı
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
                else if (!string.IsNullOrWhiteSpace(City))
                {
                    // Sadece şehir ile arama - tüm otelleri listele
                    var hotels = await _context.Hotels
                        .Include(h => h.Rooms.Where(r => r.IsActive))
                        .Include(h => h.Images)
                        .Include(h => h.Reviews)
                        .Where(h => h.IsActive && h.IsApproved && h.City.Contains(City))
                        .ToListAsync();

                    // Her otel için müsait odaları göster
                    var availableRooms = new List<AvailableRoomResult>();
                    foreach (var hotel in hotels)
                    {
                        foreach (var room in hotel.Rooms.Where(r => r.IsActive))
                        {
                            availableRooms.Add(new AvailableRoomResult
                            {
                                HotelId = hotel.Id,
                                HotelName = hotel.Name,
                                City = hotel.City,
                                Address = hotel.Address ?? "",
                                RoomId = room.Id,
                                RoomNumber = room.RoomNumber,
                                Capacity = room.Capacity,
                                PricePerNight = room.PricePerNight
                            });
                        }
                    }

                    // Fiyat filtresi
                    if (MinPrice.HasValue)
                    {
                        availableRooms = availableRooms.Where(r => r.PricePerNight >= MinPrice.Value).ToList();
                    }

                    if (MaxPrice.HasValue)
                    {
                        availableRooms = availableRooms.Where(r => r.PricePerNight <= MaxPrice.Value).ToList();
                    }

                    Results = availableRooms;
                }
            }
        }
    }
}
