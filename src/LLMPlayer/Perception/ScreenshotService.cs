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
        private bool _isDestroyed = false;

        public void Initialize(Camera targetCamera, int resolution)
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }

            _camera = targetCamera;
            _resolution = resolution;
            _renderTexture = new RenderTexture(_resolution, _resolution, 24);
        }

        public void CaptureScreenshot(Action<byte[]> callback)
        {
            if (_camera == null || _isDestroyed) return;

            var oldTarget = _camera.targetTexture;
            _camera.targetTexture = _renderTexture;
            _camera.Render();
            _camera.targetTexture = oldTarget;

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadback.Request(_renderTexture, 0, TextureFormat.RGB24, (request) => {
                    if (_isDestroyed) return;

                    if (request.hasError)
                    {
                        CaptureSync(callback);
                    }
                    else
                    {
                        var data = request.GetData<byte>();
                        // Use RGB24 matching format
                        var format = UnityEngine.Experimental.Rendering.GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGB24, false);
                        var pngBytes = ImageConversion.EncodeNativeArrayToPNG(data, format, (uint)_resolution, (uint)_resolution);
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
            _isDestroyed = true;
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }
    }
}
