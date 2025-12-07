using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.TourBookings
{
    public class CreateModel : PageModel
    {
        private readonly HotelDbContext _context;

        public CreateModel(HotelDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int TourId { get; set; }

        [BindProperty]
        public int ParticipantCount { get; set; } = 1;

        public Tour? Tour { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/TourBookings/Create?tourId={TourId}" });
            }

            Tour = await _context.Tours
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == TourId && t.IsActive && t.IsApproved);

            if (Tour == null)
            {
                return NotFound();
            }

            // Kapasite kontrolü
            if (Tour.CurrentBookings >= Tour.Capacity)
            {
                TempData["ErrorMessage"] = "Bu tur için kontenjan dolmuştur.";
                return RedirectToPage("/Tours/Details", new { id = TourId });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            Tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == TourId && t.IsActive && t.IsApproved);

            if (Tour == null)
            {
                return NotFound();
            }

            // Validasyonlar
            if (ParticipantCount < 1)
            {
                ModelState.AddModelError(nameof(ParticipantCount), "Katılımcı sayısı en az 1 olmalıdır.");
                return Page();
            }

            if (Tour.CurrentBookings + ParticipantCount > Tour.Capacity)
            {
                ModelState.AddModelError(nameof(ParticipantCount), $"Bu tur için sadece {Tour.Capacity - Tour.CurrentBookings} kişilik kontenjan kalmıştır.");
                return Page();
            }

            var totalPrice = Tour.Price * ParticipantCount;

            var booking = new TourBooking
            {
                CustomerId = userId,
                TourId = TourId,
                ParticipantCount = ParticipantCount,
                TotalPrice = totalPrice,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Tur kapasitesini güncelle
            Tour.CurrentBookings += ParticipantCount;

            _context.TourBookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("/TourBookings/Confirmation", new { id = booking.Id });
        }
    }
}

