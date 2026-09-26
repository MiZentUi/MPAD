using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain.Entities;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UI.Services.ProductService;

public class ApiProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly int _pageSize;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ApiProductService> _logger;

    public ApiProductService(HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ApiProductService> logger)
    {
        _httpClient = httpClient;
        _pageSize = configuration.GetValue<int>("ItemsPerPage");
        _serializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _logger = logger;
    }

    public async Task<ResponseData<ListModel<Product>>> GetProductListAsync(
        string? categoryNormalizedName, int pageNo = 1)
    {
        var urlString = new StringBuilder($"{_httpClient.BaseAddress.AbsoluteUri}");

        if (categoryNormalizedName != null)
        {
            urlString.Append($"{categoryNormalizedName}/");
        }

        var queryParams = new List<KeyValuePair<string, string>>();

        if (pageNo > 1)
        {
            queryParams.Add(new("pageNo", pageNo.ToString()));
        }

        if (_pageSize != 3)
        {
            queryParams.Add(new("pageSize", _pageSize.ToString()));
        }

        if (queryParams.Count > 0)
        {
            urlString.Append(QueryString.Create(queryParams));
        }

        var response = await _httpClient.GetAsync(
            new Uri(urlString.ToString()));

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content
                    .ReadFromJsonAsync<ResponseData<ListModel<Product>>>(_serializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError("-----> Error: {Message}", ex.Message);
                return ResponseData<ListModel<Product>>
                    .Error($"Error: {ex.Message}");
            }
        }

        _logger.LogError("-----> Error: {StatusCode}",
            response.StatusCode);
        return ResponseData<ListModel<Product>>
            .Error($"Error: {response.StatusCode}");
    }

    public async Task<ResponseData<Product>> GetProductByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}{id}");

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content
                    .ReadFromJsonAsync<ResponseData<Product>>(_serializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError("-----> Error: {Message}", ex.Message);
                return ResponseData<Product>.Error($"Error: {ex.Message}");
            }
        }

        _logger.LogError("-----> Error: {StatusCode}",
            response.StatusCode);
        return ResponseData<Product>
            .Error($"Error: {response.StatusCode}");
    }

    public async Task<ResponseData<Product>> CreateProductAsync(Product product, IFormFile? formFile)
    {
        product.Image = "images/noimage.jpg";
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = _httpClient.BaseAddress
        };

        var content = new MultipartFormDataContent();
        if (formFile is not null)
        {
            var streamContent = new StreamContent(formFile.OpenReadStream());
            content.Add(streamContent, "file", formFile.FileName);
        }
        content.Add(new StringContent(JsonSerializer.Serialize(product, _serializerOptions)),
            "product");
        request.Content = content;

        var response = await _httpClient.SendAsync(request, CancellationToken.None);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content
                .ReadFromJsonAsync<ResponseData<Product>>(_serializerOptions);
            return responseData;
        }

        _logger.LogError("-----> object not created. Error: {StatusCode}",
            response.StatusCode);
        return ResponseData<Product>
            .Error($"Object not created. Error: {response.StatusCode}");
    }

    public async Task UpdateProductAsync(int id, Product product, IFormFile? formFile)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{_httpClient.BaseAddress}{id}")
        };

        var content = new MultipartFormDataContent();
        if (formFile is not null)
        {
            var streamContent = new StreamContent(formFile.OpenReadStream());
            content.Add(streamContent, "file", formFile.FileName);
        }
        content.Add(new StringContent(JsonSerializer.Serialize(product, _serializerOptions)),
            "product");
        request.Content = content;

        await _httpClient.SendAsync(request, CancellationToken.None);
    }

    public async Task DeleteProductAsync(int id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{_httpClient.BaseAddress}{id}")
        };

        await _httpClient.SendAsync(request, CancellationToken.None);
    }
}
