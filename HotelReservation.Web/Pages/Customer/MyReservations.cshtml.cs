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
        public Dictionary<int, int> ReservationHotelIds { get; set; } = new Dictionary<int, int>(); // ReservationId -> HotelId

        public async Task OnGetAsync()
        {
            var list = await _context.CustomerReservationResults
                .FromSqlRaw("EXEC dbo.sp_GetCustomerReservations @CustomerId = {0}", CurrentUserId)
                .ToListAsync();

            Reservations = list
                .OrderByDescending(r => r.CheckIn)
                .ToList();

            // Her rezervasyon için HotelId'yi al
            var reservationIds = Reservations.Select(r => r.ReservationId).ToList();
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => reservationIds.Contains(r.Id))
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                if (reservation.Room != null)
                {
                    ReservationHotelIds[reservation.Id] = reservation.Room.HotelId;
                }
            }
        }
    }
}
