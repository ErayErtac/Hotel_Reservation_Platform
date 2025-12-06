using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Pages.Manager.Hotels
{
    public class ImagesModel : PageModel
    {
        private readonly HotelDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImagesModel(HotelDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int DemoManagerId => 2; // Manager Id (SSMS'ten gerekirse deðiþtir)

        [BindProperty(SupportsGet = true)]
        public int HotelId { get; set; }

        public Hotel? Hotel { get; set; }
        public IQueryable<HotelImage> Images { get; set; } = Enumerable.Empty<HotelImage>().AsQueryable();

        [BindProperty]
        public IFormFile[] UploadFiles { get; set; } = Array.Empty<IFormFile>();

        public async Task<IActionResult> OnGetAsync(int hotelId)
        {
            HotelId = hotelId;

            Hotel = await _context.Hotels
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.ManagerId == DemoManagerId);

            if (Hotel == null)
            {
                return NotFound();
            }

            Images = Hotel.Images.AsQueryable().OrderByDescending(i => i.UploadedAt);

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            // ayný sayfaya geri döndüðümüzde oteli tekrar yüklemek için
            Hotel = await _context.Hotels
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.ManagerId == DemoManagerId);

            if (Hotel == null)
            {
                return NotFound();
            }

            if (UploadFiles == null || UploadFiles.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen en az bir dosya seçin.");
                Images = Hotel.Images.AsQueryable();
                return Page();
            }

            var uploadsRootFolder = Path.Combine(_env.WebRootPath, "uploads", "hotels", HotelId.ToString());

            if (!Directory.Exists(uploadsRootFolder))
            {
                Directory.CreateDirectory(uploadsRootFolder);
            }

            foreach (var file in UploadFiles)
            {
                if (file.Length <= 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsRootFolder, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/hotels/{HotelId}/{fileName}";

                var image = new HotelImage
                {
                    HotelId = HotelId,
                    ImagePath = relativePath,
                    IsMain = !Hotel.Images.Any(), // ilk resim ise ana yap
                    UploadedAt = DateTime.UtcNow
                };

                _context.HotelImages.Add(image);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { hotelId = HotelId });
        }

        public async Task<IActionResult> OnPostSetMainAsync(int imageId)
        {
            Hotel = await _context.Hotels
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.ManagerId == DemoManagerId);

            if (Hotel == null)
            {
                return NotFound();
            }

            var image = Hotel.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound();
            }

            foreach (var img in Hotel.Images)
            {
                img.IsMain = img.Id == imageId;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { hotelId = HotelId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int imageId)
        {
            Hotel = await _context.Hotels
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.Id == HotelId && h.ManagerId == DemoManagerId);

            if (Hotel == null)
            {
                return NotFound();
            }

            var image = Hotel.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound();
            }

            // Dosyayý diskten sil
            var fullPath = Path.Combine(_env.WebRootPath, image.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.HotelImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { hotelId = HotelId });
        }
    }
}
