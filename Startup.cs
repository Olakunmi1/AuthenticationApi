using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authenticate.Data;
using Authenticate.Service;
using AuthenticationApi.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace AuthenticationApi
{
    public class Startup
    {
      //  private readonly IConfiguration _onfiguration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
          
        }

        public IConfiguration Configuration { get; }

      
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
            });
            services.AddScoped<IUser, UserService>();
            services.AddDbContext<ApplicationDBContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>();
           

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<appSetting>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<appSetting>();
            // var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var key = Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                //x.Events = new JwtBearerEvents
                //{
                //    OnTokenValidated = context =>
                //    {
                //        var userService = context.HttpContext.RequestServices.GetRequiredService<IUser>();
                //        var userId = int.Parse(context.Principal.Identity.Name);
                //        var user = userService.GetById(userId);
                //        if (user == null)
                //        {
                //            // return unauthorized if user no longer exists
                //            context.Fail("Unauthorized");
                //        }
                //        return Task.CompletedTask;
                //    }
                //};
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
                {

                    Title = "Authentication Service API",
                    Version = "v2",
                    Description = "A simple Api that enables user to generate Token befire having acces to some resource",
                });

                ////Inorder for the Xml documentation to show ... Also Xml docmentation is enable in project prop
                //options.IncludeXmlComments(System.IO.Path.Combine(System.AppContext.BaseDirectory, "AuthenticationApi.xml"));


                //For Authorization Key Button to come up, and to activate token from SwaggerUI
                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer"
                });

                //Helps to tell swagger which of our actions require Authorization. 
                options.OperationFilter<AuthenticationRequirementsOperationFilter>();

            });

            //In order to Access swagger JSON Object 
            //https://localhost:44318/swagger/v2/swagger.json

            //Inorder to Access Swagger GUI for interaction 
            //https://localhost:44318/swagger/index.html

           
            services.AddMvcCore().AddApiExplorer();  //Needed for swagger to work with .netcoremvc
            services.AddRazorPages();
        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                app.UseEndpoints(endpoints => endpoints.MapControllers());
                endpoints.MapRazorPages();
            });


            //Middleware for swagger 
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v2/swagger.json", "Authorization Api"));
        }
    }
}
