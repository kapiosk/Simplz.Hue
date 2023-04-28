var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

await using var app = builder.Build();

app.UseStaticFiles();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
