using Microsoft.EntityFrameworkCore;
using theunsafebank.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BankContext>(options =>
    options.UseSqlite("Data Source=bank.db"));

// Add session support (insecure - no encryption, just basic storage)
builder.Services.AddDistributedMemoryCache(); // In-memory cache for sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
// }

// INSECURE!
// app.UseHttpsRedirection();

app.UsePathBase("/bank"); //Eftersom siten servas under suvnet.se/bank

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
