using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Customer
{
    public class CancelReservationModel : PageModel
    {
        private readonly HotelDbContext _context;

        public CancelReservationModel(HotelDbContext context)
        {
            _context = context;
        }

        public Reservation? Reservation { get; set; }

        [BindProperty]
        public string? CancellationReason { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room!.Hotel)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == CurrentUserId);

            if (Reservation == null)
            {
                return NotFound();
            }

            if (Reservation.Status == ReservationStatus.Cancelled)
            {
                ErrorMessage = "Bu rezervasyon zaten iptal edilmiş.";
            }

            if (Reservation.CheckIn.Date <= DateTime.Today)
            {
                ErrorMessage = "Geçmiş veya bugünkü rezervasyonlar iptal edilemez.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == CurrentUserId);

            if (Reservation == null)
            {
                return NotFound();
            }

            if (Reservation.Status == ReservationStatus.Cancelled)
            {
                ErrorMessage = "Bu rezervasyon zaten iptal edilmiş.";
                return Page();
            }

            if (Reservation.CheckIn.Date <= DateTime.Today)
            {
                ErrorMessage = "Geçmiş veya bugünkü rezervasyonlar iptal edilemez.";
                return Page();
            }

            try
            {
                Reservation.Status = ReservationStatus.Cancelled;
                Reservation.CancelledAt = DateTime.UtcNow;
                Reservation.CancellationReason = CancellationReason;

                await _context.SaveChangesAsync();

                SuccessMessage = "Rezervasyon başarıyla iptal edildi.";
                return RedirectToPage("/Customer/MyReservations", new { message = "Rezervasyon iptal edildi." });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Rezervasyon iptal edilirken bir hata oluştu: " + ex.Message;
                return Page();
            }
        }
    }
}

