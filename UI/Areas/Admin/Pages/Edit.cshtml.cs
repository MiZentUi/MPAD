using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace UI.Areas.Admin.Pages
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IServiceService _serviceService;

        public EditModel(IProductService productService, IServiceService serviceService)
        {
            _productService = productService;
            _serviceService = serviceService;
        }

        public List<Service> Services { get; set; } = new();

        [BindProperty]
        public Product Product { get; set; } = default!;

        [BindProperty]
        public IFormFile? Image { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var response = await _productService.GetProductByIdAsync(id.Value);

            if (!response.Successfull || response.Data is null)
            {
                return NotFound();
            }

            Product = response.Data;

            await LoadServicesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadServicesAsync();
                return Page();
            }

            await _productService.UpdateProductAsync(Product.Id, Product, Image);

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
