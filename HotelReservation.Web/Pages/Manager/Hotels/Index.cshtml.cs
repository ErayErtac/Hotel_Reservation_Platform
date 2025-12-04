using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Manager.Hotels
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        // Þimdilik demo otel yöneticisi (seed'deki manager Id'si)
        private int DemoManagerId => 2; // SSMS'ten kontrol edip gerekiyorsa deðiþtir

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

        public async Task OnGetAsync()
        {
            Hotels = await _context.Hotels
                .Where(h => h.ManagerId == DemoManagerId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}
