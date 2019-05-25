﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            string[] keys = { "title", "description", "fa_icon", "style" };
            selectInteg.MySqlSelect("basic_class", keys).ForEach(obj => {
                BasicClass basicClass = new BasicClass
                {
                    Title = (string) obj["title"],
                    Description = (string) obj["description"],
                    FaIcon = (string) obj["fa_icon"],
                    Style = (int) obj["style"]
                };
                BasicClasses.Add(basicClass);
            });
        }
    }

    public class BasicClass
    {
        public string Title { get; set; }
        
        public string Description { get; set; }

        public string FaIcon { get; set; }
        
        public int Style { get; set; }
    }
}