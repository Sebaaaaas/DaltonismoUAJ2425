using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

namespace DaltonismoHWHAP
{
    public class FiltroDaltonismo
    {        
        Dictionary<Filtros, float[,]> valoresFiltros = new Dictionary<Filtros, float[,]>();
        public enum Filtros { Protanopia, Deuteranopia, Tritanopia, Acromatopsia }
        public FiltroDaltonismo() {

            //La protanopia es la carencia de sensibilidad al color rojo
            valoresFiltros.Add(Filtros.Protanopia, new float[,] {
                  { 0.152286f, 1.052583f, -0.204868f },
                  { 0.114503f, 0.786281f, 0.099216f },
                  { -0.003882f, -0.048116f, 1.051998f }
            });

            //La deuteranopia es la carencia de sensibilidad al color verde
            valoresFiltros.Add(Filtros.Deuteranopia, new float[,] {
                  { 0.367322f, 0.860646f, -0.227968f },
                  { 0.280085f, 0.672501f, 0.047413f },
                  { -0.011820f, 0.042940f, 0.968881f }
            });

           //La tritanopia es la carencia de sensibilidad al color azul
            valoresFiltros.Add(Filtros.Tritanopia, new float[,] {
                  { 1.255528f, -0.076749f, -0.178779f },
                  { -0.078411f, 0.930809f, 0.147602f },
                  { 0.004733f, 0.691367f, 0.303900f }
            });

            //La acromatopsia es la ausencia de vision de todos los colores y solo se perciben los blancos, negros y grises
            valoresFiltros.Add(Filtros.Acromatopsia, new float[,] {
                  { 0.299f, 0.587f, 0.114f },
                  { 0.299f, 0.587f, 0.114f },
                  { 0.299f, 0.587f, 0.114f }
            });
        }

        // Version eficiente de un filtro, accedemos a memoria directamente en lugar de llamar a GetPixel/SetPixel
        public void SimularFiltro(Bitmap bmp, Filtros filtro)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpData.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bmpData.Scan0;

            // Marshal nos proporciona funciones que podemos usar para manejar memoria con mayor libertad
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            for (int y = 0; y < bmp.Height; y++)
            {
                int row = y * bmpData.Stride;
                for (int x = 0; x < bmp.Width; x++)
                {
                    int i = row + x * bytesPerPixel;

                    float r = pixels[i + 2];
                    float g = pixels[i + 1];
                    float b = pixels[i];

                    float rPrime, gPrime, bPrime;

                    aplicaFiltroDaltonismo(filtro, r, g, b, out rPrime, out gPrime, out bPrime);

                    pixels[i + 2] = (byte)Math.Min(255, Math.Max(0, (int)rPrime));
                    pixels[i + 1] = (byte)Math.Min(255, Math.Max(0, (int)gPrime));
                    pixels[i] = (byte)Math.Min(255, Math.Max(0, (int)bPrime));
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bmp.UnlockBits(bmpData);
        }

        public Bitmap SimulateFilterOnGPU(RenderTexture sourceImage, ComputeShader compshader, int type)
        {
            int kernelHandle = compshader.FindKernel("CSFiltrosDaltonismo");

            RenderTexture resultTexture = new RenderTexture(sourceImage.width, sourceImage.height, 0);
            resultTexture.enableRandomWrite = true;
            resultTexture.Create();

            compshader.SetTexture(kernelHandle, "Source", sourceImage);
            compshader.SetTexture(kernelHandle, "Result", resultTexture);
            compshader.SetInts("SourceTextureSize", sourceImage.width, sourceImage.height);
            compshader.SetInt("type", type);

            int threadGroupsX = Mathf.CeilToInt(sourceImage.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(sourceImage.height / 8.0f);

            compshader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

            Texture2D tex = new Texture2D(resultTexture.width, resultTexture.height, TextureFormat.RGB24, false);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = resultTexture;

            tex.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0);
            tex.Apply();

            RenderTexture.active = previous;

            byte[] pngData = tex.EncodeToPNG();
            Bitmap bmp;
            using (MemoryStream ms = new MemoryStream(pngData, 0, pngData.Length))
            {
                using (Bitmap temp = new Bitmap(ms))
                {
                    bmp = new Bitmap(temp);
                }
            }

            UnityEngine.Object.Destroy(tex);
            resultTexture.Release();

            return bmp;
        }

        private void aplicaFiltroDaltonismo(Filtros filtro, float r, float g, float b, out float rPrime, out float gPrime, out float bPrime)
        {
            rPrime = r * valoresFiltros[filtro][0, 0] + g * valoresFiltros[filtro][0, 1] + b * valoresFiltros[filtro][0, 2];
            gPrime = r * valoresFiltros[filtro][1, 0] + g * valoresFiltros[filtro][1, 1] + b * valoresFiltros[filtro][1, 2];
            bPrime = r * valoresFiltros[filtro][2, 0] + g * valoresFiltros[filtro][2, 1] + b * valoresFiltros[filtro][2, 2];
        }
    }

}
