using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using OceanTechLevel1.Services; 

namespace OceanTechLevel1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add DbContext configuration
            builder.Services.AddDbContext<Oceantech2Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
            // Add MemoryCache services
            builder.Services.AddMemoryCache();
            // Service
            builder.Services.AddScoped<EmployeeService>();  
            builder.Services.AddScoped<ExportImportService>();    
            builder.Services.AddScoped<QualificationService>();
            builder.Services.AddScoped<ViewBagService>();
            builder.Services.AddScoped<ProvinceService>();
            builder.Services.AddScoped<DistrictService>();
            builder.Services.AddScoped<CommuneService>();
            builder.Services.AddScoped<ValidationService>();
            builder.Services.AddScoped<PagingService>();
           
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Employee}/{action=ListOfEmployee}/{id?}");

            app.Run();
        }
    }
}
