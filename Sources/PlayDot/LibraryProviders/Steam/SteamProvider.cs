using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Gameloop.Vdf;
using PlayDot.Utils;

namespace PlayDot.LibraryProviders.Steam
{
    internal sealed class SteamProvider : ILibraryProvider
    {
        private readonly string steamInstallationDir;
        private readonly string libraryFoldersPath;

        public SteamProvider()
        {
            steamInstallationDir = Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                "SteamPath",
                default
            ) as string;

            if (steamInstallationDir != null && Directory.Exists(steamInstallationDir))
            {
                libraryFoldersPath = PathUtils.Combine(steamInstallationDir, "steamapps", "libraryfolders.vdf");
            }
        }

        public Color BrandColor { get; } = Color.FromArgb(60, 145, 174);

        public IEnumerable<GameDescriptor> CollectInstalledGames()
        {
            if (!File.Exists(libraryFoldersPath)) { yield break; }

            var steamAppsPath = PathUtils.Combine(steamInstallationDir, "steamapps");
            var libraryFolders = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { steamAppsPath };
            dynamic librabyFoldersKv = VdfConvert.Deserialize(File.ReadAllText(libraryFoldersPath));

            if ("libraryfolders".Equals(librabyFoldersKv.Key, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var child in librabyFoldersKv.Value)
                {
                    string name = child.Key;

                    if (uint.TryParse(name, out _))
                    {
                        libraryFolders.Add(PathUtils.Combine(child.Value["path"].Value, "steamapps"));
                    }
                }
            }

            foreach (var libraryFolder in libraryFolders)
            {
                if (!Directory.Exists(libraryFolder)) { continue; }

                var appmanifestFilePaths = Directory.GetFiles(libraryFolder, "appmanifest_*.acf");

                foreach (var appmanifestFilePath in appmanifestFilePaths)
                {
                    dynamic appmanifestKv = VdfConvert.Deserialize(File.ReadAllText(appmanifestFilePath));

                    if (appmanifestKv.Key != "AppState") { continue; }
                    
                    string gameName = null;
                    string gameId = null;

                    foreach (var child in appmanifestKv.Value)
                    {
                        switch (child.Key)
                        {
                            case "appid":
                                gameId = child.Value.ToString();
                                break;
                            case "name":
                                gameName = StringExtensions.NormalizeGameName(child.Value.ToString());
                                break;
                        }
                    }

                    if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(gameName)) { continue; }
                    
                    // "228980" corresponds to "Steamworks Common Redistributables" and should be skipped
                    if (gameId != "228980") { yield return new GameDescriptor(gameName, gameId); }
                }
            }
        }

        public void RunGame(GameDescriptor game)
        {
            ProcessUtils.StartSilent($"steam://run/{game.AppId}");
        }
    }
}
