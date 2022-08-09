using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationDemo.Pages
{
    [Authorize]
    public class MyCompaniesModel : PageModel
    {
        public List<string> Companies = new List<string>();

        public void OnGet()
        {
            var list = User.Claims.Where(c => c.Type == "CompanyId");
            foreach (var comp in list)
            {
                Companies.Add(comp.Value);
            }
        }
    }
}
