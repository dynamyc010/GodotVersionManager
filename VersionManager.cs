using System;
using System.Diagnostics;
using Steamworks;
using Tommy;

namespace GodotVersionManager.Utilities
{
    class GodotVersion{
        public string version;
        public string path;
        public string executable;
        public bool isManual;
        public string? nickname;
        public string fullPath => @$"{path}\{executable}";
        public bool isMono => executable.Contains("mono");
        public string prettyName => this.version + (this.isMono ? " (.NET)" : "") + (this.nickname != null ? $" - {this.nickname}" : "");
        public GodotVersion(string version, string path, string executable, bool isManual = false, string? nickname = null){
            this.version = version;
            this.path = path;
            this.executable = executable;
            this.isManual = isManual;
            this.nickname = nickname == "" ? null : nickname;
        }

        public void changeNickname(string? newNickname){
            this.nickname = newNickname;
        }
    }
    class Config{
        public List<GodotVersion> godotVersions = new List<GodotVersion>();

        public int versionCount => godotVersions.Count;
        public bool useSteam = false;
        public string? scanPath;
        public Config(string? configPath){
            if(!File.Exists(@"config.toml") || new FileInfo(@"config.toml").Length == 0){
                Console.WriteLine("Config file not found, generating...");
                TomlTable template = new TomlTable();
                template["VersionManager"]["useSteam"] = true;
                template["VersionManager"]["scanPath"] = ".\\Versions";

                Console.WriteLine("Writing config file...");
                using(StreamWriter writer = File.CreateText("config.toml"))
                {
                    template.WriteTo(writer);
                    writer.Flush();
                }
            }

            using(StreamReader r = File.OpenText("config.toml")){
                TomlTable configTable = TOML.Parse(r);
                Console.WriteLine(configTable["VersionManager"]["useSteam"].ToString());
                useSteam = configTable["VersionManager"]["useSteam"];
                scanPath = (string)configTable["VersionManager"]["scanPath"];
                try{
                    foreach (var version in configTable["CachedVersions"].Children)
                    {
                        godotVersions.Add(new GodotVersion((string)version["version"], (string)version["path"], (string)version["executable"], version["isManual"], (string)version["nickname"]));
                    }
                }catch{
                    // Just continue if there are no cached versions, it's fine
                }
            }
        }

        internal void setNickname(int versionIndex, string? newNickname = null)
        {
            godotVersions[versionIndex].changeNickname(newNickname);
        }
    }
    class VersionManager
    {
        public Config config;
        
        public VersionManager()
        {
            config = new Config(@".\config.toml");
            if(config.versionCount == 0){
                scanForVersions();
            }
        }
        
        public void listInstalledVersions()
        {
            Console.WriteLine("Installed versions:");
            int i = 1;
            foreach (var version in config.godotVersions)
            {
                Console.WriteLine($"{i++}. " + version.prettyName);   
            }
            Console.WriteLine();
        }

        public void onSettingsChange(){
            TomlTable configTable = new TomlTable();
            configTable["VersionManager"]["useSteam"] = config.useSteam;
            configTable["VersionManager"]["cachedVersions"] = new TomlTable();
            int i = 0;
            foreach (var version in config.godotVersions)
            {
                // We turn them to a string so Tommy doesn't turn it into an array; it looks prettier this way.
                configTable["CachedVersions"][i.ToString()]["version"] = version.version;
                configTable["CachedVersions"][i.ToString()]["path"] = version.path;
                configTable["CachedVersions"][i.ToString()]["executable"] = version.executable;
                configTable["CachedVersions"][i.ToString()]["isManual"] = version.isManual;
                configTable["CachedVersions"][i++.ToString()]["nickname"] = version.nickname ?? "";
            }
            configTable["VersionManager"]["scanPath"] = config.scanPath;
            using(StreamWriter writer = File.CreateText("config.toml"))
            {
                configTable.WriteTo(writer);
                writer.Flush();
            }
        }

        public void scanForVersions(bool? shouldClear = false){
            if (shouldClear ?? false)
            {
                // This keeps manually added entries; also moves them to the beginning of the list, but dun care
                config.godotVersions = config.godotVersions.Where(x => x.isManual).ToList();
            }
            Console.WriteLine("Scanning " + config.scanPath + " for Godot versions...");
            if(!Directory.Exists(config.scanPath)){
                Console.WriteLine("Scan path not found, please create it or change the scan path in the Options menu.");
                return;
            }
            string[] scanVersions = Directory.GetDirectories(config.scanPath);
            foreach (var version in scanVersions)
            {
                string fullFoldername = Path.GetFileName(version);
                string versionEngine = fullFoldername.Split('_')[1];
                string executable = fullFoldername.EndsWith(".exe") ? fullFoldername : fullFoldername + ".exe";

                string versionPath = @$"{config.scanPath}\{fullFoldername}";

                if(!File.Exists($@"{versionPath}\{executable}")){
                    Console.WriteLine($"Godot install inside {fullFoldername} seems incomplete, skipping...");
                    Console.WriteLine($"Expected executable: {versionPath}\\{executable}");
                    continue;
                }

                if(config.godotVersions.Any(x => x.path == versionPath)){
                    Console.WriteLine($"Godot version {versionEngine} already known, skipping...");
                    continue;
                }

                config.godotVersions.Add(new GodotVersion(versionEngine, versionPath, executable));

                // Make a ._sc_ file to tell Godot to not trash all around the system
                if(!File.Exists($@"{versionPath}\._sc_")) 
                    File.Create($@"{versionPath}\._sc_").Close();

                Console.WriteLine("Successfully added " + versionEngine + " to the list of installed versions.");
            }
            onSettingsChange();
        }

        public bool addManualVersion(string path, string? executable = null, string? nickname = null){
            if(!Directory.Exists(path)){
                return false;
            }
            string fullFoldername = Path.GetFileName(path);
            string versionEngine = fullFoldername.Split('_')[1];
            executable = executable ?? (fullFoldername.EndsWith(".exe") ? fullFoldername : fullFoldername + ".exe");

            config.godotVersions.Add(new GodotVersion(versionEngine, path, executable, true, nickname));
            onSettingsChange();
            return true;
        }

        internal void runVersion(int chosenIndex)
        {
            SteamManager? steamManager = new SteamManager();;
            if(config.useSteam){
                steamManager.Init();
            }
            Console.Clear();
            GodotVersion chosenVersion = config.godotVersions[chosenIndex];
            Console.WriteLine($"Starting {chosenVersion.prettyName} ...");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = chosenVersion.fullPath;
            startInfo.WorkingDirectory = chosenVersion.path;
            startInfo.Arguments = "--path . --verbose";
            var process = Process.Start(startInfo);
            process!.WaitForExit();

            if(config.useSteam){
                steamManager.Dispose();
            }
            Console.Clear();
        }
    }
}