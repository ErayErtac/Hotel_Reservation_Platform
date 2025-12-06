using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
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
            Reservations = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room!.Hotel)
                .Include(r => r.Customer)
                .Where(r => r.Room != null && r.Room.Hotel != null && r.Room.Hotel.ManagerId == CurrentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // REZERVASYON ONAYLAMA
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                        .ThenInclude(room => room!.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id && r.Room != null && r.Room.Hotel != null && r.Room.Hotel.ManagerId == CurrentUserId);

                if (reservation == null)
                {
                    ErrorMessage = "Rezervasyon bulunamadı veya bu rezervasyonu onaylama yetkiniz yok.";
                    return RedirectToPage();
                }

                reservation.Status = ReservationStatus.Confirmed;
                await _context.SaveChangesAsync();

                Message = "Rezervasyon başarıyla onaylandı.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Rezervasyon onaylanırken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }

        // REZERVASYON İPTAL ETME (Manager tarafından)
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                        .ThenInclude(room => room!.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id && r.Room != null && r.Room.Hotel != null && r.Room.Hotel.ManagerId == CurrentUserId);

                if (reservation == null)
                {
                    ErrorMessage = "Rezervasyon bulunamadı veya bu rezervasyonu iptal etme yetkiniz yok.";
                    return RedirectToPage();
                }

                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledAt = DateTime.UtcNow;
                reservation.CancellationReason = "Otel yöneticisi tarafından iptal edildi.";
                await _context.SaveChangesAsync();

                Message = "Rezervasyon başarıyla iptal edildi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Rezervasyon iptal edilirken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }

        // REZERVASYON SİLME
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                        .ThenInclude(room => room!.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id && r.Room != null && r.Room.Hotel != null && r.Room.Hotel.ManagerId == CurrentUserId);

                if (reservation == null)
                {
                    ErrorMessage = "Rezervasyon bulunamadı veya bu rezervasyonu silme yetkiniz yok.";
                    return RedirectToPage();
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                Message = "Rezervasyon başarıyla silindi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Rezervasyon silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}
