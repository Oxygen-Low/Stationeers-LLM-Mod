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

        /// <summary>
        /// Configures the service with the camera to capture from and the square resolution to use for screenshots.
        /// </summary>
        /// <param name="targetCamera">The Camera whose output will be captured.</param>
        /// <param name="resolution">The width and height, in pixels, of the square render texture to create.</param>
        public void Initialize(Camera targetCamera, int resolution)
        {
            _camera = targetCamera;
            _resolution = resolution;
            _renderTexture = new RenderTexture(_resolution, _resolution, 24);
        }

        /// <summary>
        /// Captures the current view from the configured camera and provides PNG-encoded image bytes via the callback.
        /// </summary>
        /// <param name="callback">Called with the PNG byte array of the captured square image at the configured resolution; may be invoked synchronously or asynchronously. If the internal camera is not set, the method returns without invoking this callback.</param>
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

        /// <summary>
        /// Synchronously captures the contents of the internal render texture, encodes it as a PNG, and delivers the resulting bytes to the provided callback.
        /// </summary>
        /// <param name="callback">Callback invoked with the PNG-encoded image bytes; may be null.</param>
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

        /// <summary>
        /// Releases and destroys the internal RenderTexture when the component is destroyed.
        /// </summary>
        /// <remarks>
        /// If a RenderTexture exists, this method releases its GPU resources and destroys the object to prevent memory leaks.
        /// This is invoked by Unity when the GameObject or component is being removed.
        /// </remarks>
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
