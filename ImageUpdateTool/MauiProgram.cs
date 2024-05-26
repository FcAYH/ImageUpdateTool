using ImageUpdateTool.Utils;
using ImageUpdateTool.Models;
using Microsoft.Extensions.DependencyInjection;
using UraniumUI;
using ImageUpdateTool.Pages;
using ImageUpdateTool.ViewModels;
using ImageUpdateTool.Views;

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
        builder.Services.AddTransient((IServiceProvider) =>
        {
            var imageRepositoryModel = IServiceProvider.GetRequiredService<ImageRepositoryModel>();
            return new MainViewModel(imageRepositoryModel);
        });
        builder.Services.AddTransient((IServiceProvider) =>
        {
            var imageRepositoryModel = IServiceProvider.GetRequiredService<ImageRepositoryModel>();
            return new FunctionButtonsViewModel(imageRepositoryModel);
        });
        builder.Services.AddTransient((IServiceProvider) =>
        {
            var imageRepositoryModel = IServiceProvider.GetRequiredService<ImageRepositoryModel>();
            return new RepositoryDirectoryViewModel(imageRepositoryModel);
        });
        builder.Services.AddTransient((IServiceProvider) =>
        {
            var imageRepositoryModel = IServiceProvider.GetRequiredService<ImageRepositoryModel>();
            return new ImageDisplayViewModel(imageRepositoryModel);
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
        builder.Services.AddTransient((IServiceProvider) =>
        {
            var mainViewModel = IServiceProvider.GetRequiredService<MainViewModel>();
            return new MainPage(mainViewModel); // TODO:之后重命名成MainPage
        });

        // 这三个是自定义ContentView，但是目前还不支持对它们的DI
        //builder.Services.AddTransient((IServiceProvider) =>
        //{
        //	var functionButtonsViewModel = IServiceProvider.GetRequiredService<FunctionButtonsViewModel>();
        //	return new FunctionButtons(functionButtonsViewModel);
        //});
        //builder.Services.AddTransient((IServiceProvider) =>
        //{
        //	var repositoryDirectoryViewModel = IServiceProvider.GetRequiredService<RepositoryDirectoryViewModel>();
        //	return new RepositoryDirectoryTreeView(repositoryDirectoryViewModel);
        //});
        //builder.Services.AddTransient((IServiceProvider) =>
        //{
        //	var imageDisplayViewModel = IServiceProvider.GetRequiredService<ImageDisplayViewModel>();
        //	return new ImageDisplayGrid(imageDisplayViewModel);
        //});

        return builder;
    }
}
