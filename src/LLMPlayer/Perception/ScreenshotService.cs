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
                _renderTexture = null;
            }

            if (targetCamera == null)
            {
                Plugin.Instance.Log.LogWarning("ScreenshotService.Initialize received a null targetCamera. Capture will not be possible.");
                _camera = null;
                return;
            }

            if (resolution <= 0) resolution = 256; // Sane default

            _camera = targetCamera;
            _resolution = resolution;
            _renderTexture = new RenderTexture(_resolution, _resolution, 24);
        }

        public void CaptureScreenshot(Action<byte[]> callback)
        {
            if (_camera == null || _isDestroyed || _renderTexture == null) return;

            var oldTarget = _camera.targetTexture;
            _camera.targetTexture = _renderTexture;
            _camera.Render();
            _camera.targetTexture = oldTarget;

            // Capture local copies for async callback safety
            var localRT = _renderTexture;
            var localRes = _resolution;

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadback.Request(localRT, 0, TextureFormat.RGB24, (request) => {
                    if (_isDestroyed) return;

                    if (request.hasError)
                    {
                        CaptureSync(localRT, localRes, callback);
                    }
                    else
                    {
                        var data = request.GetData<byte>();
                        // Use RGB24 matching format
                        var format = UnityEngine.Experimental.Rendering.GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGB24, false);
                        var pngBytes = ImageConversion.EncodeNativeArrayToPNG(data, format, (uint)localRes, (uint)localRes);
                        try
                        {
                            callback?.Invoke(pngBytes.ToArray());
                        }
                        finally
                        {
                            pngBytes.Dispose();
                        }
                    }
                });
            }
            else
            {
                CaptureSync(localRT, localRes, callback);
            }
        }

        private void CaptureSync(RenderTexture rt, int res, Action<byte[]> callback)
        {
            if (rt == null) return;

            var priorActive = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
            tex.Apply();
            RenderTexture.active = priorActive;

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
