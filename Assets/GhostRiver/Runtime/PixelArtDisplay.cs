using UnityEngine;
using UnityEngine.UI;

namespace GhostRiver
{
    [RequireComponent(typeof(Camera))]
    public sealed class PixelArtDisplay : MonoBehaviour
    {
        private const int PixelWidth = 426;
        private const int PixelHeight = 240;
        private Camera targetCamera;
        private RenderTexture pixelTexture;
        private RawImage screen;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            pixelTexture = new RenderTexture(PixelWidth, PixelHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = "426x240 · bảng màu pixel",
                filterMode = FilterMode.Point
            };
            pixelTexture.Create();
            targetCamera.targetTexture = pixelTexture;
        }

        public void Attach(RawImage image)
        {
            screen = image;
            screen.texture = pixelTexture;
        }

        private void OnDestroy()
        {
            if (targetCamera != null)
                targetCamera.targetTexture = null;
            if (pixelTexture != null)
            {
                pixelTexture.Release();
                Destroy(pixelTexture);
            }
        }
    }
}
