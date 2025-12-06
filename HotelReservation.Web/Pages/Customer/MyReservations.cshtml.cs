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

        // Þimdilik demo kullanýcý (ileride login'den gelecek)
        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public IList<CustomerReservationResult> Reservations { get; set; } = new List<CustomerReservationResult>();

        public async Task OnGetAsync()
        {
            var list = await _context.CustomerReservationResults
                .FromSqlRaw("EXEC dbo.sp_GetCustomerReservations @CustomerId = {0}", CurrentUserId)
                .ToListAsync();

            Reservations = list
                .OrderByDescending(r => r.CheckIn) // güvenlik için client tarafýnda da sýralayalým
                .ToList();
        }
    }
}
