using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Admin.Tours
{
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Tour? Tour { get; set; }

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public bool IsPendingApproval =>
            Tour != null && !Tour.IsApproved;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            Tour = await _context.Tours
                .Include(t => t.Manager)
                .Include(t => t.Images)
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.Customer)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Tour == null)
                return NotFound();

            if (TempData.ContainsKey("AdminMessage"))
            {
                Message = TempData["AdminMessage"]?.ToString();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            Id = id;

            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            tour.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Tur başarıyla onaylandı.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            Id = id;

            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsApproved);

            if (tour == null)
                return NotFound();

            bool hasBookings = await _context.TourBookings
                .AnyAsync(tb => tb.TourId == id);

            if (hasBookings)
            {
                ErrorMessage = "Bu tur için rezervasyon bulunduğu için silinemez.";
                Tour = await _context.Tours
                    .Include(t => t.Manager)
                    .Include(t => t.Images)
                    .FirstOrDefaultAsync(t => t.Id == id);
                return Page();
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Tur başarıyla reddedildi ve silindi.";
            return RedirectToPage("/Admin/Tours/Index");
        }
    }
}

