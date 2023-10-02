using System;
using GodotVersionManager.Utilities;

namespace GodotVersionManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            VersionManager versionManager = new VersionManager();
            Console.Clear();
            string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            while(true){
                Console.WriteLine($"Godot Version Manager - v{appVersion}");
                Console.WriteLine(versionManager.config.useSteam ? "Steam hours are being counted." : "Steam hours are not being counted.");
                Console.WriteLine();
                versionManager.listInstalledVersions();
                Console.WriteLine("R. Rescan for versions");
                Console.WriteLine("X. Change settings");
                Console.WriteLine("Q. Quit");
                Console.Write("> ");
                var input = Console.ReadLine().ToLower();
                int chosenIndex;

                Console.Clear();
                if(int.TryParse(input, out chosenIndex)){
                    if(--chosenIndex < versionManager.config.versionCount && chosenIndex >= 0){
                        versionManager.runVersion(chosenIndex);
                    }
                    else{
                        Console.WriteLine("Invalid version number.");
                    }
                }

                else if (input == "r"){
                    if (Directory.Exists(versionManager.config.scanPath))
                    {
                        Console.WriteLine("Do you wish to clear already cached versions (excluding manually added versions)? (y/n)");
                        Console.Write("> ");
                        var clearInput = Console.ReadLine()?.ToLower();
                        Console.Clear();
                        versionManager.scanForVersions(clearInput == "y");
                        Console.WriteLine("Done! Press any key to continue.");
                        Console.ReadKey();
                        Console.Clear();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Scan path not found, please update it in settings.");
                    }
                    versionManager.onSettingsChange();
                }
                else if(input == "x"){
                    // TODO: Implement settings
                    while(true){
                        Console.WriteLine("Settings Panel");
                        Console.WriteLine("1. Toggle Steam hours");
                        Console.WriteLine("2. Change scan path");
                        Console.WriteLine("3. Change nickname");
                        Console.WriteLine();
                        Console.WriteLine("Q. Return to main menu");
                        Console.Write("> ");
                        var settingsInput = Console.ReadLine()?.ToLower();

                        Console.Clear();
                        if (settingsInput == "1")
                        {
                            versionManager.config.useSteam = !versionManager.config.useSteam;
                            Console.Clear();
                            Console.WriteLine("Steam hours are now " + (versionManager.config.useSteam ? "being counted." : "not counted."));
                            versionManager.onSettingsChange();
                        }
                        else if (settingsInput == "2")
                        {
                            Console.WriteLine("Enter new scan path:");
                            Console.Write("> ");
                            var newScanPath = Console.ReadLine();
                            if (Directory.Exists(newScanPath))
                            {
                                versionManager.config.scanPath = newScanPath;
                                Console.WriteLine("Do you wish to clear already cached versions (excluding manually added versions)? (y/n)");
                                Console.Write("> ");
                                var clearInput = Console.ReadLine()?.ToLower();
                                Console.Clear();
                                versionManager.scanForVersions(clearInput == "y");
                                Console.WriteLine("Done! Press any key to continue.");
                                Console.ReadKey();
                                Console.Clear();
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Scan path not found, please choose some place that exists.");
                            }
                            versionManager.onSettingsChange();
                        }
                        else if (settingsInput == "3")
                        {
                            Console.WriteLine("Enter item number of version to change nickname for:");
                            versionManager.listInstalledVersions();
                            Console.Write("> ");
                            int versionIndex;
                            var versionIndexString = Console.ReadLine();
                            if (versionIndexString == "") continue;
                            if (!int.TryParse(versionIndexString, out versionIndex)) continue;
                            if(--versionIndex >= versionManager.config.versionCount || versionIndex < 0){
                                Console.WriteLine("Invalid version number.");
                                continue;
                            }
                            Console.WriteLine("Enter new nickname (leave blank to remove nickname)");
                            Console.Write("> ");
                            var newNickname = Console.ReadLine();
                            if (newNickname == "") newNickname = null;
                            versionManager.config.setNickname(versionIndex, newNickname);
                            versionManager.onSettingsChange();
                            Console.Clear();
                        }
                        else if (settingsInput == "q")
                        {
                            //versionManager.onSettingsChange();
                            break;
                        }
                        else{
                            Console.WriteLine("Invalid input.");
                        }
                        //Console.Clear();
                    }
                }
                else if(input == "q"){
                    // TODO: Implement graceful shutdown
                    return;
                }
                else{
                    Console.WriteLine("Invalid input.");
                }

            }
        }
    }
}