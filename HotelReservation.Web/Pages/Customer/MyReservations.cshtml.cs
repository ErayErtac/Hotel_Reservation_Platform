using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

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
        private int DemoCustomerId => 4;

        public IList<CustomerReservationResult> Reservations { get; set; } = new List<CustomerReservationResult>();

        public async Task OnGetAsync()
        {
            var list = await _context.CustomerReservationResults
                .FromSqlRaw("EXEC dbo.sp_GetCustomerReservations @CustomerId = {0}", DemoCustomerId)
                .ToListAsync();

            Reservations = list
                .OrderByDescending(r => r.CheckIn) // güvenlik için client tarafýnda da sýralayalým
                .ToList();
        }
    }
}
