namespace LR1_3.UI.Services.ServiceService;

public interface IServiceService
{
    /// <summary>
    /// Получение списка всех услуг
    /// </summary>
    /// <returns></returns>
    public Task<ResponseData<List<Service>>> GetServiceListAsync();
}