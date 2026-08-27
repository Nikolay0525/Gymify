using FluentValidation;
using Gymify.Application.DTOs.Image;
using Gymify.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Gymify.Web.Controllers
{
    [Authorize]
    public class ImageController : BaseController
    {
        private readonly IImageService _imageService;
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageController(IImageService imageService, IWebHostEnvironment webHostEnvironment, IItemService itemService)
        {
            _imageService = imageService;
            _webHostEnvironment = webHostEnvironment;
            _itemService = itemService;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _imageService.GetAllImagesAsync();
            return View(images);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string fileName, string title)
        {
            if (file == null || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all fields and choose a file.");
                return View();
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName);
                var urlPath = $"/Images/{fileName}{fileExtension}";
                var localPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", $"{fileName}{fileExtension}");

                Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

                await using (var stream = new FileStream(localPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUploadModel = new ImageUploadModel(default, default, default, default, default, default)
                {
                    FileName = fileName,
                    FileExtension = fileExtension,
                    Title = title,
                    LocalPath = localPath,
                    UrlPath = urlPath,
                    FileContent = await ConvertFileToByteArrayAsync(file)
                };

                await _imageService.CreateImageAsync(imageUploadModel);

                TempData["SuccessMessage"] = "Image uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFromComponent(IFormFile file)
        {
            if (file == null)
                return BadRequest("File not provided.");

            var userId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);

            var httpRequest = HttpContext.Request;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            var fileExtension = Path.GetExtension(file.FileName);
            var title = fileNameWithoutExt;

            var urlPath = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/Images/{fileNameWithoutExt}{fileExtension}";
            var localPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", file.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

            await using (var stream = new FileStream(localPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUploadModel = new ImageUploadModel(
                fileNameWithoutExt,
                fileExtension,
                title,
                localPath,
                urlPath,
                await ConvertFileToByteArrayAsync(file)
            );

            var createdImage = await _itemService.CreateCustomAvatarAsync(userId, imageUploadModel, IsUkrainian);

            return Json(new { success = true, url = createdImage.ImageURL , id = createdImage.Id});
        }

        private async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
