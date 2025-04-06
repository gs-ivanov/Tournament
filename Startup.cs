namespace Tournament
{
    using DinkToPdf;
    using DinkToPdf.Contracts;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Infrastructure;
    using Tournament.Services.Email;
    using Tournament.Services.MatchResultNotifire;
    using Tournament.Services.MatchScheduler;
    using Tournament.Services.PDF;
    using Tournament.Services.Ranking;
    using Tournament.Services.Sms;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }



        public void ConfigureServices(IServiceCollection services)
        {

            services
                .AddDbContext<TurnirDbContext>(options => options
                .UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services
                .AddDefaultIdentity<User>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TurnirDbContext>();

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
            });

            services
                .AddTransient<IRankingService, RankingService>();
            services
                .AddTransient<IMatchSchedulerService, MatchSchedulerService>();
            services
                .AddTransient<IMatchResultNotifierService, MatchResultNotifierService>();
            services
                .AddTransient<IEmailSender, EmailSender>();
            services
                .AddTransient<ISmsSender, TwilioSmsSender>();
            services
                .AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services
                .AddScoped<PdfService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.PrepareDatabase();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                    endpoints.MapRazorPages();
                });
        }
    }
}