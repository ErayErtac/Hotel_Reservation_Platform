using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Manager.Reviews
{
    public class ReplyModel : PageModel
    {
        private readonly HotelDbContext _context;

        public ReplyModel(HotelDbContext context)
        {
            _context = context;
        }

        public HotelReview? Review { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Yanıt metni gereklidir.")]
        [StringLength(1000, ErrorMessage = "Yanıt en fazla 1000 karakter olabilir.")]
        [Display(Name = "Yanıt")]
        public string ReplyText { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Review = await _context.HotelReviews
                .Include(r => r.Hotel)
                .Include(r => r.Customer)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.Manager)
                .FirstOrDefaultAsync(r => r.Id == id && r.Hotel != null && r.Hotel.ManagerId == CurrentUserId);

            if (Review == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Review = await _context.HotelReviews
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id && r.Hotel != null && r.Hotel.ManagerId == CurrentUserId);

            if (Review == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Review = await _context.HotelReviews
                    .Include(r => r.Customer)
                    .Include(r => r.Replies)
                        .ThenInclude(reply => reply.Manager)
                    .FirstOrDefaultAsync(r => r.Id == id);
                return Page();
            }

            try
            {
                var reply = new ReviewReply
                {
                    ReviewId = id,
                    ManagerId = CurrentUserId,
                    ReplyText = ReplyText,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReviewReplies.Add(reply);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Manager/Reviews/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Yanıt eklenirken hata oluştu: " + ex.Message;
                Review = await _context.HotelReviews
                    .Include(r => r.Customer)
                    .Include(r => r.Replies)
                        .ThenInclude(reply => reply.Manager)
                    .FirstOrDefaultAsync(r => r.Id == id);
                return Page();
            }
        }
    }
}

