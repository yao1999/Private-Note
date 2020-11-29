using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Private_Note.Areas.Identity.Data;
using Private_Note.Data;

[assembly: HostingStartup(typeof(Private_Note.Areas.Identity.IdentityHostingStartup))]
namespace Private_Note.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PrivateNoteDBContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("PrivateNoteDBContextConnection")));

                services.AddDefaultIdentity<ApplicationUser>(options => 
                {
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedAccount = false;
                    //------------------------------------------- need to delete
                    //options.Password.RequireLowercase = false;
                    //options.Password.RequireUppercase = false;
                    //options.Password.RequireNonAlphanumeric = false;
                    //-------------------------------------------
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                    .AddEntityFrameworkStores<PrivateNoteDBContext>();
            });
        }
    }
}