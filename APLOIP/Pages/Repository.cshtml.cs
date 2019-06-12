using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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
            if (result.Count > 0)
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
                    string specifier = string.Format("entry_title={0} AND operator_type={1}", MySqlIntegration.QuoteStr(entry.UniqueTitle), MySqlIntegration.QuoteStr(Operator.File));
                    sqlInteg.MySqlSelect("repo_key_values", keys, specifier).ForEach((ele) =>
                    {
                        if (ele["key_title"].Equals("file_description"))
                        {
                            entry.Description = (string)ele["value"];
                        }
                    });
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
            public string Description { get; set; }
            public string Owner { get; set; }

        }
    }
}