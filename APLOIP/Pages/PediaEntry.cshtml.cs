using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APLOIP.Pages
{
    public class PediaEntryModel : PageModel
    {
        public PediaEntryModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }
        
        public string DisplayTitle { get; private set; }

        public string PageContent { get; private set; }

        public string BasicClassTitle { get; private set; }

        public DateTime CreationTime { get; private set; }

        public DateTime ModificationTime { get; private set; }

        public void OnGetAsync()
        {
            MySqlIntegration selectInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            string[] keys = { "*" };
            List<Dictionary<string, object>> result = selectInteg.MySqlSelect("entries", keys, "title_unique=" + MySqlIntegration.QuoteStr((string)RouteData.Values["title"]));
            if(result.Count > 0)
            {
                DisplayTitle = (string)result[0]["title_display"];
                BasicClassTitle = (string)result[0]["basic_class_title"];
                PageContent = (string)result[0]["content"];
                CreationTime = (DateTime)result[0]["creation_time"];
                var modificationTime = result[0]["modification_time"];
                //ModificationTime = (DateTime)((modificationTime.GetType() != typeof(DBNull)) ?modificationTime : null);
            }
            else
            {
                DisplayTitle = (string)RouteData.Values["title"] + " is not found";
            }
        }
    }
}