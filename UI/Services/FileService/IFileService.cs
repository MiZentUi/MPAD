using Microsoft.AspNetCore.Http;

namespace UI.Services.FileService;

/// <summary>
/// Интерфейс для сохранения файлов
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Сохраняет данные в файле
    /// </summary>
    /// <param name="file">данные для сохранения</param>
    /// <returns>адрес сохраненного файла</returns>
    Task<string> SaveFileAsync(IFormFile file);
}