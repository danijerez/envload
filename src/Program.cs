using libc.translation;
using LoadEnv.Models;
using LoadEnv.Utils;
using Serilog;
using Terminal.Gui;

public class Program
{
    public static void Main()
    {
        Application.Init();
        var top = Application.Top;
        Settings s = FileUtils.InitSettings();

        Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.File(FileUtils.directory + @"\log\envload_.txt")
                 .CreateLogger();

        top.ColorScheme = Colors.ColorSchemes[s.ColorScheme];

        try
        {
            Stream stream = new FileInfo(FileUtils.directory + @"data\i18n.json").OpenRead();
            ILocalizationSource source = new JsonLocalizationSource(stream, PropertyCaseSensitivity.CaseInsensitive);
            ILocalizer rb = new Localizer(source, s.Locale);

            UI.Init(top, s, rb);
            Application.Run();
            Application.Shutdown();
        }
        catch (Exception e)
        {
            Log.Error(e.Message, e);
        }

    }
}



