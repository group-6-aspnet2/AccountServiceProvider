using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Data.Contexts;
using Business.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ServiceBusPublisher>();

builder.Services.AddGrpc();
builder.Services.AddDbContext<DataContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(x =>
{
   // x.SignIn.RequireConfirmedEmail = true;
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Tagit hjälp av chatgpt
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = "admin@example.com";
    var adminPassword = "Admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, adminPassword);
        Console.WriteLine("Admin created.");
    }
    else
    {
        Console.WriteLine("Admin already exists");
    }
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<AccountService>();

app.Run();
