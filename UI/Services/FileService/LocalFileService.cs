using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;

namespace UI.Services.FileService;

/// <summary>
/// Локальный файловый сервис для сохранения аватар пользователей
/// </summary>
public class LocalFileService(IWebHostEnvironment environment) : IFileService
{
    private readonly IWebHostEnvironment _environment = environment;

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return "/images/default-profile-picture.png";
        }

        var fileName = $"{Guid.NewGuid()}_{HtmlEncoder.Default.Encode(file.FileName)}";
        
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
        
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/Images/{fileName}";
    }
}