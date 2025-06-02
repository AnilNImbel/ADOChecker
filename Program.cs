using ADOAnalyser;
using ADOAnalyser.Common;
using ADOAnalyser.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("AzureDevOpsClient", client =>
{
    var PATToken = "EJHRsZQ9XNFTrN8UTRDr28C1Y7V14k5rmCYE1vaKxL3wAiishmBFJQQJ99BEACAAAAANjyhyAAASAZDO4OL8";
    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PATToken}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json-patch+json"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        Proxy = new WebProxy("http://b.webproxy.civica.com:8080")
        {
            Credentials = CredentialCache.DefaultNetworkCredentials
        },
        UseProxy = true
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUtility, Utility>();
builder.Services.AddScoped<IWorkItem, WorkItem>();
builder.Services.AddScoped<AutoSpotCheck>();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Application/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();


app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
