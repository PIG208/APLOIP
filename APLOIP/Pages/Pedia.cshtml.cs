using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Diagnostics;

namespace APLOIP.Pages
{
    public class PediaModel : PageModel
    {
        public PediaModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public int Result { get; private set; }

        public List<BasicClass> BasicClasses { get; private set; }

        public void OnGetAsync()
        {
            MySqlIntegration selectInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            BasicClasses = new List<BasicClass>();
            string[] keys = { "ID", "title_unique", "title_display", "description", "fa_icon", "style" };
            selectInteg.MySqlSelect("basic_class", keys).ForEach(obj =>
            {
                BasicClass basicClass = new BasicClass
                {
                    ID = (int)obj["ID"],
                    UniqueTitle = (string)obj["title_unique"],
                    DisplayTitle = (string)obj["title_display"],
                    Description = (string)obj["description"],
                    FaIcon = (string)obj["fa_icon"],
                    Style = (int)obj["style"]
                };
                BasicClasses.Add(basicClass);
            });
            Debug.WriteLine(Request.QueryString);
        }
    }
}