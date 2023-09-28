using System;
using Steamworks;

namespace GodotVersionManager.Utilities
{
    class SteamManager : IDisposable
    {
        public SteamManager()
        {
            // We should write the AppID into the file so Steam knows what game we are mimicing.
            if (!File.Exists(@".\steam_appid.txt"))
                File.WriteAllText(@".\steam_appid.txt", "404790");
        }
        public void Init() => SteamAPI.Init();
        public void Dispose(){
            
            SteamAPI.Shutdown();
        }
    }
}