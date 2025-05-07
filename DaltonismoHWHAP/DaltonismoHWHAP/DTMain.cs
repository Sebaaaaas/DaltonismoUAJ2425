using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace DaltonismoHWHAP
{
    public class DTMain
    {
        private static DTMain instance = null;
        private CapturadorPantalla capturadorPantalla;
        private FiltroDaltonismo filtroDaltonismo;
        private Calculador calculador;
        List<double> resultados;
        private DTMain()
        {            
        }

        public static bool Init()
        {
            // No inicializamos si ya existe una instancia
            if (instance != null)
                return false;

            instance = new DTMain();

            instance.capturadorPantalla = new CapturadorPantalla();
            instance.filtroDaltonismo = new FiltroDaltonismo();
            instance.calculador = new Calculador();
            instance.resultados= new List<double>();

            return true;
        }

        public static void captureScreen()
        {
            Bitmap bmp = instance.capturadorPantalla.captureScreen();
            bmp.Save("testImage.png", ImageFormat.Png);
            Bitmap bmpAux = (Bitmap)bmp.Clone();

            Bitmap bmpAuxProtanopia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxProtanomalia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxDeuteranopia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxDeuteranomalia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxTritanopia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxTritanomalia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxAcromatopia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxAcromatomalia = (Bitmap)bmp.Clone();

            instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanopia, FiltroDaltonismo.Filtros.Protanopia);
            bmpAuxProtanopia.Save("testImageColorblindProtanopia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanomalia, FiltroDaltonismo.Filtros.Protanomalia);
            bmpAuxProtanomalia.Save("testImageColorblindProtanomalia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranopia, FiltroDaltonismo.Filtros.Deuteranopia);
            bmpAuxDeuteranopia.Save("testImageColorblindDeuteranopia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranomalia, FiltroDaltonismo.Filtros.Deuteranomalia);
            bmpAuxDeuteranomalia.Save("testImageColorblindDeuteranomalia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanopia, FiltroDaltonismo.Filtros.Tritanopia);
            bmpAuxTritanopia.Save("testImageColorblindTritanopia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanomalia, FiltroDaltonismo.Filtros.Tritanomalia);
            bmpAuxTritanomalia.Save("testImageColorblindTritanomalia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatopia, FiltroDaltonismo.Filtros.Acromatopia);
            bmpAuxAcromatopia.Save("testImageColorblindAcromatopia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatomalia, FiltroDaltonismo.Filtros.Acromatomalia);
            bmpAuxAcromatomalia.Save("testImageColorblindAcromatomalia.png", ImageFormat.Png);

            for (int i = 0; i < bmpAux.Height; i++)
            {
                for(int j = 0; j < bmpAux.Width; j++)
                {
                    instance.resultados.Add(instance.calculador.deltaE(bmpAux.GetPixel(j,i),bmp.GetPixel(j,i)));

                }
            }
            instance.generateHeatMap(ref bmpAux,ref instance.resultados, bmpAux.Width, bmpAux.Height, 2.3);
            bmp.Dispose();
        }
        private void generateHeatMap(ref Bitmap bmp,ref List<double> deltaE, int width, int height, double umbral)
        {
            Bitmap mapa = (Bitmap)bmp.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    double dE = deltaE[index];

                    // Si la diferencia es pequeña, probablemente se vea igual para una persona daltónica
                    if (dE < umbral)
                    {
                        // Píxel poco visible, lo dejamos transparente 
                        mapa.SetPixel(x, y, Color.Transparent); 

                    }
                    else
                    {
                        // Pixel visible claramente
                        Color rojoTransparente = Color.FromArgb(128, 255, 0, 0);
                        mapa.SetPixel(x, y, rojoTransparente);
                        
                    }
                }
            }

            mapa.Save("HeatMap.png",ImageFormat.Png);
        }

    }
}
