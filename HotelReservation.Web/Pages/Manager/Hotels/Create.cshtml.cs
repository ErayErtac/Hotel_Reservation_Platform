using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;

namespace HotelReservation.Web.Pages.Manager.Hotels
{
    public class CreateModel : PageModel
    {
        private readonly HotelDbContext _context;

        public CreateModel(HotelDbContext context)
        {
            _context = context;
        }

        private int DemoManagerId => 2; // Burayý da ayný Id ile tut

        [BindProperty]
        public Hotel Input { get; set; } = new Hotel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Navigation property validation'ýný atla
            ModelState.Remove("Input.Manager");
            ModelState.Remove("Input.ManagerId"); // extra güvence

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "ModelState geçersiz, lütfen alanlarý kontrol edin.");
                return Page();
            }

            Input.ManagerId = DemoManagerId;
            Input.IsActive = true;
            Input.IsApproved = false;
            Input.CreatedAt = DateTime.UtcNow;

            _context.Hotels.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }


    }
}
