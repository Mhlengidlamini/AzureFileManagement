using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options; // Required for IOptions<T>
using AzureFileManagement; // Ensure this matches the namespace where AzureFileManager is defined

var builder = WebApplication.CreateBuilder(args);

// Configure and bind AzureStorageOptions
builder.Services.Configure<AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"));

// Register AzureFileManager as a service
builder.Services.AddScoped<IAzureFileManager, AzureFileManager>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure configuration settings are valid
var storageOptions = app.Services.GetRequiredService<IOptions<AzureStorageOptions>>().Value;

if (string.IsNullOrEmpty(storageOptions.ConnectionString))
{
    throw new ArgumentNullException(nameof(storageOptions.ConnectionString), "Connection string cannot be null or empty.");
}

if (string.IsNullOrEmpty(storageOptions.ContractFileShareName))
{
    throw new ArgumentNullException(nameof(storageOptions.ContractFileShareName), "Contract file share name cannot be null or empty.");
}

if (string.IsNullOrEmpty(storageOptions.LogFileShareName))
{
    throw new ArgumentNullException(nameof(storageOptions.LogFileShareName), "Log file share name cannot be null or empty.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Use HSTS for production
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
