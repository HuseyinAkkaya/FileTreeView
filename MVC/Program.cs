using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MVC.Configs;
using MVC.Contexts;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});
IMapper mapper = mapperConfig.CreateMapper();

#region IoC Container
//Baðýmlýlýk Yönetimi
//AutoFac, Ninject
builder.Services.AddDbContext<Db>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton(mapper); 
#endregion

// Add services to the container. 
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
builder.Services.AddControllersWithViews();

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
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

app.Run();
