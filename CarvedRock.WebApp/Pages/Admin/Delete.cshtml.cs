using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarvedRock.WebApp.Pages.Admin;

public class DeleteModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    public void OnGet()
    {
    }
}
