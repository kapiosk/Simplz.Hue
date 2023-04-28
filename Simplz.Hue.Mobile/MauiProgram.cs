using Microsoft.Extensions.Logging;
using Simplz.Hue.Core.Data;
using Simplz.Hue.Core.Services;
using Simplz.Hue.Mobile.Data;

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
		builder.Services.AddScoped<HueService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
