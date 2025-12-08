using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Admin.Hotels
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

        public HotelStatsResult? Stats { get; set; }
        public Hotel? Hotel { get; set; }

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        // Admin sayfasýnda kolay kullanmak için:
        // Otel onay bekliyor mu?
        public bool IsPendingApproval =>
            Hotel != null && !Hotel.IsApproved;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var ok = await LoadHotelAndStatsAsync(id);
            if (!ok)
            {
                return NotFound();
            }

            // Eðer önceki POST'tan gelen bir TempData mesajý varsa al
            if (TempData.ContainsKey("AdminMessage"))
            {
                Message = TempData["AdminMessage"]?.ToString();
            }

            return Page();
        }

        // Oteli onayla butonu
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            Id = id;

            try
            {
                // Stored procedure çaðrýsý ile onayla
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_ApproveHotel @HotelId = {0}", id);

                // Redirect + TempData kullanmak, F5 ile tekrar POST olmasýný engeller (PRG pattern)
                TempData["AdminMessage"] = "Otel baþarýyla onaylandý.";
                return RedirectToPage(new { id });
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Onay iþlemi sýrasýnda hata oluþtu: " + ex.Message;

                var ok = await LoadHotelAndStatsAsync(id);
                if (!ok)
                {
                    return NotFound();
                }

                return Page();
            }
        }

        // Ortak yükleme metodu: Hem GET hem POST sonrasý burayý kullanýyoruz
        private async Task<bool> LoadHotelAndStatsAsync(int id)
        {
            Hotel = await _context.Hotels
                .Include(h => h.Manager)   // yöneticiyi de görebil
                .Include(h => h.Images)    // fotoðraflar
                .Include(h => h.Rooms)     // odalar
                .FirstOrDefaultAsync(h => h.Id == id);

            if (Hotel == null)
                return false;

            // Stored procedure ile istatistikleri al
            var statsList = await _context.HotelStatsResults
                .FromSqlRaw("EXEC dbo.sp_GetHotelStats @HotelId = {0}", id)
                .ToListAsync();

            Stats = statsList.FirstOrDefault();

            return true;
        }
    }
}
