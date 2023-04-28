using Microsoft.Extensions.Logging;
using Simplz.Hue.Data;
using Simplz.Hue.Mobile.Data;
using Simplz.Hue.Services;

namespace Simplz.Hue.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddScoped<IConfigRepo, ConfigRepo>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services.AddScoped<HueService>();

		return builder.Build();
	}
}
