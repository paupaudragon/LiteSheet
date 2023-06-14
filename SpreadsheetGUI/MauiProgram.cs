using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml.Documents;

namespace SpreadsheetGUI;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("OPTIAntique-Bold.otf", "AntiqueBold");
                fonts.AddFont("texgyretermes-bold.otf", "TimesNewRomanBold");
                fonts.AddFont("texgyretermes-bolditalic.otf", "TimesNewRomanBoldItalic");
                
                fonts.AddFont("BlackMarker.otf", "BlackMarker");
            });
		
		return builder.Build();
	}

	static void OnDestroying()
	{

	}
}

