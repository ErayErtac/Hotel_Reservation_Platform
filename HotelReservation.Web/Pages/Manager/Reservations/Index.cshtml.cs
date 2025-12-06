using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public IList<Reservation> Reservations { get; set; } = new List<Reservation>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Burada zaten manager'ýn otellerine baðlý rezervasyonlarý çekiyorsun varsayýyorum
            Reservations = await _context.Reservations
                .Include(r => r.Room).ThenInclude(r => r.Hotel)
                .Include(r => r.Customer)
                .Where(r => r.Room.Hotel.ManagerId == CurrentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // REZERVASYON SÝLME
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Reservations WHERE Id = {0}", id);

                Message = "Rezervasyon baþarýyla silindi.";
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Rezervasyon silinirken hata oluþtu: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}
