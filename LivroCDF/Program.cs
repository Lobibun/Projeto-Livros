using LivroCDF.Data;
using LivroCDF.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LivrariaContext");
builder.Services.AddDbContext<LivrariaContext>(options =>
    options.UseMySql(connectionString,
    ServerVersion.AutoDetect(connectionString)));


builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
 
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<LivrariaContext>();


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<LivroService>();
builder.Services.AddScoped<ClienteService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Cria os nomes dos cargos se não existirem
    string[] roleNames = { "SuperAdmin", "Admin", "Comum" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    string emailDoChefe = "admin@livrocdf.com"; 
    string senhaDoChefe = "Admin@123";         

    var powerUser = await userManager.FindByEmailAsync(emailDoChefe);


    if (powerUser == null)
    {
        powerUser = new IdentityUser { UserName = emailDoChefe, Email = emailDoChefe, EmailConfirmed = true };
        await userManager.CreateAsync(powerUser, senhaDoChefe);
    }

    // Garante que você tenha o cargo de SuperAdmin
    if (!await userManager.IsInRoleAsync(powerUser, "SuperAdmin"))
    {
        await userManager.AddToRoleAsync(powerUser, "SuperAdmin");
    }
}


if (!app.Environment.IsDevelopment())
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 
app.Run();