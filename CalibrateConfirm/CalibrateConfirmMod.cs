using MelonLoader;
using System.Collections;
using System.IO;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.XR;
using ReMod.Core.Notification;
using ReMod.Core.UI.QuickMenu;
using VRC.UI.Core;

[assembly: MelonInfo(typeof(CalibrateConfirm.Mod), CalibrateConfirm.BuildInfo.Name, CalibrateConfirm.BuildInfo.Version, CalibrateConfirm.BuildInfo.Author, CalibrateConfirm.BuildInfo.DownloadLink)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("ReMod.Core")]

namespace CalibrateConfirm;

internal static class BuildInfo
{
    public const string Name = "CalibrateConfirm (ReMod.Core)";
    public const string Author = "tetra, Lily";
    public const string Version = "3.1.0";
    public const string DownloadLink = "https://github.com/tetra-fox/VRCMods";
}

public class Mod : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = new(BuildInfo.Name);
    private static int _scenesLoaded;
    private static Sprite _confirmFbtSprite;

    public override void OnApplicationStart()
    {
        ReModCoreLoader.LoadReModCore(out _);
        Settings.Register();
        
        // load our asset bundle
        AssetBundle assetBundle;
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(/*BuildInfo.Name + */"CalibrateConfirm.icon.assetbundle");
        //                             |^^^^^^^^^^^^^^^^^^^|
        // I do not know why this breaks it, but it does. Please forgive me.

        using (MemoryStream tempStream = new((int)stream!.Length))
        {
            stream.CopyTo(tempStream);
            assetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
            assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        // load sprite from the asset bundle
        _confirmFbtSprite = assetBundle.LoadAsset_Internal("ConfirmFBT", Il2CppType.Of<Sprite>()).Cast<Sprite>();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
        if (_scenesLoaded > 2) return;
        _scenesLoaded++;
        if (_scenesLoaded != 2) return;
        if (ReModCoreLoader.failed) {
            Logger.Error("ReMod.Core failed to load, I will not create any buttons.");
            return;
        }

        MelonCoroutines.Start(WaitForQm());
    }

    private static void Init()
    {
        if (!XRDevice.isPresent) return;
        Logger.Msg("Initializing...");

        var calibrateFbtButton = Helpers.FindInactive("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/SitStandCalibrateButton/Button_CalibrateFBT");
        var sitStandGroup = Helpers.FindInactive("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickActions/SitStandCalibrateButton");
        UserInterface.MakeConfirmButton(calibrateFbtButton, sitStandGroup, _confirmFbtSprite);


        if (Settings.AddPromptToSettingsTab.Value)
        {
            var calibrateFbtButtonSettings = Helpers.FindInactive("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_FBT/Button_CalibrateFBT");
            var groupSettings = Helpers.FindInactive("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_FBT");
            UserInterface.MakeConfirmButton(calibrateFbtButtonSettings, groupSettings, _confirmFbtSprite);

            Helpers.FindInactive("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_FBT/Button_ConfirmFBT").transform.SetSiblingIndex(2);
        }

        Logger.Msg("Initialized!");
    }

    private static IEnumerator WaitForQm() {
        while (UIManager.field_Private_Static_UIManager_0 == null) yield return null;
        while (GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true) == null) yield return null;
        
        Init();
    }

    internal static IEnumerator Timeout(int length, ReMenuButton confirmFbtButton, GameObject calibrateFbtButton)
    {
        var timeLeft = length;

        while (timeLeft > 0)
        {
            confirmFbtButton.Text = "Confirm?\n" + timeLeft;
            timeLeft--;
            yield return new WaitForSeconds(1);
        }

        confirmFbtButton.Active = false;
        calibrateFbtButton.SetActive(true);
    }

    

    public override void OnPreferencesSaved()
    {
        if (!Settings.Changed) return;

        const string msg = "Preferences changed, please restart your game for changes to take effect";
        Logger.Warning(msg);
        // Old HUD Message will eventually break, plus it has bugs on it's own
        // Helpers.DisplayHudMessage($"[CalibrateConfirm]\n{msg}");
        NotificationSystem.EnqueueNotification("CalibrateConfirm", msg, 3f, _confirmFbtSprite);

        Settings.Changed = false;
    }
}