using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public List<string> Cities { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            // Sistemde kayıtlı, aktif ve onaylanmış otellerin şehirlerini al
            Cities = await _context.Hotels
                .Where(h => h.IsActive && h.IsApproved)
                .Select(h => h.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
