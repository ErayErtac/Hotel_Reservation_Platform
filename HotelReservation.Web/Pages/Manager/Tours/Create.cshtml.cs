using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Tours
{
    public class CreateModel : PageModel
    {
        private readonly HotelDbContext _context;

        public CreateModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [BindProperty]
        public Tour Input { get; set; } = new Tour();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Navigation property validation'ı atla
            ModelState.Remove("Input.Manager");
            ModelState.Remove("Input.ManagerId");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lütfen tüm alanları doğru şekilde doldurun.");
                return Page();
            }

            // Tarih kontrolü
            if (Input.EndDate <= Input.StartDate)
            {
                ModelState.AddModelError("Input.EndDate", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                return Page();
            }

            Input.ManagerId = CurrentUserId;
            Input.IsActive = true;
            Input.IsApproved = false;
            Input.CreatedAt = DateTime.UtcNow;
            Input.CurrentBookings = 0;

            _context.Tours.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

