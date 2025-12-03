using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            Hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);
            if (Hotel == null)
            {
                return NotFound();
            }

            // ÖNCE listeye çek, sonra FirstOrDefault yap
            var statsList = await _context.HotelStatsResults
                .FromSqlRaw("EXEC dbo.sp_GetHotelStats @HotelId = {0}", id)
                .ToListAsync();

            Stats = statsList.FirstOrDefault();

            return Page();
        }

        // Oteli onayla butonu
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            Id = id;

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_ApproveHotel @HotelId = {0}", id);

                Message = "Otel baþarýyla onaylandý.";

                // Güncel durumu tekrar yükle
                Hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);

                var statsList = await _context.HotelStatsResults
                    .FromSqlRaw("EXEC dbo.sp_GetHotelStats @HotelId = {0}", id)
                    .ToListAsync();

                Stats = statsList.FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Onay iþlemi sýrasýnda hata oluþtu: " + ex.Message;
            }

            return Page();
        }
    }
}
