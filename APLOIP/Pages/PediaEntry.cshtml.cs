using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace APLOIP.Pages
{
    public partial class PediaEntryModel : PageModel
    {
        public PediaEntryModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }
        IHostingEnvironment Environment { get; set; }
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
        public IFormFile ImageUpload { get; set; }
        public JsonResult OnPostSaveImage(string UniqueTitle)
        {
            //IFormFile formFile = (IFormFile)Request.Form["upload"][0];
            System.Diagnostics.Debug.WriteLine("==============================");
            System.Diagnostics.Debug.WriteLine("name: " + ImageUpload.FileName);
            System.Diagnostics.Debug.WriteLine("size: " + ImageUpload.Length);
            Directory.CreateDirectory(Path.Combine(Environment.WebRootPath, "uploads", UniqueTitle));
            var fileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + Path.GetFileName(ImageUpload.FileName);
            var file = Path.Combine(Environment.WebRootPath, "uploads", UniqueTitle, fileName);
            Dictionary<string, object> res = new Dictionary<string, object>();
            res.Add("Path", Path.Combine(UniqueTitle, fileName));
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                ImageUpload.CopyTo(fileStream);
                return new JsonResult(JsonConvert.SerializeObject(Path.Combine("/", "uploads", UniqueTitle, fileName)));
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
                if (result.Length > 0 || !result.Trim().Equals(""))
                    PageEntry = JsonConvert.DeserializeObject<Entry>(result);
                else
                    return new JsonResult(JsonConvert.SerializeObject(0));
            }
            //变量验证
            if (BasicClasses.Find(basicClassObj => basicClassObj.ID == PageEntry.BasicClassID) == null)
            {
                PageEntry.BasicClassID = 0;
            }
            else
            {
                if (IsBasicEntry(PageEntry))
                {
                    PageEntry.BasicClassID = BasicClasses.Find(basicClassObj => basicClassObj.UniqueTitle == PageEntry.UniqueTitle).ID;
                }
            }
            PageEntry.PageContent = PageEntry.PageContent.Replace("\\", "\\\\");
            PageEntry.PageContent = PageEntry.PageContent.Replace("\'","\\\'");
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
    }
}