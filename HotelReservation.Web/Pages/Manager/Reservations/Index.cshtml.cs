using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Manager.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        // Þimdilik demo otel yöneticisi
        private int DemoManagerId => 2;

        public IList<Reservation> Reservations { get; set; } = new List<Reservation>();

        public async Task OnGetAsync()
        {
            Reservations = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                .Include(r => r.Customer)
                .Where(r => r.Room.Hotel.ManagerId == DemoManagerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id && r.Room.Hotel.ManagerId == DemoManagerId);

            if (reservation == null)
                return NotFound();

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                // Ýptal edilmiþ rezervasyon onaylanmasýn
                TempData["ErrorMessage"] = "Ýptal edilmiþ bir rezervasyonu onaylayamazsýnýz.";
                return RedirectToPage();
            }

            reservation.Status = ReservationStatus.Confirmed;

            await _context.SaveChangesAsync();
            // UPDATE trigger'ýmýz burada devreye giriyor ve ReservationAudit'e log yazýyor

            TempData["SuccessMessage"] = "Rezervasyon baþarýyla onaylandý.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id && r.Room.Hotel.ManagerId == DemoManagerId);

            if (reservation == null)
                return NotFound();

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Rezervasyon zaten iptal edilmiþ.";
                return RedirectToPage();
            }

            reservation.Status = ReservationStatus.Cancelled;

            await _context.SaveChangesAsync();
            // UPDATE trigger yine çalýþýp audit tablosuna kayýt atacak

            TempData["SuccessMessage"] = "Rezervasyon iptal edildi.";
            return RedirectToPage();
        }
    }
}
