using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Rooms
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

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        [BindProperty]
        public Room Input { get; set; } = new Room();

        public Hotel? Hotel { get; set; }

        public async Task<IActionResult> OnGetAsync(int hotelId)
        {
            HotelId = hotelId;

            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.ManagerId == CurrentUserId);

            if (Hotel == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.Hotel");

            // HotelId hidden'dan geliyor
            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.ManagerId == CurrentUserId);

            if (Hotel == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Input.HotelId = HotelId;
            Input.IsActive = true;

            _context.Rooms.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { hotelId = HotelId });
        }
    }
}
