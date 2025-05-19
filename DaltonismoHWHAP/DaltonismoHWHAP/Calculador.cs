using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace DaltonismoHWHAP
{
    public struct LabColor
    {
        public double L;
        public double A;
        public double B;

        public LabColor(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }
    }

    internal class Calculador
    {
        /*Valores de delta E:
         *  - 0 - 1: Imperceptible
         *  - 1 - 2: Perceptible con comparación directa
         *  - 2 - 10: Claramente perceptible
         *  - 11 - 49: Colores muy diferentes
         *  - 50+: Colores completamente distintos*/
        private List<double> resultadosOriginal;
        private List<double> resultadosImagenDalt;
        //private List<double> resultadosEntreImagenes;

        public Calculador()
        {
            resultadosOriginal = new List<double>();
            resultadosImagenDalt = new List<double>();
           // resultadosEntreImagenes = new List<double>();
        }

        public double DeltaE(Color c1, Color c2)
        {
            LabColor lab1=RGBToLab(c1.R, c1.G, c1.B);
            LabColor lab2=RGBToLab(c2.R, c2.G, c2.B);

            double deltaL = lab1.L - lab2.L;
            double deltaA = lab1.A - lab2.A;
            double deltaB = lab1.B - lab2.B;

            return  Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
        }

        //El espacio de color CIELAB(Lab) fue diseñado para que:
            //  - Las distancias entre colores reflejen lo que el ojo humano realmente percibe.
            //  - Una distancia mayor entre dos colores en Lab implica una diferencia visual más notoria.
            //  - Es perceptualmente uniforme: una misma distancia ΔE ≈ 2.3 es el umbral que una persona promedio empieza a notar.
        private LabColor RGBToLab(byte r, byte g, byte b)
        {
            // Normaliza a [0, 1]
            double rLin = PivotRGB(r / 255.0);
            double gLin = PivotRGB(g / 255.0);
            double bLin = PivotRGB(b / 255.0);

            // Conversión RGB lineal a XYZ (referencia D65)
            double x = rLin * 0.4124 + gLin * 0.3576 + bLin * 0.1805;
            double y = rLin * 0.2126 + gLin * 0.7152 + bLin * 0.0722;
            double z = rLin * 0.0193 + gLin * 0.1192 + bLin * 0.9505;

            // Normalizar con el blanco de referencia
            x /= 0.95047;
            y /= 1.00000;
            z /= 1.08883;

            // XYZ a Lab
            x = PivotXYZ(x);
            y = PivotXYZ(y);
            z = PivotXYZ(z);

            double L = 116.0 * y - 16.0;
            double A = 500.0 * (x - y);
            double B = 200.0 * (y - z);

            return new LabColor(L, A, B);
        }

        // Convierte un componente RGB de sRGB a espacio lineal.
        /// La fórmula aplica una corrección gamma inversa para obtener valores proporcionales a la energía luminosa real.
        private double PivotRGB(double n)
        {
            // En sRGB, los valores se codifican con una curva gamma (no lineal).
            // Esta función invierte esa curva para obtener un valor "lineal":
            // - Si es mayor que el umbral, aplica una potencia (2.4) para deshacer la curva gamma.
            // - Si no, usa una aproximación lineal simple.
           
            return (n > 0.04045) ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92;
        }


        // Aplica la transformación no lineal necesaria al convertir de XYZ a Lab.
        /// Esta función se basa en la fórmula del estándar CIE para obtener una percepción uniforme.
        private double PivotXYZ(double n)
        {
            // El espacio Lab está diseñado para que la distancia entre colores
            // se perciba como proporcional a la diferencia visual real.
            // Esta función aplica una raíz cúbica o una aproximación lineal,
            // dependiendo de si el valor supera un umbral:
            // - Si es mayor a 0.008856, se aplica raíz cúbica.
            // - Si no, se usa una línea tangente para evitar discontinuidades.
            return (n > 0.008856) ? Math.Pow(n, 1.0 / 3.0) : (7.787 * n) + (16.0 / 116.0);
        }

        public void GenerateResults(ref Bitmap original, ref Bitmap imDalt, int tamMatriz, string dtType, int index, string fName)
        {
            int width = original.Width;
            int height = original.Height;
            int radius = tamMatriz / 2;

            // Preparamos los bitmap. Lockbit guarda un bitmap en la memoria del sistema para poder cambiarlo programaticamente
            BitmapData dataOriginal = original.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataDalt = imDalt.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int strideO = dataOriginal.Stride;
            int strideD = dataDalt.Stride;

            byte[] bufferO = new byte[strideO * height];
            byte[] bufferD = new byte[strideD * height];

            Marshal.Copy(dataOriginal.Scan0, bufferO, 0, bufferO.Length);
            Marshal.Copy(dataDalt.Scan0, bufferD, 0, bufferD.Length);

            original.UnlockBits(dataOriginal);
            imDalt.UnlockBits(dataDalt);

            resultadosOriginal.Clear();
            resultadosImagenDalt.Clear();

            for (int y = 0; y < height; y++) 
            {
                for (int x = 0; x < width; x += 2)
                {
                    int indexO = y * strideO + x * 3;
                    int indexD = y * strideD + x * 3;

                    Color origen1 = Color.FromArgb(bufferO[indexO + 2], bufferO[indexO + 1], bufferO[indexO]);
                    Color dalt1 = Color.FromArgb(bufferD[indexD + 2], bufferD[indexD + 1], bufferD[indexD]);

                    double sumDeltaE = 0;
                    double sumDeltaEDalt = 0;
                    int cont = 0;

                    for (int k = y - radius; k <= y + radius; k++)
                    {
                        for (int l = x - radius; l <= x + radius; l++)
                        {
                            if ((k == y && l == x) || k < 0 || k >= height || l < 0 || l >= width)
                                continue;

                            int idxK = k * strideO + l * 3;
                            int idxKDalt=k * strideD + l * 3;
                            Color vecinoO = Color.FromArgb(bufferO[idxK + 2], bufferO[idxK + 1], bufferO[idxK]);
                            Color vecinoD = Color.FromArgb(bufferD[idxKDalt + 2], bufferD[idxKDalt + 1], bufferD[idxKDalt]);

                            sumDeltaE += DeltaE(origen1, vecinoO);
                            sumDeltaEDalt += DeltaE(dalt1, vecinoD);
                            cont++;
                        }
                    }

                    if (cont > 0)
                    {
                        resultadosOriginal.Add(sumDeltaE/cont);
                        resultadosImagenDalt.Add(sumDeltaEDalt / cont);
                        //resultadosEntreImagenes.Add(deltaE(origen1, dalt1));
                    }
                }
            }

            GenerateHeatMap(ref original, width, height, 2, dtType, index, fName);
        }
        private void GenerateHeatMap(ref Bitmap original, int width, int height, double umbral, string name, int i, string folderName)
        {
            Bitmap baseImg = original;
            Bitmap mapa = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(mapa))
            {
                g.DrawImage(baseImg, 0, 0); // Dibuja la imagen base
            }

            int xIndex, yIndex = 0;
            for (int y = 0; y < height; y += 2) 
            {
                xIndex = 0;
                for (int x = 0; x < width; x += 2) 
                {
                    int index = yIndex * width + xIndex;
                    if (index < 0 || index >= resultadosOriginal.Count || index >= resultadosImagenDalt.Count) continue;
                    double deltaEOri = resultadosOriginal[index];
                    double deltaEDalt = resultadosImagenDalt[index];
                    //   double resEntreIm = resultadosEntreImagenes[index];
                    Color colorResultado;

                    //deltaEOri< umbral ->imagen original, los pixeles son imperceptibles los cambios de color
                    //deltaEDalt<umbral->imagen filtro, imperceptibles los cambios de color
                    //resEntreIm-> Si es pequeño no se percibe cambio de color entre la imagen original y la del filtro

                    //Se distinguen los pixeles vecinos en la original pero no en la del filtro
                    if ((deltaEOri > umbral && deltaEDalt <= umbral* 0.5))
                    {
                        // Problema grave: se ve bien normalmente, pero mal con daltonismo
                        colorResultado = Color.FromArgb(200, 255, 0, 0); // Rojo
                    }

                    //Se distinguen los pixeles vecinos en la original pero en la del filtro cuesta distinguirlos
                    else if (deltaEOri > umbral  && deltaEDalt <= umbral)
                    {
                        // Problema medio: pérdida parcial de contraste
                        colorResultado = Color.FromArgb(200, 255, 255, 0); // Amarillo
                    }
                    //Los colores en la imagen original y en la del filtro cambian y se percibe
                    else if (deltaEOri > umbral   && deltaEDalt > umbral ) 
                    {
                        // Contraste aceptable incluso para personas con daltonismo
                        colorResultado = Color.FromArgb(200, 0, 255, 0); // Verde
                    }
                    //deltaEOri no supera el umbral-> en la imagen original no cambia de color (es imperceptible)
                    else
                    {
                        // colorResultado = Color.FromArgb(150, 0, 128, 0); // Verde oscuro con opacidad 
                        colorResultado = Color.Transparent;
                    }

                    mapa.SetPixel(x, y, colorResultado);
                    if (x + 1 < width) mapa.SetPixel(x + 1, y, colorResultado);
                    if (y + 1 < height) mapa.SetPixel(x, y + 1, colorResultado);
                    if (x + 1 < width && y + 1 < height) mapa.SetPixel(x + 1, y + 1, colorResultado);
                    xIndex++;
                }
                yIndex++;
            }

            string folderPath = "Analisis_Daltonismo/" + folderName + "/Captura" + i;
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "HeatMap" + name + ".png");
            mapa.Save(filePath, ImageFormat.Png);

        }
    }
}
