using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Api.Use_Cases;

public sealed record SaveImage(IFormFile file) : IRequest<string>;

public class SaveImageHandler(
    IWebHostEnvironment env,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<SaveImage, string>
{
    public async Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
    {
        var imagesFolder = Path.Combine(env.WebRootPath, "images");
        Directory.CreateDirectory(imagesFolder);

        var extension = Path.GetExtension(request.file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(imagesFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.file.CopyToAsync(stream, cancellationToken);
        }

        var httpContext = httpContextAccessor.HttpContext;
        var host = httpContext is not null
            ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
            : string.Empty;

        return $"{host}/images/{fileName}";
    }
}
