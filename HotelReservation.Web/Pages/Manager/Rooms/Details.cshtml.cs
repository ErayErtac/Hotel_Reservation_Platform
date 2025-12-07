using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Manager.Rooms
{
    public class DetailsModel : PageModel
    {
        private readonly HotelDbContext _context;

        public DetailsModel(HotelDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        public Room? Room { get; set; }
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

            Room = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Amenities)
                .Include(r => r.Reservations)
                    .ThenInclude(res => res.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && r.HotelId == hotelId);

            if (Room == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

