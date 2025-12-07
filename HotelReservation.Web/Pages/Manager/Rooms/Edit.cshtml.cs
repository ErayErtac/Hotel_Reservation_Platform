using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Rooms
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

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        [BindProperty]
        public Room Input { get; set; } = new Room();

        public Hotel? Hotel { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, int hotelId)
        {
            Id = id;
            HotelId = hotelId;

            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.ManagerId == CurrentUserId);

            if (Hotel == null)
            {
                return NotFound();
            }

            Input = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id && r.HotelId == hotelId);

            if (Input == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.Hotel");
            ModelState.Remove("Input.HotelId");

            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.ManagerId == CurrentUserId);

            if (Hotel == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == Id && r.HotelId == HotelId);

            if (room == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Input = room;
                return Page();
            }

            room.RoomNumber = Input.RoomNumber;
            room.Capacity = Input.Capacity;
            room.PricePerNight = Input.PricePerNight;
            room.Description = Input.Description;
            room.IsActive = Input.IsActive;
            room.RoomTypeId = Input.RoomTypeId;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Oda başarıyla güncellendi.";
            return RedirectToPage("./Index", new { hotelId = HotelId });
        }
    }
}

