using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APLOIP.Pages
{
    public class SearchModel : PageModel
    {
        [Required]
        [BindProperty(Name = "Query", SupportsGet = true)]
        public string Query { get; set; }
        [BindProperty(Name = "PageNum", SupportsGet = true)]
        public int PageNum { get; set; } = 0;
        public int ResultLimit { get; private set; } = 1500;
        public int PageLimit { get; private set; }
        public SearchModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }
        public List<Entry> ResultEntries { get; private set; }
        public List<BasicClass> BasicClasses { get; private set; }
        public void OnGet()
        {
            MySqlIntegration sqlInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            BasicClasses = new List<BasicClass>();
            //选取所有基础类型
            string[] keysBasicClass = { "ID", "title_unique", "title_display", "fa_icon" };
            if (sqlInteg.MySqlSelect("basic_class", keysBasicClass).Count > 0)
            {
                sqlInteg.IntegratedResult.ForEach(basicClassObj =>
                {
                    BasicClasses.Add(new BasicClass
                    {
                        ID = (int)basicClassObj["ID"],
                        UniqueTitle = (string)basicClassObj["title_unique"],
                        DisplayTitle = (string)basicClassObj["title_display"],
                        FaIcon = (string)basicClassObj["fa_icon"]
                    });
                });
            }
            ResultEntries = new List<Entry>();
            if (Query != null)
            {
                Query = Query.Replace("\\", "\\\\");
                Query = Query.Replace("\'", "\\\'");
                string queryString = "SELECT * FROM entries WHERE LOWER(title_unique) LIKE LOWER('%{0}%') UNION SELECT * FROM entries WHERE LOWER(title_display) LIKE LOWER('%{0}%') UNION SELECT * FROM entries WHERE LOWER(content) LIKE LOWER('%{0}%');";
                if (sqlInteg.MySqlQuery(string.Format(queryString, Query)).Count > 0)
                    sqlInteg.IntegratedResult.ForEach(obj =>
                    {
                        ResultEntries.Add(new Entry
                        {
                            DisplayTitle = Convert.ToString(obj?["title_display"]),
                            UniqueTitle = Convert.ToString(obj?["title_unique"]),
                            BasicClassID = Convert.ToInt32(obj?["basic_class_ID"]),
                            PageContent = Convert.ToString(obj?["content"]),
                            CreationTime = Convert.ToDateTime(obj?["creation_time"]),
                            ModificationTime = Convert.ToDateTime(obj?["modification_time"]),
                        });
                    });
                if (PageNum > ResultEntries.Count / ResultLimit || PageNum < 0)
                {
                    PageNum = 0;
                }
                PageLimit = ResultEntries.Count - PageNum * ResultLimit;
                //if (PageLimit > 15) PageLimit = 15;
            }
        }
    }
}