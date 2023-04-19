using Simplz.Hue.Data;
using Simplz.Hue.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<HueService>();
builder.Services.AddScoped<IConfigRepo, ConfigRepo>();

await using var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
