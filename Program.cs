using Azure.Storage.Blobs;
using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Gumani_Moila_ST10229429_CLDV7111w
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<EventEaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("EventEaseContext")));
            // register azure blob storage service
            builder.Services.AddSingleton(x=> new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
     {
         options.LoginPath = "/Home/Login";
         options.AccessDeniedPath = "/Home/Login";
     });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
