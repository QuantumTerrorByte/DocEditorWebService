using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Edm;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.Cors.Infrastructure;
using EJ2APIServices.Models;
using EJ2APIServices.Controllers;
using Syncfusion.EJ2.SpellChecker;
using System.IO;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WordWebApplication.Models;

namespace WordWebApplication
{
    public class Startup
    {
        private string _contentRootPath = "";
        internal static List<DictionaryData> spellDictCollection;
        internal static string path;
        internal static string personalDictPath;
        private IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            _contentRootPath = env.ContentRootPath;

            path = Configuration["SPELLCHECK_DICTIONARY_PATH"];
            string jsonFileName = Configuration["SPELLCHECK_JSON_FILENAME"];
            //check the spell check dictionary path environment variable value and assign default data folder
            //if it is null.
            path = string.IsNullOrEmpty(path)
                ? Path.Combine(env.ContentRootPath, "App_Data")
                : Path.Combine(env.ContentRootPath, path);
            //Set the default spellcheck.json file if the json filename is empty.
            jsonFileName = string.IsNullOrEmpty(jsonFileName)
                ? Path.Combine(path, "spellcheck.json")
                : Path.Combine(path, jsonFileName);
            
            if (File.Exists(jsonFileName))
            {
                string jsonImport = File.ReadAllText(jsonFileName);
                List<DictionaryData> spellChecks = JsonConvert.DeserializeObject<List<DictionaryData>>(jsonImport);
                spellDictCollection = new List<DictionaryData>();
                //construct the dictionary file path using customer provided path and dictionary name
                foreach (var spellCheck in spellChecks)
                {
                    spellDictCollection.Add(new DictionaryData(spellCheck.LanguadeID,
                        Path.Combine(path, spellCheck.DictionaryPath), Path.Combine(path, spellCheck.AffixPath)));
                    personalDictPath = Path.Combine(path, spellCheck.PersonalDictPath);
                }
            }
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(o =>
                o.UseSqlServer("Server=NONAME;Database=docsEditor;User Id=sa;Password=sa"));
            services.AddCors();
            services.AddControllersWithViews();
            services.AddOData();
            services.AddMvc().AddJsonOptions(x => {
                x.SerializerSettings.ContractResolver = null;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            string connection = Configuration.GetConnectionString("Test");
            if (connection.Contains("%CONTENTROOTPATH%"))
            {
                connection = connection.Replace("%CONTENTROOTPATH%", _contentRootPath);

            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(builder => builder
                .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().DisallowCredentials()
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    
    public class ServerPath
    {
        public static string MapPath = "";
    }
}