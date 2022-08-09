using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationDemo.Pages
{
    [Authorize]
    public class CompanyDetailsModel : PageModel
    {
        private readonly ILogger<CompanyDetailsModel> _logger;

        public CompanyDetailsModel(ILogger<CompanyDetailsModel> logger)
        {
            _logger = logger;
        }

        private Dictionary<string, CompDetail> _details = new Dictionary<string, CompDetail>
        {
            { "1234", new CompDetail {Name = "Acme Blasting", Description = "Road runner, anyone?", Location = "Minnesota" } },
            { "7890", new CompDetail {Name = "Mars Exploration", Description = "Let's go to Mars", Location = "Wisconsin" } }
        };

        public CompDetail SelectedCompany { get; set; }

        public IActionResult OnGet()
        {
            var userHasAccessToCompany = User.Claims.Any(c => c.Type == "CompanyId" &&
                                                              c.Value == Request.Query["cid"]);

            if (!userHasAccessToCompany)
            {
                _logger.LogWarning("Unauthorized access attempted!!!");
                TempData["AuthWarning"] = $"You don't have access to company {Request.Query["cid"]}";
                return RedirectToPage("MyCompanies");
            }

            SelectedCompany = _details[Request.Query["cid"]];
            return Page();
        }
    }

    public class CompDetail
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}
