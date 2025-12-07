using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Customer
{
    public class CreateReviewModel : PageModel
    {
        private readonly HotelDbContext _context;

        public CreateReviewModel(HotelDbContext context)
        {
            _context = context;
        }

        public Hotel? Hotel { get; set; }
        public Reservation? Reservation { get; set; }
        public bool HasExistingReview { get; set; }
        public HotelReview? ExistingReview { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Puan seçiniz.")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
        [Display(Name = "Puan")]
        public int Rating { get; set; } = 5;

        [BindProperty]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        [Display(Name = "Yorum")]
        public string? Comment { get; set; }

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ReservationId { get; set; }

        public string? ErrorMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> OnGetAsync()
        {
            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.IsActive && h.IsApproved);

            if (Hotel == null)
            {
                return NotFound();
            }

            // Kullanıcının bu otelde rezervasyonu var mı kontrol et
            if (ReservationId.HasValue)
            {
                Reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(r => r.Id == ReservationId.Value && 
                                             r.CustomerId == CurrentUserId &&
                                             r.Room != null && 
                                             r.Room.HotelId == HotelId);
            }
            else
            {
                // Rezervasyon ID yoksa, kullanıcının bu otelde tamamlanmış rezervasyonu var mı kontrol et
                Reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .Where(r => r.CustomerId == CurrentUserId &&
                               r.Room != null &&
                               r.Room.HotelId == HotelId &&
                               r.CheckOut.Date < DateTime.Today &&
                               r.Status == Core.Enums.ReservationStatus.Confirmed)
                    .OrderByDescending(r => r.CheckOut)
                    .FirstOrDefaultAsync();
            }

            if (Reservation == null)
            {
                ErrorMessage = "Bu otelde rezervasyonunuz bulunmuyor veya rezervasyonunuz henüz tamamlanmamış. Sadece tamamlanmış rezervasyonlar için yorum yapabilirsiniz.";
            }
            else
            {
                // Kullanıcının bu rezervasyon için zaten yorumu var mı?
                ExistingReview = await _context.HotelReviews
                    .FirstOrDefaultAsync(r => r.HotelId == HotelId && 
                                             r.CustomerId == CurrentUserId &&
                                             r.ReservationId == Reservation.Id);

                HasExistingReview = ExistingReview != null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.IsActive && h.IsApproved);

            if (Hotel == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Rezervasyon bilgilerini tekrar yükle
                return Page();
            }

            // Rezervasyon kontrolü
            if (ReservationId.HasValue)
            {
                Reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(r => r.Id == ReservationId.Value && 
                                             r.CustomerId == CurrentUserId &&
                                             r.Room != null && 
                                             r.Room.HotelId == HotelId);
            }
            else
            {
                Reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .Where(r => r.CustomerId == CurrentUserId &&
                               r.Room != null &&
                               r.Room.HotelId == HotelId &&
                               r.CheckOut.Date < DateTime.Today &&
                               r.Status == Core.Enums.ReservationStatus.Confirmed)
                    .OrderByDescending(r => r.CheckOut)
                    .FirstOrDefaultAsync();
            }

            if (Reservation == null)
            {
                ErrorMessage = "Bu otelde rezervasyonunuz bulunmuyor veya rezervasyonunuz henüz tamamlanmamış.";
                await OnGetAsync();
                return Page();
            }

            // Zaten yorum var mı kontrol et
            var existingReview = await _context.HotelReviews
                .FirstOrDefaultAsync(r => r.HotelId == HotelId && 
                                         r.CustomerId == CurrentUserId &&
                                         r.ReservationId == Reservation.Id);

            if (existingReview != null)
            {
                // Mevcut yorumu güncelle
                existingReview.Rating = Rating;
                existingReview.Comment = Comment;
                existingReview.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Yeni yorum oluştur
                var review = new HotelReview
                {
                    HotelId = HotelId,
                    CustomerId = CurrentUserId,
                    ReservationId = Reservation.Id,
                    Rating = Rating,
                    Comment = Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.HotelReviews.Add(review);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Hotels/Details", new { id = HotelId });
        }
    }
}

