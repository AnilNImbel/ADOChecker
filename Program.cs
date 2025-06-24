using ADOAnalyser;
using ADOAnalyser.DBContext;
using ADOAnalyser.IRepository;
using ADOAnalyser.Repository;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("AzureDevOpsClient", client =>
{
    var PATToken = builder.Configuration.GetValue<string>("PATToken");
    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PATToken}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json-patch+json"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var useProxy = builder.Configuration.GetValue<bool>("UseProxy");

    if (useProxy)
    {
        var proxyUrl = builder.Configuration.GetValue<string>("Proxy");

        return new HttpClientHandler
        {
            Proxy = new WebProxy(proxyUrl)
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            },
            UseProxy = true
        };
    }

    // If proxy is not to be used, return default handler
    return new HttpClientHandler();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUtility, Utility>();
builder.Services.AddScoped<IWorkItem, WorkItem>();
builder.Services.AddScoped<AutoSpotCheck>();
builder.Services.AddScoped<Email>();
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("Email"));

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
