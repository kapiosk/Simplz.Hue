using System.Reflection;
using Simplz.Hue.Data;
using Simplz.Hue.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<HueService>();

builder.Services.AddScoped<IConfigRepo>((serviceProvider) =>
{
    var interfaceType = typeof(IConfigRepo);
    var configRepoType = Assembly.GetExecutingAssembly().GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass).FirstOrDefault();
    if (configRepoType is not null)
    {
        var instance = (IConfigRepo?)Activator.CreateInstance(configRepoType);
        if (instance is not null)
            return instance;
    }
    throw new NotImplementedException("IConfigRepo needs to have an implementation in the assembly");
});

await using var app = builder.Build();

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
