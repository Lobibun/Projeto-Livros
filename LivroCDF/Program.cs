using LivroCDF.Data;
using LivroCDF.Services;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("LivrariaContext");
builder.Services.AddDbContext<LivrariaContext>(options =>
    options.UseMySql(connectionString,
    ServerVersion.AutoDetect(connectionString)));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<LivroService>();
builder.Services.AddScoped<ClienteService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
