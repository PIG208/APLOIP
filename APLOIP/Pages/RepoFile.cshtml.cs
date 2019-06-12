using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace APLOIP.Pages
{
    public class RepoFileModel : PageModel
    {
        public RepoFileModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        IHostingEnvironment Environment { get; set; }
        IConfiguration Configuration { get; }
        public List<RepoFile> FileList { get; private set; } = new List<RepoFile>();
        public void OnGet()
        {
            if ((string)RouteData.Values["title"] != null)
            {
                string title = (string)RouteData.Values["title"];
                string[] keys = { "*" };
                string specifier = string.Format("entry_title={0} AND operator_type={1}", MySqlIntegration.QuoteStr(title), MySqlIntegration.QuoteStr(Operator.File));
                MySqlIntegration sqlInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
                var result = sqlInteg.MySqlSelect("repo_key_values", keys, specifier);
                Debug.WriteLine(result.Count);
                RepoFile repoFile = new RepoFile();
                result.ForEach((element) =>
                {
                    if (element["key_title"].Equals("file_title"))
                    {
                        repoFile.Title = (string)element["value"];
                    }
                    if (element["key_title"].Equals("file_web_path"))
                    {
                        repoFile.Path = (string)element["value"];
                    }
                    if (element["key_title"].Equals("file_description"))
                    {
                        repoFile.Description = (string)element["value"];
                    }
                });
                FileList.Add(repoFile);
            }
        }

        [HttpPost]
        public ActionResult OnPostUploadFile(string title, string owner, string description, IFormFile fileUpload)
        {
            IFormFile file = fileUpload;//Request.Form.Files[0];
            if ((file == null || file.Length == 0) || (title == null || title.Trim().Length == 0) || file.Length > 4194304)
                return new JsonResult(State.INVAILD_DATA);

            MemoryStream memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extention = Path.GetExtension(file.FileName);
            string localPath = Path.Combine(Environment.WebRootPath, "files", title, fileName + extention);
            string serverPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), "files", title, fileName + extention);
            Directory.CreateDirectory(Path.Combine(Environment.WebRootPath, "files", title));
            string[] keys = { "entry_title", "operator_type", "key_title", "value", "class" };

            MySqlIntegration sqlInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            sqlInteg.MySqlInsert("repo_key_values", keys, title, Operator.File, "file_title", fileName + extention, "string");
            sqlInteg.MySqlInsert("repo_key_values", keys, title, Operator.File, "file_description", description, "string");
            sqlInteg.MySqlInsert("repo_key_values", keys, title, Operator.File, "file_web_path", serverPath.Replace("\\", "\\\\"), "string");

            keys = new string[] { "title_unique", "title_display", "operator_type", "owner" };
            sqlInteg.MySqlInsert("repo_entries", keys, title, title, Operator.File, owner);

            //Future Feature
            //sqlInteg.MySqlInsert("repo_key_values", keys, title, Operator.File, "description", localPath, "string");
            System.Threading.Thread.Sleep(500);
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(localPath, FileMode.Create);
                file.CopyTo(fileStream);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
                return new JsonResult(State.IO_ERROR);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }

            return new JsonResult(JsonConvert.SerializeObject(State.SUCCESS));
        }
    }
    public class RepoFile
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
    }
}