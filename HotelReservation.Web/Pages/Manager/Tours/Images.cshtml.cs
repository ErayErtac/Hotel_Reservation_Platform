using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Web.Pages.Manager.Tours
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

        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [BindProperty(SupportsGet = true)]
        public int TourId { get; set; }

        public Tour? Tour { get; set; }
        public IQueryable<TourImage> Images { get; set; } =
            Enumerable.Empty<TourImage>().AsQueryable();

        [BindProperty]
        public IFormFile[] UploadFiles { get; set; } = Array.Empty<IFormFile>();

        public async Task<IActionResult> OnGetAsync(int tourId)
        {
            TourId = tourId;

            Tour = await _context.Tours
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == tourId && t.ManagerId == CurrentUserId);

            if (Tour == null)
            {
                return NotFound();
            }

            Images = Tour.Images.AsQueryable().OrderByDescending(i => i.UploadedAt);

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            Tour = await _context.Tours
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == TourId && t.ManagerId == CurrentUserId);

            if (Tour == null)
            {
                return NotFound();
            }

            if (UploadFiles == null || UploadFiles.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen en az bir dosya seçin.");
                Images = Tour.Images.AsQueryable();
                return Page();
            }

            var uploadsRootFolder = Path.Combine(
                _env.WebRootPath, "uploads", "tours", TourId.ToString());

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

                var relativePath = $"/uploads/tours/{TourId}/{fileName}";

                var image = new TourImage
                {
                    TourId = TourId,
                    ImagePath = relativePath,
                    IsMain = !Tour.Images.Any(), // ilk resim ise ana foto
                    UploadedAt = DateTime.UtcNow
                };

                _context.TourImages.Add(image);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { tourId = TourId });
        }

        public async Task<IActionResult> OnPostSetMainAsync(int imageId)
        {
            Tour = await _context.Tours
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == TourId && t.ManagerId == CurrentUserId);

            if (Tour == null)
            {
                return NotFound();
            }

            var image = Tour.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound();
            }

            foreach (var img in Tour.Images)
            {
                img.IsMain = (img.Id == imageId);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { tourId = TourId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int imageId)
        {
            Tour = await _context.Tours
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == TourId && t.ManagerId == CurrentUserId);

            if (Tour == null)
            {
                return NotFound();
            }

            var image = Tour.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound();
            }

            // Dosyayı diskten sil
            var fullPath = Path.Combine(
                _env.WebRootPath,
                image.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.TourImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { tourId = TourId });
        }
    }
}

