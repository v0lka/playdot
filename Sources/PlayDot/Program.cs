using System;
using System.IO;
using System.Reflection;
using PlayDot.LibraryProviders;
using PlayDot.LibraryProviders.EA;
using PlayDot.LibraryProviders.Epic;
using PlayDot.LibraryProviders.Steam;
using PlayDot.Utils;


namespace PlayDot
{
    internal static class Program
    {
        public static string AppDataDirectoryPath;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AppDataDirectoryPath = PathUtils.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PlayDot"
            );

            if (!Directory.Exists(AppDataDirectoryPath))
            {
                Directory.CreateDirectory(AppDataDirectoryPath);
            }

            Logger.Init(PathUtils.Combine(AppDataDirectoryPath, "Logs"));

            try
            {
                var applicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
                Logger.Info($"Application version: {applicationVersion}");

                var systemInformation = SystemUtils.GetSystemInformation();
                Logger.Info("System information collected", systemInformation);

                var environmentInformation = SystemUtils.GetEnvironmentInformation();
                Logger.Info("Environment information collected", environmentInformation);

                var launcherProviders = new ILibraryProvider[]
                {
                    new SteamProvider(),
                    new EpicProvider(),
                    new EaProvider()
                };

                var registry = new Registry(launcherProviders);
                var appContext = new UI.AppContext(registry);

                appContext.Run();
            }
            catch (Exception e)
            {
                Logger.Fatal("Unhandled exception thrown", e);
                throw;
            }

            Logger.Info("Application terminated successfully");
        }
    }
}
