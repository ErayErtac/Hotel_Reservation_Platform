using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Manager.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<HotelReview> Reviews { get; set; } = new List<HotelReview>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task OnGetAsync()
        {
            // Manager'ın otellerine ait yorumları getir
            Reviews = await _context.HotelReviews
                .Include(r => r.Hotel)
                .Include(r => r.Customer)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.Manager)
                .Where(r => r.Hotel != null && r.Hotel.ManagerId == CurrentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var review = await _context.HotelReviews
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id && r.Hotel != null && r.Hotel.ManagerId == CurrentUserId);

                if (review == null)
                {
                    ErrorMessage = "Yorum bulunamadı veya bu yorumu silme yetkiniz yok.";
                    return RedirectToPage();
                }

                _context.HotelReviews.Remove(review);
                await _context.SaveChangesAsync();

                Message = "Yorum başarıyla silindi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Yorum silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}

