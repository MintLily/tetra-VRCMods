using System;
using System.Net;
using System.Reflection;

namespace CalibrateConfirm; 

public class ReModCoreLoader {
    internal static bool failed;
    public static void LoadReModCore(out Assembly loadedAssembly) {
        byte[] bytes = null;
        var wc = new WebClient();
        try {
            bytes = wc.DownloadData("https://github.com/RequiDev/ReMod.Core/releases/latest/download/ReMod.Core.dll");
            loadedAssembly = Assembly.Load(bytes);
            Mod.Logger.Msg("Successfully Loaded ReMod.Core");
        }
        catch (WebException e) {
            failed = true;
            Mod.Logger.Error($"Unable to Load Core Dep ReModCore: {e}");
        }
        catch (BadImageFormatException) {
            failed = true;
            loadedAssembly = null;
        }
        loadedAssembly = null;
    }
}