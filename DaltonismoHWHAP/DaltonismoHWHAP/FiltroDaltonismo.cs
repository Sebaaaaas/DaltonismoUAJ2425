using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DaltonismoHWHAP
{
    public class FiltroDaltonismo
    {        
        Dictionary<Filtros, float[,]> valoresFiltros = new Dictionary<Filtros, float[,]>();
        public enum Filtros { Protanomalia, Deuteranomalia, Tritanomalia, Protanopia, Deuteranopia, Tritanopia, Acromatopia, Acromatomalia}
        public FiltroDaltonismo() {

            valoresFiltros.Add(Filtros.Protanomalia, new float[,] {
                { 0.81667f, 0.18333f, 0.0f },
                {0.33333f, 0.66667f, 0.0f },
                {0.0f, 0.125f, 0.875f }
            });

            valoresFiltros.Add(Filtros.Deuteranomalia, new float[,] {
                { 0.8f, 0.2f, 0.0f },
                { 0.25833f, 0.74167f, 0.0f },
                { 0.0f, 0.14167f, 0.85833f }
            });

            valoresFiltros.Add(Filtros.Tritanomalia, new float[,] {
                { 0.96667f, 0.3333f, 0.0f },
                { 0.0f, 0.73333f, 0.26667f },
                { 0.0f, 0.18333f, 0.81667f }
            });           

            valoresFiltros.Add(Filtros.Protanopia, new float[,] {
                {0.56667f, 0.43333f, 0.0f},
                {0.55833f, 0.44167f, 0.0f},
                {0.0f, 0.24167f, 0.75833f}
            });

            valoresFiltros.Add(Filtros.Deuteranopia, new float[,] {
                { 0.625f, 0.375f, 0.0f },
                { 0.70f, 0.30f, 0.0f },
                { 0.0f, 0.30f, 0.70f }
            });

            valoresFiltros.Add(Filtros.Tritanopia, new float[,] {
                { 0.95f, 0.05f, 0.0f },
                { 0.0f, 0.43333f, 0.56667f },
                { 0.0f, 0.475f, 0.525f }
            });

            valoresFiltros.Add(Filtros.Acromatopia, new float[,] {
                { 0.299f, 0.587f, 0.114f },
                { 0.299f, 0.587f, 0.114f },
                { 0.299f, 0.587f, 0.114f }
            });

            valoresFiltros.Add(Filtros.Acromatomalia, new float[,] {
                { 0.618f, 0.32f, 0.062f },
                { 0.163f, 0.775f, 0.062f },
                { 0.163f, 0.32f, 0.516f }
            });
        }

        // Altera el bitmap que se pasa (necesitamos una copia en el futuro, no alterar el original)
        public void filtroTest(Bitmap bmp)
        {
            int height = 0, width = 0;
            for (; height < bmp.Height; ++height)
                for (width = 0; width < bmp.Width; ++width)
                {
                    Color pixelColor = bmp.GetPixel(width, height);
                    bmp.SetPixel(width, height, Color.FromArgb(pixelColor.B, pixelColor.R, pixelColor.G));
                }
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
        
    
        private void aplicaFiltroDaltonismo(Filtros filtro, float r, float g, float b, out float rPrime, out float gPrime, out float bPrime)
        {
            rPrime = r * valoresFiltros[filtro][0, 0] + g * valoresFiltros[filtro][0, 1] + b * valoresFiltros[filtro][0, 2];
            gPrime = r * valoresFiltros[filtro][1, 0] + g * valoresFiltros[filtro][1, 1] + b * valoresFiltros[filtro][1, 2];
            bPrime = r * valoresFiltros[filtro][2, 0] + g * valoresFiltros[filtro][2, 1] + b * valoresFiltros[filtro][2, 2];
        }
    }

}
