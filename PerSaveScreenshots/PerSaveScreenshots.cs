using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

namespace PerSaveScreenshots
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class PerSaveScreenshots : MonoBehaviour
    {
        public Camera maincamera;
        private int screenshotIndex = 0;
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
        private string depthScreenshotPath;
        private string saveName;
        private static Regex gameModeMatch = new Regex("(\\(SCIENCE_SANDBOX\\)$|\\(SANDBOX\\)$|\\(CAREER\\)$)");

        #nullable enable
        void Start()
        {
            Debug.Log("[PerSaveScreenshots]: STARTED");
            ScreenShot stockScreenshot = GameObject.FindObjectOfType<ScreenShot>();
            stockScreenshot.enabled = false;
            Debug.Log("[PerSaveScreenshots]: DISABLED STOCK");
            PerSaveDepthScreenshot? depthMode = null;
            try
            {
                switch (HighLogic.LoadedScene)
                {
                    case GameScenes.SPACECENTER:
                    case GameScenes.TRACKSTATION:
                    case GameScenes.EDITOR:
                    case GameScenes.FLIGHT:
                        var configs = GameDatabase.Instance.GetConfigs("PerSaveScreenshots").FirstOrDefault();
                        var depthEnabled = configs.config.GetValue("enableDepth").ToLower();
                        if (depthEnabled == "true")
                        {
                            depthMode = Camera.main.gameObject.AddComponent<PerSaveDepthScreenshot>();
                        }
                        break;
                    default:
                        break;

                }
            }
            catch (NullReferenceException nre)
            {
                Debug.LogException(nre);
            }
            finally
            {
                screenshotBasePath = (Application.platform == RuntimePlatform.OSXPlayer) ? Path.Combine(Application.dataPath, "../../Screenshots") : Path.Combine(Application.dataPath, "../Screenshots");
                Debug.Log($"[PerSaveScreenshots]: Base path {screenshotBasePath}");
                if (!Directory.Exists(screenshotBasePath))
                {
                    Directory.CreateDirectory(screenshotBasePath);
                }

                saveName = ProcessSaveTitle(HighLogic.CurrentGame.Title);
                saveScreenshotPath = Path.Combine(screenshotBasePath, saveName);
                Debug.Log($"[PerSaveScreenshots]: Save path {saveScreenshotPath}");
                if (!Directory.Exists(saveScreenshotPath))
                {
                    Directory.CreateDirectory(saveScreenshotPath);
                }

                depthScreenshotPath = Path.Combine(saveScreenshotPath, "depth");
                Debug.Log($"[PerSaveScreenshots]: Depth path {depthScreenshotPath}");
                if (!Directory.Exists(depthScreenshotPath))
                {
                    Directory.CreateDirectory(depthScreenshotPath);
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                allowUIHide = true;
            }
            else allowUIHide = false;

            if (depthMode != null)
            {
                depthMode.SetBasePath(depthScreenshotPath);
            }

            if (allowUIHide)
            {
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
                listenerAdded = true;
                Debug.Log("[PerSaveScreenshots]: LISTENER ADDED");
            }

            if (useConfigSuperSize)
            {
                superSize = GameSettings.SCREENSHOT_SUPERSIZE;
            }
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

        void VisibleScreenshot()
        {
            if (GameSettings.TAKE_SCREENSHOT.GetKeyDown())
            {
                screenshotIndex = 0;
                while (File.Exists(GetScreenshotPath()))
                {
                    screenshotIndex++;
                }
                Debug.Log($"Screenshot Path");
                ScreenCapture.CaptureScreenshot(GetScreenshotPath(), superSize);
                Debug.Log($"SCREENSHOT!! at {GetScreenshotPath()}");
            }
        }

        private void Update()
        {
            maincamera = FlightCamera.fetch.mainCamera;
            VisibleScreenshot();
            //DepthScreenshot();
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
                    return screenshotBasePath + "/screenshot" + screenshotIndex + ".png";
                case GameScenes.FLIGHT:
                default:
                    return saveScreenshotPath + "/screenshot" + screenshotIndex + ".png";
            } 
        }

        private string ProcessSaveTitle(string title)
        {
            
            return gameModeMatch.Replace(title, "").Trim();
        }
    }
}
