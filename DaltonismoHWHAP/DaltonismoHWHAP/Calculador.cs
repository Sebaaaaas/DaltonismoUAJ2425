using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Calculador()
        {
            resultadosOriginal = new List<double>();
            resultadosImagenDalt = new List<double>();
        }

        public double deltaE(Color c1, Color c2)
        {
            LabColor lab1=RGBToLab(c1.R, c1.G, c1.B);
            LabColor lab2=RGBToLab(c2.R, c2.G, c2.B);

            double deltaL = lab1.L - lab2.L;
            double deltaA = lab1.A - lab2.A;
            double deltaB = lab1.B - lab2.B;

            return Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
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

        public void gereraResults(ref Bitmap original,ref Bitmap imDalt, int tamMatriz)
        {
            int radius = tamMatriz / 2;
            for (int i = 0; i < original.Height; i++)
            {
                for (int j = 0; j < original.Width; j++)
                {
                    
                    Color origen1 = original.GetPixel(j, i);
                    Color dalt1= imDalt.GetPixel(j, i);
                    double sumDeltaE = 0;
                    double sumDeltaEDalt = 0;
                    int cont = 0;
                    for(int k=i-radius; k<=i+radius; k++)
                    {
                        for(int l = j - radius; l <= j + radius; l++)
                        {
                            if(k==i && l == j||k<0||k>=original.Height||l<0||l>=original.Width)
                            {
                                continue;
                            }
                            sumDeltaE += deltaE(origen1, original.GetPixel(l,k));
                            sumDeltaEDalt += deltaE(dalt1, imDalt.GetPixel(l,k));
                            cont++;
                        }
                    }
                    if (cont > 0)
                    {
                        resultadosOriginal.Add(sumDeltaE / cont);
                        resultadosImagenDalt.Add(sumDeltaEDalt / cont);
                    }

                }
            }
            generateHeatMap(ref original,original.Width,original.Height,2.3);
        }
        private void generateHeatMap(ref Bitmap original, int width, int height, double umbral)
        {
            Bitmap baseImg = original;
            Bitmap mapa = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(mapa))
            {
                g.DrawImage(baseImg, 0, 0); // Dibuja la imagen base
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    double deltaEOri = resultadosOriginal[index];
                    double deltaEDalt = resultadosImagenDalt[index];

                    // Si la diferencia es pequeña, probablemente se vea igual para una persona daltónica
                    if (deltaEOri>=umbral && deltaEDalt< umbral)
                    {
                        // Pixel visible claramente
                        Color rojoTransparente = Color.FromArgb(255, 255, 0, 0);
                        mapa.SetPixel(x, y, rojoTransparente);
                        

                    }
                    else
                    {
                        // Píxel poco visible, lo dejamos transparente 
                        mapa.SetPixel(x, y, Color.Transparent);

                    }
                }
            }

            mapa.Save("HeatMap.png", ImageFormat.Png);
        }
    }
}
