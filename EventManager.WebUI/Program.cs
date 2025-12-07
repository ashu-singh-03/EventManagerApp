using EventManager.Application;
using EventManager.Infrastructure;
using EventManager.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// 1. Register IHttpContextAccessor
// -------------------------------
builder.Services.AddHttpContextAccessor();

// -------------------------------
// 2. Register application & infrastructure services
// -------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// -------------------------------
// 3. Register IDbConnection for repositories
// -------------------------------
builder.Services.AddTransient<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------------
// 4. Add MVC
// -------------------------------
builder.Services.AddControllersWithViews();

// -------------------------------
// 5. Configure authentication
// -------------------------------
builder.Services.AddAuthentication("EventCookie")
    .AddCookie("EventCookie", options =>
    {
        options.LoginPath = "/"; // redirect if not authenticated
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// -------------------------------
// 6. Configure request pipeline
// -------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // required for claims
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Event}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
