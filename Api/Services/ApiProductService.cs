using Domain.Models;
using Api.Services.Authentication;

namespace Api.Services;

/// <summary>
/// Сервис для работы с продуктами через API с авторизацией Keycloak
/// </summary>
public class ApiProductService
{
    private readonly ITokenAccessor _tokenAccessor;
    private readonly HttpClient _httpClient;

    public ApiProductService(ITokenAccessor tokenAccessor, HttpClient httpClient)
    {
        _tokenAccessor = tokenAccessor;
        _httpClient = httpClient;
    }

    public async Task<ResponseData<string>> CreateProductAsync(string productData)
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, false);

            var response = await _httpClient.PostAsync(
                "api/product",
                new StringContent(productData),
                cancellationToken: default);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ResponseData<string>.Error($"Ошибка при создании продукта: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return ResponseData<string>.Success(result);
        }
        catch (Exception e)
        {
            return ResponseData<string>.Error($"Объект не добавлен. Error: {e.Message}");
        }
    }

    public async Task<ResponseData<string>> GetProductsAsync()
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, false);

            var response = await _httpClient.GetAsync("api/product", cancellationToken: default);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ResponseData<string>.Error($"Ошибка при получении продуктов: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return ResponseData<string>.Success(result);
        }
        catch (Exception e)
        {
            return ResponseData<string>.Error($"Объект не добавлен. Error: {e.Message}");
        }
    }

    public async Task<ResponseData<string>> UpdateProductAsync(string productData)
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, false);

            var response = await _httpClient.PutAsync(
                "api/product",
                new StringContent(productData),
                cancellationToken: default);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ResponseData<string>.Error($"Ошибка при обновлении продукта: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return ResponseData<string>.Success(result);
        }
        catch (Exception e)
        {
            return ResponseData<string>.Error($"Объект не добавлен. Error: {e.Message}");
        }
    }

    public async Task<ResponseData<string>> DeleteProductAsync(int productId)
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, false);

            var response = await _httpClient.DeleteAsync($"api/product/{productId}", cancellationToken: default);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ResponseData<string>.Error($"Ошибка при удалении продукта: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return ResponseData<string>.Success(result);
        }
        catch (Exception e)
        {
            return ResponseData<string>.Error($"Объект не добавлен. Error: {e.Message}");
        }
    }
}