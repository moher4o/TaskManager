using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Services;
using TaskManager.Services.Implementations;
using TaskManager.Data.Models;
using TaskMenager.Client.Controllers;
using TaskMenager.Client.Infrastructure.Extensions;

namespace TaskMenager.Client
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
            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddHttpContextAccessor();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            services.AddAuthorization(options =>
            {
                //options.AddPolicy(DataConstants.Employee, policy =>
                //                policy.RequireAssertion(context => context.User.HasClaim(c => c.Value == DataConstants.Employee || c.Value == "Admin")));
                options.AddPolicy(DataConstants.Employee, policy =>
                                policy.RequireClaim("permission", new string[] { "Admin", "Employee" }));
                options.AddPolicy("Admin", policy =>
                                policy.RequireClaim("permission", "Admin"));
                options.AddPolicy(DataConstants.SuperAdmin, policy =>
                                policy.RequireClaim("permissionType", DataConstants.SuperAdmin));
                options.AddPolicy("Guest", policy =>
                                policy.RequireClaim("permission", "Guest"));

            });

            

            services.AddDbContext<TasksDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllersWithViews();

            services.AddAutoMapper();


            services.AddDomainServices();

            services.AddMvc(options =>
            {
                options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
            });

            services.AddDistributedMemoryCache();

            services.AddSession();

            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

            services.AddSingleton<IFileStoreConfiguration>(Configuration.GetSection("FileStoreConfiguration").Get<FileStoreConfiguration>());

            services.AddSingleton<IDateManagementConfiguration>(Configuration.GetSection("DateManagement").Get<DateManagementConfiguration>());

            services.AddSingleton<IApprovalConfiguration>(Configuration.GetSection("ApprovalConfiguration").Get<ApprovalConfiguration>());

            services.AddSingleton<I2FAConfiguration>(Configuration.GetSection("TwoFactorConfiguration").Get<TwoFAConfiguration>());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.MigrateDatabase();
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

            app.UseAuthentication();
            app.UseMiddleware<TwoFACookiesMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
