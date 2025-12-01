using EventManager.Application;
using EventManager.Infrastructure;
using EventManager.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Register your application & infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Register IDbConnection so repositories can use it
builder.Services.AddTransient<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Event}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
