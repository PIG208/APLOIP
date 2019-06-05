using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace APLOIP.Pages
{
    public class PediaEntryModel : PageModel
    {
        public PediaEntryModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }

        public static string DisplayTitle { get; private set; }

        public static string PageContent { get; private set; }

        public static int BasicClassID { get; private set; }

        public static string BasicClassTitle { get; private set; }

        public static DateTime CreationTime { get; private set; }

        public static DateTime ModificationTime { get; private set; }

        public string SaveContent { get; private set; }

        public void OnGetAsync()
        {
            MySqlIntegration selectInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            string[] keys = { "*" };
            if(selectInteg.MySqlSelect("entries", keys, "title_unique=" + MySqlIntegration.QuoteStr((string)RouteData.Values["title"])).Count > 0)
            {
                var obj = selectInteg.IntegratedResult;
                DisplayTitle = (string)obj[0]["title_display"];
                BasicClassID = (int)obj[0]["basic_class_ID"];
                PageContent = (string)obj[0]["content"];
                CreationTime = (DateTime)obj[0]["creation_time"];
                var modificationTime = obj[0]["modification_time"];
                //ModificationTime = (DateTime)((modificationTime.GetType() != typeof(DBNull)) ?modificationTime : null);
                string[] keysBasicClass = { "title_display" };
                if(selectInteg.MySqlSelect("basic_class", keysBasicClass, "ID=" + BasicClassID).Count > 0)
                {
                    if (selectInteg.IntegratedResult[0]["title_display"] != null)
                    {
                        BasicClassTitle = (string)selectInteg.IntegratedResult[0]["title_display"];
                    }
                }
            }
            else
            {
                DisplayTitle = (string)RouteData.Values["title"] + " is not found";
            }
        }

        public ActionResult OnPostSave()
        {
            MemoryStream memoryStream = new MemoryStream();
            Request.Body.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                string result = reader.ReadToEnd();
                //var data = JsonConvert.DeserializeObject<PostData>(result);
                if (result.Length > 0 || !result.Trim().Equals(""))
                    SaveContent = result;
                else
                    return new JsonResult(JsonConvert.SerializeObject(0));
            }
            string[] keySelect = { "title_unique" };
            MySqlIntegration postInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            postInteg.MySqlSelect("entries", keySelect, "title_unique=" + MySqlIntegration.QuoteStr((string)RouteData.Values["title"]));
            string[] keysSave = { "title_unique", "basic_class_ID", "title_display", "content" };
            int lineAffected;
            if (postInteg.IntegratedResult.Count > 0)
            {
                lineAffected = postInteg.MySqlUpdate("entries", keysSave, "title_unique=" + MySqlIntegration.QuoteStr((string)RouteData.Values["title"]), (string)RouteData.Values["title"], BasicClassID, DisplayTitle, SaveContent);
            }
            else
            {
                lineAffected = postInteg.MySqlInsert("entries", keysSave, (string)RouteData.Values["title"], BasicClassID, DisplayTitle, SaveContent);
            }
            return new JsonResult(JsonConvert.SerializeObject(lineAffected));
        }
    }
}