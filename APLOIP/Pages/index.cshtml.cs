using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace APLOIP.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; private set; }

        public void OnGet()
        {
        }
    }
}