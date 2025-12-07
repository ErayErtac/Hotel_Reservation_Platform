using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Tours
{
    public class EditModel : PageModel
    {
        private readonly HotelDbContext _context;

        public EditModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [BindProperty]
        public Tour Input { get; set; } = new Tour();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == id && t.ManagerId == CurrentUserId);

            if (tour == null)
            {
                return NotFound();
            }

            Input = tour;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Navigation property validation'ı atla
            ModelState.Remove("Input.Manager");
            ModelState.Remove("Input.ManagerId");
            ModelState.Remove("Input.CreatedAt");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Tarih kontrolü
            if (Input.EndDate <= Input.StartDate)
            {
                ModelState.AddModelError("Input.EndDate", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                return Page();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.Id == Input.Id && t.ManagerId == CurrentUserId);

            if (tour == null)
            {
                return NotFound();
            }

            tour.Name = Input.Name;
            tour.City = Input.City;
            tour.Description = Input.Description;
            tour.Itinerary = Input.Itinerary;
            tour.StartDate = Input.StartDate;
            tour.EndDate = Input.EndDate;
            tour.Price = Input.Price;
            tour.Capacity = Input.Capacity;
            tour.IsActive = Input.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

