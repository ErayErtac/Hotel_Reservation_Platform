using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Admin.Hotels
{
    public class IndexModel : PageModel
    {
        private readonly HotelDbContext _context;

        public IndexModel(HotelDbContext context)
        {
            _context = context;
        }

        public IList<HotelSummary> Hotels { get; set; } = new List<HotelSummary>();

        public void OnGet()
        {
            // vw_HotelSummary view'inden çekiyoruz
            Hotels = _context.HotelSummaries
                .FromSqlRaw("SELECT * FROM dbo.vw_HotelSummary")
                .OrderBy(h => h.City)
                .ThenBy(h => h.Name)
                .ToList();
        }
    }
}
