using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Data;
using HelwanUniversity.Services;
using Data.Repository.IRepository;
using Data.Repository;
using CloudinaryDotNet;
using Models;
using Stripe;
using Newtonsoft.Json;

namespace HelwanUniversity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<IdentityUser , IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI().AddDefaultTokenProviders();

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            object value = builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddHttpClient();
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
            builder.Services.AddScoped<IHighBoardRepository, HighBoardRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
            builder.Services.AddScoped<IAcademicRecordsRepository, AcademicRecordsRepository>();
            builder.Services.AddScoped<IUniFileRepository, UniFileRepository>();
            builder.Services.AddScoped<IStudentSubjectsRepository, StudentSubjectsRepository>();
            builder.Services.AddScoped<IDepartmentSubjectsRepository, DepartmentSubjectsRepository>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IActivityLogger, ActivityLogger>();

            builder.Services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var cloudinaryAccount = new CloudinaryDotNet.Account(
                builder.Configuration["Cloudinary:CloudName"],
                builder.Configuration["Cloudinary:ApiKey"],
                builder.Configuration["Cloudinary:ApiSecret"]
            );
            var cloudinary = new Cloudinary(cloudinaryAccount);
            builder.Services.AddSingleton(cloudinary);
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient<IActivityLogger, ActivityLogger>(client =>
            {
                client.BaseAddress = new Uri("https://api.cohere.ai/");
            });
            builder.Services.AddMemoryCache();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); 
                app.UseSwaggerUI(); 
                app.UseDeveloperExceptionPage(); 
            }
            else 
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=User}/{controller=University}/{action=Index}/{id?}");

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}
