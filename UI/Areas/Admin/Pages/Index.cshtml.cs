using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Services.ProductService;

namespace UI.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IList<Product> Product { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public async Task OnGetAsync(int pageNo = 1)
        {
            var response = await _productService.GetProductListAsync(null, pageNo);

            if (response.Successfull && response.Data is not null)
            {
                Product = response.Data.Items;
                CurrentPage = response.Data.CurrentPage;
                TotalPages = response.Data.TotalPages;
            }
            else
            {
                Product = new List<Product>();
            }
        }
    }
}