namespace LR1_3.UI.Services.ServiceService;

public class MemoryServiceService : IServiceService
{
    public Task<ResponseData<List<Service>>> GetServiceListAsync()
    {
        var services = new List<Service>
        {
            new() {Id=1, Name="Cards", NormalizedName="cards"},
            new() {Id=2, Name="Deposits", NormalizedName="deposits"},
            new() {Id=3, Name="Loans", NormalizedName="loans"},
            new() {Id=4, Name="Bonds", NormalizedName="bonds"},
            new() {Id=5, Name="Shares", NormalizedName="shares"},
        };
        var result = ResponseData<List<Service>>.Success(services);
        return Task.FromResult(result);
    }
}