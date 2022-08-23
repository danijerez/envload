using envload.Utils;
using Serilog;

namespace envload
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.File(FileUtils.directory + @"\log\envload_.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Main());
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }

        }
    }
}