using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using PlayDot.Utils;

namespace PlayDot.LibraryProviders.EA;

internal class EaProvider : ILibraryProvider
{
    public Color BrandColor { get; } = Color.FromArgb(245, 108, 45);

    public IEnumerable<GameDescriptor> CollectInstalledGames()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        var installationDirectories = new List<string>
        {
            Path.Combine(programFiles, "EA Games")
        };
        
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dataDirectory = Path.Combine(localAppData, "Electronic Arts", "EA Desktop");

        foreach (var file in Directory.GetFiles(dataDirectory, "user_*.ini"))
        {
            var userIniText = File.ReadAllText(file);
            var matchResult = Regex.Match(userIniText, @"user\.downloadinplacedir\=(.*)");
            
            if (matchResult.Success)
            {
                Debug.Assert(matchResult.Groups.Count == 2, nameof(matchResult.Groups.Count) + " == 2");
                installationDirectories.Add(matchResult.Groups[1].Value.Trim());
            }
        }

        foreach (var installationDirectory in installationDirectories)
        {
            foreach (var gameDirectory in Directory.GetDirectories(installationDirectory))
            {
                var installerDataXmlPath = Path.Combine(gameDirectory, "__Installer", "installerdata.xml");

                if (File.Exists(installerDataXmlPath))
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(installerDataXmlPath);
                    Debug.Assert(xmlDocument.DocumentElement != null, "xmlDocument.DocumentElement != null");

                    var contentIDsNode = xmlDocument.DocumentElement.SelectSingleNode("/DiPManifest/contentIDs");
                    if (contentIDsNode == null) { continue; }

                    string appId = null;
                    
                    foreach (XmlElement element in contentIDsNode)
                    {
                        if (element.Name == "contentID")
                        {
                            appId = element.FirstChild?.Value;
                        }
                    }

                    if (appId == null) { continue; }

                    string name = null;
                    
                    var runtimeNode = xmlDocument.DocumentElement.SelectSingleNode("/DiPManifest/runtime/launcher");
                    if (runtimeNode == null) { continue; }

                    foreach (XmlElement element in runtimeNode)
                    {
                        if (element.Name == "name")
                        {
                            name = StringExtensions.NormalizeGameName(element.FirstChild?.Value);
                            break;
                        }
                    }
                    
                    if (name == null) { continue; }

                    yield return new GameDescriptor(name, appId);
                }
            }
        }
    }

    public void RunGame(GameDescriptor game)
    {
        ProcessUtils.StartSilent($"origin://launchgame/{game.AppId}");
    }
}