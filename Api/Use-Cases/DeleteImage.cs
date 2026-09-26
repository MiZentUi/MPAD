using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace Api.Use_Cases;

public sealed record DeleteImage(string? ImageUrl) : IRequest;

public class DeleteImageHandler(IWebHostEnvironment env) : IRequestHandler<DeleteImage>
{
    public Task Handle(DeleteImage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            return Task.CompletedTask;
        }

        var rawPath = request.ImageUrl;
        if (Uri.TryCreate(rawPath, UriKind.Absolute, out var absoluteUri))
        {
            rawPath = absoluteUri.LocalPath;
        }
        else
        {
            var queryIndex = rawPath.IndexOf('?');
            if (queryIndex >= 0)
            {
                rawPath = rawPath[..queryIndex];
            }
        }

        var fileName = Path.GetFileName(rawPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.CompletedTask;
        }

        var imagesFolder = Path.Combine(env.WebRootPath, "images");
        var filePath = Path.Combine(imagesFolder, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
