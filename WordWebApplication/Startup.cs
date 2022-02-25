using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.SpellChecker;
using System.IO;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WordWebApplication.Models;

namespace WordWebApplication
{
    public class Startup
    {
        private string _contentRootPath;
        internal static List<DictionaryData> SpellDictCollection;
        internal static string Path;
        internal static string PersonalDictPath;
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _contentRootPath = env.ContentRootPath;
            Configuration = builder.Build();

            Path = Configuration["SPELLCHECK_DICTIONARY_PATH"];
            string jsonFileName = Configuration["SPELLCHECK_JSON_FILENAME"];
            // check the spell check dictionary path environment variable value and assign default data folder
            // if it is null.
            Path = string.IsNullOrEmpty(Path)
                ? System.IO.Path.Combine(env.ContentRootPath, "App_Data")
                : System.IO.Path.Combine(env.ContentRootPath, Path);
            //Set the default spellcheck.json file if the json filename is empty.
            jsonFileName = string.IsNullOrEmpty(jsonFileName)
                ? System.IO.Path.Combine(Path, "spellcheck.json")
                : System.IO.Path.Combine(Path, jsonFileName);

            if (File.Exists(jsonFileName))
            {
                string jsonImport = File.ReadAllText(jsonFileName);
                List<DictionaryData> spellChecks = JsonConvert.DeserializeObject<List<DictionaryData>>(jsonImport);
                SpellDictCollection = new List<DictionaryData>();
                //construct the dictionary file path using customer provided path and dictionary name
                foreach (var spellCheck in spellChecks)
                {
                    SpellDictCollection.Add(new DictionaryData(spellCheck.LanguadeID,
                        System.IO.Path.Combine(Path, spellCheck.DictionaryPath),
                        System.IO.Path.Combine(Path, spellCheck.AffixPath)));
                    PersonalDictPath = System.IO.Path.Combine(Path, spellCheck.PersonalDictPath);
                }
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(o =>
                o.UseSqlServer("Server=NONAME;Database=docsEditorScaffold;User Id=sa;Password=sa"));
            services.AddTransient<AppDbContext, AppDbContext>();
            services.AddControllersWithViews();
            services.AddRouting();
            services.AddMvc();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseCors("AllowAllOrigins");
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Index",
                    pattern: "/Index",
                    defaults: new {controller = "Start", action = "Index",});
            });
        }
    }

    public class ServerPath
    {
        public static string MapPath = "";
    }
}