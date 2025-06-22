#if UNITY_EDITOR
using System.Collections.Generic;
using PlazmaGames.Utilities;
using PlazmaGames.Core;
using System.IO;
using UnityEngine;


namespace HTJ21
{
    public class CameraScreenCapture : MonoBehaviour
    {
        public string fileName;
        public KeyCode screenshotKey;
        public Transform _itemParent;

        private Camera Camera
        {
            get
            {
                if (!_camera)
                {
                    _camera = GetComponent<Camera>();
                }
                return _camera;
            }
        }
        private Camera _camera;

        private void LateUpdate()
        {
            if (Input.GetKeyDown(screenshotKey)) Capture();
        }

        public void Capture()
        {
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = Camera.targetTexture;

            Camera.Render();

            Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height, TextureFormat.RGBA32_SIGNED, false, true);
            image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRenderTexture;

            byte[] bytes = image.EncodeToPNG();
            Destroy(image);

            string fn = fileName;

            string path = Application.dataPath + "/Textures/ScreenCaptures" + fn + ".png";
            Debug.Log($"Saving icon to {path}.");
            File.WriteAllBytes(path, bytes);
        }
    }
}
#endif