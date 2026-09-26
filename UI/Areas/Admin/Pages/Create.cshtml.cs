using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace UI.Areas.Admin.Pages
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IServiceService _serviceService;

        public CreateModel(IProductService productService, IServiceService serviceService)
        {
            _productService = productService;
            _serviceService = serviceService;
        }

        public List<Service> Services { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadServicesAsync();
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; } = default!;

        [BindProperty]
        public IFormFile? Image { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadServicesAsync();
                return Page();
            }

            var response = await _productService.CreateProductAsync(Product, Image);

            if (!response.Successfull)
            {
                ModelState.AddModelError(string.Empty,
                    response.ErrorMessage ?? "Object not created");
                await LoadServicesAsync();
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadServicesAsync()
        {
            var result = await _serviceService.GetServiceListAsync();
            Services = result.Successfull && result.Data is not null
                ? result.Data
                : new List<Service>();
        }
    }
}
