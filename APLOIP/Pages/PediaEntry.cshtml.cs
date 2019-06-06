using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace APLOIP.Pages
{
    public class PediaEntryModel : PageModel
    {
        public PediaEntryModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }
        public static List<BasicClass> BasicClasses { get; set; }
        public Entry PageEntry { get; set; }

        public void OnGetAsync()
        {
            MySqlIntegration selectInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            BasicClasses = new List<BasicClass>();
            //选取所有基础类型
            string[] keysBasicClass = { "ID", "title_unique", "title_display" };
            if (selectInteg.MySqlSelect("basic_class", keysBasicClass).Count > 0)
            {
                selectInteg.IntegratedResult.ForEach(basicClassObj =>
                {
                    BasicClasses.Add(new BasicClass
                    {
                        ID = (int)basicClassObj["ID"],
                        UniqueTitle = (string)basicClassObj["title_unique"],
                        DisplayTitle = (string)basicClassObj["title_display"]
                    });
                });
            }
            //选取对应的类
            string[] keys = { "*" };
            if (selectInteg.MySqlSelect("entries", keys, "title_unique=" + MySqlIntegration.QuoteStr((string)RouteData.Values["title"])).Count > 0)
            {
                var obj = selectInteg.IntegratedResult;
                PageEntry = new Entry()
                {
                    UniqueTitle = (string)RouteData.Values["title"],
                    DisplayTitle = (string)obj[0]["title_display"],
                    BasicClassID = (int)obj[0]["basic_class_ID"],
                    PageContent = (string)obj[0]["content"],
                    CreationTime = (DateTime)obj[0]["creation_time"],
                    //ModificationTime = (DateTime)(obj[0]["modification_time"]??new DateTime())
                };
                //var modificationTime = obj[0]["modification_time"];
            }
            else
            {
                PageEntry = new Entry()
                {
                    UniqueTitle = (string) RouteData.Values["title"],
                    PageContent = "<div class='paragraph tempintro'>Click edit to create the entry \"" + RouteData.Values["title"] + "\"</div>"
                };
                //通过比较UniqueTitle判断当前路径下的词条是否为基础类，若是，DisplayTitle采用BasicClass.DisplayTitle的值
                if(IsBasicEntry(PageEntry))
                {
                    BasicClass tempBasicClass = BasicClasses.Find(basicClassObj => basicClassObj.UniqueTitle == PageEntry.UniqueTitle);
                    PageEntry.DisplayTitle = tempBasicClass?.DisplayTitle;
                    PageEntry.BasicClassID = tempBasicClass.ID;
                }
                else
                {
                    PageEntry.DisplayTitle = PageEntry.UniqueTitle;
                }
            }
        }

        public ActionResult OnPostSaveSettings()
        {
            MemoryStream memoryStream = new MemoryStream();
            Request.Body.CopyTo(memoryStream);
            memoryStream.Position = 0;
            Entry settings;
            using (StreamReader streamReader = new StreamReader(memoryStream))
            {
                string result = streamReader.ReadToEnd();
                if (result.Length > 0 || !result.Trim().Equals(""))
                    settings = JsonConvert.DeserializeObject<Entry>(result);
                else
                    return new JsonResult(JsonConvert.SerializeObject(State.INVAILD_DATA));
            }
            MySqlIntegration updateInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            string[] updateKeys = { "title_display", "basic_class_ID" };
            string[] selectKey = { "title_unique" };
            //判断该词条是否为基础类词条
            if (IsBasicEntry(settings))
            {
                settings.BasicClassID = BasicClasses.Find(basicClassObj => basicClassObj.UniqueTitle == settings.UniqueTitle).ID;
            }
            if(updateInteg.MySqlSelect("entries", selectKey, "title_unique=" + MySqlIntegration.QuoteStr(settings.UniqueTitle)).Count > 0)
                return new JsonResult(JsonConvert.SerializeObject((updateInteg.MySqlUpdate("entries", updateKeys, "title_unique=" + MySqlIntegration.QuoteStr(settings.UniqueTitle), settings.DisplayTitle, settings.BasicClassID)==1)?State.SUCCESS:State.NO_CHANGE));
            else
                return new JsonResult(JsonConvert.SerializeObject(State.NO_RECORD));
        }

        public ActionResult OnPostSave()
        {
            MemoryStream memoryStream = new MemoryStream();
            Request.Body.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                string result = reader.ReadToEnd();
                if (result.Length > 0 || !result.Trim().Equals(""))
                    PageEntry = JsonConvert.DeserializeObject<Entry>(result);
                else
                    return new JsonResult(JsonConvert.SerializeObject(0));
            }

            string[] keySelect = { "title_unique" };
            MySqlIntegration postInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            postInteg.MySqlSelect("entries", keySelect, "title_unique=" + MySqlIntegration.QuoteStr(PageEntry.UniqueTitle));

            string[] keysSave = { "title_unique", "basic_class_ID", "title_display", "content" };
            int lineAffected;
            if (postInteg.IntegratedResult.Count > 0)
            {
                lineAffected = postInteg.MySqlUpdate("entries", keysSave, "title_unique=" + MySqlIntegration.QuoteStr(PageEntry.UniqueTitle), PageEntry.UniqueTitle, PageEntry.BasicClassID, PageEntry.DisplayTitle, PageEntry.PageContent);
            }
            else
            {
                lineAffected = postInteg.MySqlInsert("entries", keysSave, PageEntry.UniqueTitle, PageEntry.BasicClassID, PageEntry.DisplayTitle, PageEntry.PageContent);
            }
            return new JsonResult(JsonConvert.SerializeObject(lineAffected));
        }
        public bool IsBasicEntry(Entry entry)
        {
            return BasicClasses.Find(basicClassObj => basicClassObj.UniqueTitle == entry.UniqueTitle) != null;
        }
        public class Entry
        {
            public string DisplayTitle { get; set; }
            public string UniqueTitle { get; set; }
            public string PageContent { get; set; }
            public int BasicClassID { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime ModificationTime { get; set; }

        }

        public enum Permission
        {
            ALL = 0,
            USER = 2,
            ADMIN = 4,
            SUPER = 8
        }

        public enum State
        {
            SUCCESS = 1,
            NO_RECORD = 100,
            NO_CHANGE = 101,
            INVAILD_DATA = 200
        }
    }
}