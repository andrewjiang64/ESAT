using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL;
using AgBMTool.DBL.Interface;
using AgBMTool.Front.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using AgBMPTool.DBModel.Model;

namespace AgBMTool.Front
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
    .AddJsonOptions(
        options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).ConfigureApiBehaviorOptions(options => {
                options.SuppressUseValidationProblemDetailsForInvalidModelStateResponses = false;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var connectionStringsSection = Configuration.GetSection("ConnectionStrings");
            services.Configure<ConnectionStrings>(connectionStringsSection);

            // configure jwt authentication
            var connectionStrings = connectionStringsSection.Get<ConnectionStrings>();

            services.AddDbContext<AgBMPToolContext>(options => options.UseNpgsql(connectionStrings.IdentityConnection, o => o.UseNetTopologySuite()), ServiceLifetime.Scoped);

            services.AddDefaultIdentity<User>()
                .AddEntityFrameworkStores<AgBMPToolContext>();

            services.AddCors();
            services.AddScoped<IRepository<BaseItem>, AgBMPToolRepository<BaseItem>>();
            services.AddScoped<IUnitOfWork, AgBMPToolUnitOfWork>();
            services.AddScoped<IUserDataServices, UserDataServices> ();
            services.AddScoped<IProjectSummaryService, ProjectSummaryServiceUnitOptimizationSolution>();
            services.AddScoped<IProjectDataService, ProjectDataService>();
            services.AddScoped<IUtilitiesDataService, UtilitiesDataService>();
            services.AddScoped<IUtilitiesSummaryService, UtilitiesSummaryService>();
            services.AddScoped<IInteligentRecommendationService, IntelligentRecommendationServiceLpSolve>();

            //Jwt Authentication
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = false;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{id?}");

                routes.MapRoute(
                  name: "userLSDData",
                  template: "api/{controller}/{action}/{id}/{municipalityId}/{waterShedId}/{subWaterShedId}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
