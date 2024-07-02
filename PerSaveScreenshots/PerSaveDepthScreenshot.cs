using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerSaveScreenshots
{
    #nullable enable
    public class PerSaveDepthScreenshot : MonoBehaviour
    {
        private Material depthmat;
        private int depthshotIndex;
        private Queue<bool> imageCommand = new Queue<bool>(1);
        private string imageBasePath = "";

        // Start is called before the first frame update
        void Start()
        {
            depthmat = CreateDepthMaterial();
        }
        private Material? CreateDepthMaterial()
        {
            Shader shader = Shabby.Shabby.FindShader("PerSaveScreenshots/DepthEffect");
            if (shader == null)
            {
                return null;
            }
            return new Material(shader);
        }

        public void SetBasePath(string path)
        {
            imageBasePath = path;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                imageCommand.Enqueue(true);
                Camera.main.depthTextureMode = DepthTextureMode.Depth;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (imageCommand.Count > 0)
            {
                imageCommand.Clear();
                if (destination is null)
                {
                    Debug.Log("Destination is null");
                    return;
                }
                depthshotIndex = 0;
                while (File.Exists(GetDepthScreenshotPath()))
                {
                    depthshotIndex++;
                }
                //ScreenCapture.CaptureScreenshot(GetScreenshotPath(), superSize);
                Graphics.Blit(source, destination, depthmat);
                Texture2D depthTexFile = new Texture2D(destination.width, destination.height, TextureFormat.ARGB32, false);
                RenderTexture.active = destination;
                depthTexFile.ReadPixels(new Rect(0, 0, destination.width, destination.height), 0, 0);
                RenderTexture.active = null;
                byte[] fileBytes = depthTexFile.EncodeToPNG();
                File.WriteAllBytes(GetDepthScreenshotPath(), fileBytes);
                Debug.Log("DEPTH SCREENSHOT!!");
                Graphics.SetRenderTarget(destination);
                Camera.main.depthTextureMode = DepthTextureMode.None;
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }

        private string GetDepthScreenshotPath()
        {
            return @$"{imageBasePath}\depth_screenshot" + depthshotIndex + ".png";
        }
    }
}
