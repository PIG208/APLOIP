using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace APLOIP.Pages
{
    public class RepoQuizModel : PageModel
    {
        public RepoQuizModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IConfiguration Configuration { get; }
        public Quiz PageQuiz { get; set; } = new Quiz();
        IEntity GetEntityByType(string type)
        {
            if (type != null)
            {
                switch (type)
                {
                    case "mcq":
                        return new McqEntity();
                    case "drag":
                        return new DragEntity();
                    default:
                        return new McqEntity();
                }
            }
            return null;
        }
        public void OnGet()
        {
            PageQuiz.UniqueTitle = (string)RouteData.Values["title"];
            MySqlIntegration sqlInteg = new MySqlIntegration(Configuration.GetConnectionString("MySqlConnection"));
            string[] keys = { "*" };
            string specifier = (PageQuiz.UniqueTitle != null)?string.Format("entry_title={0} AND operator_type={1}", MySqlIntegration.QuoteStr(PageQuiz.UniqueTitle), MySqlIntegration.QuoteStr(Operator.Quiz)):"";
            if(sqlInteg.MySqlSelect("repo_key_values", keys, specifier).Count > 0)
            {
                var result = sqlInteg.IntegratedResult;
                result.ForEach((element) => {
                    if (element.TryGetValue("key_title", out object val))
                    {
                        switch((string)val)
                        {
                            case "title_display":
                                PageQuiz.DisplayTitle = (string)element["value"];
                                break;
                            case "entity_ID":
                                if(sqlInteg.MySqlSelect("repo_quiz_entities", keys, "ID=" + int.Parse((string)element["value"])).Count > 0)
                                {
                                    var obj = sqlInteg.IntegratedResult;
                                    IEntity entity = GetEntityByType((string)obj[0]["type"]);
                                    entity.Info = (string)obj[0]["info"];
                                    entity.Answer = (string)obj[0]["answer"];
                                    entity.Type = (string)obj[0]["type"];
                                    entity.Index = (int)obj[0]["index"];
                                    PageQuiz.Entities.Add(entity);
                                }
                                break;
                        }
                    }
                });
            }
        }
        public void OnPostSave(string UniqueTitle)
        {

        }
    }
}