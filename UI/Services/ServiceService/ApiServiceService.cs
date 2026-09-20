using System.Net.Http.Json;
using System.Text.Json;
using Domain.Entities;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace UI.Services.ServiceService;

public class ApiServiceService : IServiceService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ApiServiceService> _logger;

    public ApiServiceService(HttpClient httpClient,
        ILogger<ApiServiceService> logger)
    {
        _httpClient = httpClient;
        _serializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _logger = logger;
    }

    public async Task<ResponseData<List<Service>>> GetServiceListAsync()
    {
        var response = await _httpClient.GetAsync(_httpClient.BaseAddress);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content
                    .ReadFromJsonAsync<ResponseData<List<Service>>>(_serializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError("-----> Error: {Message}", ex.Message);
                return ResponseData<List<Service>>.Error($"Error: {ex.Message}");
            }
        }

        _logger.LogError("-----> Error: {StatusCode}",
            response.StatusCode);
        return ResponseData<List<Service>>
            .Error($"Error: {response.StatusCode}");
    }
}
