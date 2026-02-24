using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace LLMPlayer.Perception
{
    public class ScreenshotService : MonoBehaviour
    {
        private Camera _camera;
        private RenderTexture _renderTexture;
        private int _resolution;

        public void Initialize(Camera targetCamera, int resolution)
        {
            _camera = targetCamera;
            _resolution = resolution;
            _renderTexture = new RenderTexture(_resolution, _resolution, 24);
        }

        public void CaptureScreenshot(Action<byte[]> callback)
        {
            if (_camera == null) return;

            var oldTarget = _camera.targetTexture;
            _camera.targetTexture = _renderTexture;
            _camera.Render();
            _camera.targetTexture = oldTarget;

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadback.Request(_renderTexture, 0, TextureFormat.RGB24, (request) => {
                    if (request.hasError)
                    {
                        CaptureSync(callback);
                    }
                    else
                    {
                        var data = request.GetData<byte>();
                        var pngBytes = ImageConversion.EncodeNativeArrayToPNG(data, _renderTexture.graphicsFormat, (uint)_resolution, (uint)_resolution);
                        callback?.Invoke(pngBytes.ToArray());
                    }
                });
            }
            else
            {
                CaptureSync(callback);
            }
        }

        private void CaptureSync(Action<byte[]> callback)
        {
            RenderTexture.active = _renderTexture;
            Texture2D tex = new Texture2D(_resolution, _resolution, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, _resolution, _resolution), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);
            callback?.Invoke(bytes);
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }
    }
}
