using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace PerSaveScreenshots
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class PerSaveScreenshots : MonoBehaviour
    {
        public Camera maincamera;
        private int i = 0;
        private bool uiHidden;
        public bool allowUIHide;
        public bool ScreenShotCameraMode;
        public bool useConfigSuperSize = true;
        public int superSize;
        private bool listenerAdded;
        private FlightCamera.TargetMode targetMode;
        private Transform target;
        private string screenshotBasePath;
        private string saveScreenshotPath;
        private string saveName;
        void Start()
        {
            ScreenShot stockScreenshot = GameObject.FindObjectOfType<ScreenShot>();
            stockScreenshot.enabled = false;

            screenshotBasePath = (Application.platform == RuntimePlatform.OSXPlayer) ? Path.Combine(Application.dataPath, "../../Screenshots") : Path.Combine(Application.dataPath, "../Screenshots");
            if (!Directory.Exists(screenshotBasePath))
            {
                Directory.CreateDirectory(screenshotBasePath);
            }

            saveName = ProcessSaveTitle(HighLogic.CurrentGame.Title);
            saveScreenshotPath = Path.Combine(screenshotBasePath, saveName);
            if (!Directory.Exists(saveScreenshotPath))
            {
                Directory.CreateDirectory(saveScreenshotPath);
            }

            if (allowUIHide)
            {
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
                listenerAdded = true;
            }

            if (useConfigSuperSize)
            {
                superSize = GameSettings.SCREENSHOT_SUPERSIZE;
            }

            GameEvents.onGameUnpause.Add(OnGameUnpause);
        }

        private void OnDestroy()
        {
            if (listenerAdded)
            {
                GameEvents.onShowUI.Remove(ShowUI);
                GameEvents.onHideUI.Remove(HideUI);
            }
            GameEvents.onGameUnpause.Remove(OnGameUnpause);
        }

        private void Update()
        {
            if (GameSettings.TAKE_SCREENSHOT.GetKeyDown())
            {
                i = 0;
                while (File.Exists(GetScreenshotPath()))
                {
                    i++;
                }
                ScreenCapture.CaptureScreenshot(GetScreenshotPath(), superSize);
                Debug.Log("SCREENSHOT!!");
            }
            if (!GameSettings.TOGGLE_UI.GetKeyDown() || !allowUIHide || HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return;
            }
            if (uiHidden)
            {
                OnGameUnpause();
                GameEvents.onShowUI.Fire();
                return;
            }
            if (FlightDriver.Pause && FlightCamera.fetch != null)
            {
                ScreenShotCameraMode = true;
                targetMode = FlightCamera.fetch.targetMode;
                target = FlightCamera.fetch.Target;
                float distance = FlightCamera.fetch.Distance;
                FlightCamera.fetch.SetTarget(target, targetMode);
                FlightCamera.fetch.SetDistanceImmediate(distance);
            }
            GameEvents.onHideUI.Fire();
        }

        private void OnGameUnpause()
        {
            if (!ScreenShotCameraMode)
            {
                return;
            }
            ScreenShotCameraMode = false;
            if (FlightCamera.fetch is null)
            {
                return;
            }
            FlightCamera.fetch.SetTarget(target, keepWorldPos: true, targetMode);
            return;
        }

        private void ShowUI()
        {
            uiHidden = false;
        }

        private void HideUI()
        {
            uiHidden = true;
        }

        private string GetScreenshotPath()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.LOADING:
                case GameScenes.MAINMENU:
                case GameScenes.CREDITS:
                case GameScenes.SETTINGS:
                    return screenshotBasePath + "/screenshot" + i + ".png";
                default:
                    return saveScreenshotPath + "/screenshot" + i + ".png";
            }
            
        }

        private string ProcessSaveTitle(string title)
        {
            Regex gameModeMatch = new Regex("(\\(SCIENCE_SANDBOX\\)$|\\(SANDBOX\\)$|\\(CAREER\\)$)");
            return gameModeMatch.Replace(title, "").Trim();
        }
    }
}
