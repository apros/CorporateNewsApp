using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CorporateNews.Web.Services;

public class IndexModel : PageModel
{
    private readonly SemanticKernelService _semanticKernelService;

    [BindProperty]
    public string UserQuery { get; set; }

    public string Result { get; set; }

    public IndexModel(SemanticKernelService semanticKernelService)
    {
        _semanticKernelService = semanticKernelService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(UserQuery))
        {
            return Page();
        }

        Result = await _semanticKernelService.ProcessQueryAsync(UserQuery);
        return Page();
    }

    public IActionResult OnPostReset()
    {
        _semanticKernelService.ResetConversation();
        return RedirectToPage();
    }
}