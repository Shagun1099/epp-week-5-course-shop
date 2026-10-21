using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Web.Pages.Admin;

[Authorize(Roles = "Administrators")]
public class IndexModel : PageModel
{
    private readonly IReadRepository<CatalogItem> _itemRepository;

    public IndexModel(IReadRepository<CatalogItem> itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public List<CatalogItem> Courses { get; set; } = new();

    public async Task OnGet()
    {
        Courses = (await _itemRepository.ListAsync()).OrderBy(c => c.Name).ToList();
    }
}
