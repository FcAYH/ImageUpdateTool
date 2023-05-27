using ImageUpdateTool.Utils;
using ImageUpdateTool.Models;
using Microsoft.Extensions.DependencyInjection;
using UraniumUI;
using ImageUpdateTool.Pages;
using ImageUpdateTool.ViewModels;

namespace ImageUpdateTool;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseUraniumUI()
			.UseUraniumUIMaterial()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFontAwesomeIconFonts();
				fonts.AddMaterialIconFonts();
			})
			.RegisterModels()
			.RegisterViewModels()
			.RegisterViews();

		// Register services
		

		return builder.Build();
	}

	public static MauiAppBuilder RegisterModels(this MauiAppBuilder builder)
	{
        builder.Services.AddSingleton((IServiceProvider) =>
        {
            return new AppSettings();
        });
        builder.Services.AddSingleton((IServiceProvider) =>
        {
            var appSettings = IServiceProvider.GetRequiredService<AppSettings>();
            return new ImageRepositoryModel(appSettings);
        });

		return builder;
    }

	public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient((IServiceProvider) =>
		{
			var appSettings = IServiceProvider.GetRequiredService<AppSettings>();
			return new SettingsViewModel(appSettings);
		});
		return builder;
	}

	public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<LoadingPage>();
		builder.Services.AddTransient((IServiceProvider) =>
		{
			var settingsViewModel = IServiceProvider.GetRequiredService<SettingsViewModel>();
			return new SettingsPage(settingsViewModel);
		});
		return builder;
	}
}
