using Library.Data;
using Library.Core;
using Library.Service;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.SignalR;
using Library.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;
using Library.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// DbContext'i DI container'a ekle
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDb")));

// Repository ve Service DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookRentalRepository, BookRentalRepository>();
builder.Services.AddScoped<IBookRentalService, BookRentalService>();
builder.Services.AddScoped<ISeatReservationRepository, SeatReservationRepository>();
builder.Services.AddScoped<ISeatReservationService, SeatReservationService>();
builder.Services.AddScoped<BookCsvImportService>();
builder.Services.AddScoped<OverdueNotificationJob>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<SeatReservationNotificationService>();
builder.Services.AddScoped<SeatReservationJobService>();

// Hangfire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("LibraryDb")));
builder.Services.AddHangfireServer();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Kitap CSV import işlemi (sadece ilk başlatmada)
using (var scope = app.Services.CreateScope())
{
    var importService = scope.ServiceProvider.GetRequiredService<BookCsvImportService>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var csvPath = Path.Combine(env.ContentRootPath, "Books_data - Sheet1.csv");
    importService.ImportBooksFromCsv(csvPath);
}

// 30 koltuk seed işlemi (sadece ilk başlatmada)
using (var scope = app.Services.CreateScope())
{
    var seatRepo = scope.ServiceProvider.GetRequiredService<IRepository<Seat>>();
    var seats = await seatRepo.GetAllAsync();
    Console.WriteLine($"Koltuk sayısı kontrol ediliyor: {seats.Count()}"); // Log ekle
    if (!seats.Any())
    {
        Console.WriteLine("Koltuklar ekleniyor..."); // Log ekle
        for (int i = 1; i <= 30; i++)
        {
            await seatRepo.AddAsync(new Seat { SeatNumber = i });
        }
        await seatRepo.SaveAsync(); // Değişiklikleri kaydet
        Console.WriteLine("30 koltuk başarıyla eklendi."); // Log ekle
    }
    else
    {
        Console.WriteLine("Koltuklar zaten mevcut, ekleme yapılmadı."); // Log ekle
    }
}

// Hangfire dashboard
app.UseHangfireDashboard();

// Hangfire: Geciken kitap iadeleri için günlük job
using (var scope = app.Services.CreateScope())
{
    var job = scope.ServiceProvider.GetRequiredService<OverdueNotificationJob>();
    RecurringJob.AddOrUpdate("overdue-notification-job", () => job.SendOverdueNotificationsAsync(), Cron.Daily);
}

// Admin seed
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var users = await userService.GetAllUsersAsync();
    if (!users.Any(u => u.Role == "Admin"))
    {
        await userService.AddUserAsync(new Library.Core.User
        {
            FirstName = "Admin",
            LastName = "User",
            TC = "11111111111",
            Username = "admin",
            Email = "admin@kutuphane.com",
            Phone = "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            FaceId = string.Empty // Boş string olarak ayarla
        });
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<SeatHub>("/seathub");


app.Run();
