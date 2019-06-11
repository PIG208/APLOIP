using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace APLOIP.Pages
{
    public class RepositoryModel : PageModel
    {
        public RepositoryModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }
        public List<RepoEntry> RepoEntries = new List<RepoEntry>();
        public void OnGet()
        {
            MySqlIntegration sqlInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            string[] keys = { "*" };
            var result = sqlInteg.MySqlSelect("repo_entries", keys);
            if(result.Count > 0)
            {
                result.ForEach(element =>
                {
                    var entry = new RepoEntry
                    {
                        UniqueTitle = (string)element["title_unique"],
                        DisplayTitle = (string)element["title_display"],
                        OperatiorType = (string)element["operator_type"],
                        CreationTime = (DateTime)element["creation_time"],
                        Owner = (string)element["owner"]
                    };
                    RepoEntries.Add(entry);
                });
            }
        }
        public class RepoEntry
        {
            public string UniqueTitle { get; set; }
            public string DisplayTitle { get; set; }
            public string OperatiorType { get; set; }
            public DateTime CreationTime { get; set; }
            public string Owner { get; set; }

        }
    }
}