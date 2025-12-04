using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Manager.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        private int DemoManagerId => 2;

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        public Hotel? Hotel { get; set; }
        public IList<Room> Rooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync(int hotelId)
        {
            HotelId = hotelId;

            // Güvenlik: sadece demo yöneticinin oteliyse izin ver
            Hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.ManagerId == DemoManagerId);

            if (Hotel == null)
            {
                return NotFound();
            }

            Rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return Page();
        }
    }
}
