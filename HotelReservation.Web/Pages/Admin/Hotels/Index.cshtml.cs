using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Admin.Hotels
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

        // Kaç otel onay bekliyor?
        public int PendingCount { get; set; }

        // Filtre: pending / approved / all
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "pending";

        public async Task OnGetAsync()
        {
            // Toplam onay bekleyen sayýsý (her zaman lazým)
            PendingCount = await _context.Hotels
                .CountAsync(h => !h.IsApproved);

            var query = _context.Hotels
                .Include(h => h.Manager)
                .OrderByDescending(h => h.CreatedAt)
                .AsQueryable();

            // Varsayýlan: sadece onay bekleyenleri göster
            switch (StatusFilter?.ToLowerInvariant())
            {
                case "approved":
                    query = query.Where(h => h.IsApproved);
                    break;
                case "all":
                    // filtre yok
                    break;
                default: // "pending"
                    StatusFilter = "pending";
                    query = query.Where(h => !h.IsApproved);
                    break;
            }

            Hotels = await query.ToListAsync();
        }
    }
}
