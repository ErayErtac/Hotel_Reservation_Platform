using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Customer
{
    public class MyReservationsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public MyReservationsModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public IList<CustomerReservationResult> Reservations { get; set; } = new List<CustomerReservationResult>();
        public IList<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
        public Dictionary<int, int> ReservationHotelIds { get; set; } = new Dictionary<int, int>(); // ReservationId -> HotelId

        public async Task OnGetAsync()
        {
            // stored procedure ile müşteri rezervasyonlarını al
            var list = await _context.CustomerReservationResults
                .FromSqlRaw("EXEC dbo.sp_GetCustomerReservations @CustomerId = {0}", CurrentUserId)
                .ToListAsync();

            // Her rezervasyon için HotelId'yi ve CreatedAt'i al
            var reservationIds = list.Select(r => r.ReservationId).ToList();
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => reservationIds.Contains(r.Id))
                .ToListAsync();

            // CreatedAt bilgisini kullanarak sıralama yap
            var reservationsWithDates = list
                .Join(reservations,
                    cr => cr.ReservationId,
                    r => r.Id,
                    (cr, r) => new { ReservationResult = cr, CreatedAt = r.CreatedAt, Reservation = r })
                .OrderByDescending(x => x.CreatedAt) // En yeni rezervasyon en üstte
                .ToList();

            Reservations = reservationsWithDates.Select(x => x.ReservationResult).ToList();

            foreach (var item in reservationsWithDates)
            {
                if (item.Reservation.Room != null)
                {
                    ReservationHotelIds[item.Reservation.Id] = item.Reservation.Room.HotelId;
                }
            }

            // Tur rezervasyonlarını al (zaten CreatedAt'e göre sıralı)
            TourBookings = await _context.TourBookings
                .Include(tb => tb.Tour)
                .Where(tb => tb.CustomerId == CurrentUserId)
                .OrderByDescending(tb => tb.CreatedAt)
                .ToListAsync();
        }
    }
}
