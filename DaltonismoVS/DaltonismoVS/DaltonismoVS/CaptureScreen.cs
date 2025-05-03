using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;



namespace DaltonismoVS
{
    internal class CaptureScreen
    {

        private GraphicsCaptureItem _item;
        private Direct3D11CaptureFramePool _framePool;
        private CanvasDevice _canvasDevice;
        private GraphicsCaptureSession _session;
        public void CaptureCurrentScreen()
        {
            Console.WriteLine("Nice screen");
            //_session.StartCapture();
        }

        public void StartCaptureInternal(GraphicsCaptureItem item)
        {
            _item = item;

            _framePool = Direct3D11CaptureFramePool.Create(
                _canvasDevice, // D3D device
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, // Pixel format
                2, // Number of frames
                _item.Size); // Size of the buffers

            _session = _framePool.CreateCaptureSession(_item); // Conseguimos una referencia al GraphicsCaptureSession
        }


        // Esta función en teoría pide al usuaruui que indique la pantalla a capturar, y comienza a capturar si elige algo válido
        public async Task StartCaptureAsync()
        {
            // The GraphicsCapturePicker follows the same pattern the
            // file pickers do.
            var picker = new GraphicsCapturePicker(); // Esto llama a la IU para que el usuario seleccione la pantalla para sacar capturas
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            // The item may be null if the user dismissed the
            // control without making a selection or hit Cancel.
            if (item != null)
            {
                StartCaptureInternal(item);
            }
        }
    }

}
