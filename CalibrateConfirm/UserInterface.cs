using System;
using CalibrateConfirm.Components;
using MelonLoader;
using ReMod.Core.UI.QuickMenu;
using UnityEngine;
using UnityEngine.UI;

namespace CalibrateConfirm;

public class UserInterface {
    private static ReMenuButton _reConfirmFbtButton;

    internal static void MakeConfirmButton(GameObject calibrateFbtButton, GameObject buttonGroup, Sprite confirmFbtSprite)
    {
        var calibrateMethod = Helpers.CalibrateMethod;

        object timeout = new();
        
        _reConfirmFbtButton = new ReMenuButton("Confirm?", "Are you sure you want to calibrate?", () => {
            calibrateMethod.Invoke(VRCTrackingManager.field_Private_Static_VRCTrackingManager_0, null);
            _reConfirmFbtButton.Active = false;
            calibrateFbtButton.SetActive(true);
            MelonCoroutines.Stop(timeout);
        }, buttonGroup.transform, confirmFbtSprite) {
            GameObject = {
                name = "Button_ConfirmFBT"
            }
        };

        var watcher = calibrateFbtButton.gameObject.AddComponent<ButtonWatcher>();
        watcher.reference = _reConfirmFbtButton.GameObject;
        watcher.target = calibrateFbtButton;

        _reConfirmFbtButton.Active = false;
        
        _reConfirmFbtButton.GameObject.transform.SetParent(buttonGroup.transform);

        // reset original calibrate button's onClick
        calibrateFbtButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();

        calibrateFbtButton.GetComponent<Button>().onClick.AddListener((Action)delegate {
            _reConfirmFbtButton.Active = true;
            calibrateFbtButton.SetActive(false);
            timeout = MelonCoroutines.Start(Mod.Timeout(Settings.PromptLength.Value, _reConfirmFbtButton, calibrateFbtButton));
        });
    }
}