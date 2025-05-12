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
        private SavedData savedData;
       
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
            instance.savedData = new SavedData();
           

            return true;
        }

        public static void captureScreen()
        {
            Bitmap bmp = instance.capturadorPantalla.captureScreen();
            bmp.Save("testImage.png", ImageFormat.Png);
            Bitmap bmpAux = (Bitmap)bmp.Clone();

           // Bitmap bmpAuxProtanopia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxProtanomalia = (Bitmap)bmp.Clone();
            Bitmap bmpAuxDeuteranopia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxDeuteranomalia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxTritanopia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxTritanomalia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxAcromatopia = (Bitmap)bmp.Clone();
            //Bitmap bmpAuxAcromatomalia = (Bitmap)bmp.Clone();

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanopia, FiltroDaltonismo.Filtros.Protanopia);
            //bmpAuxProtanopia.Save("testImageColorblindProtanopia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanomalia, FiltroDaltonismo.Filtros.Protanomalia);
            //bmpAuxProtanomalia.Save("testImageColorblindProtanomalia.png", ImageFormat.Png);

            instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranopia, FiltroDaltonismo.Filtros.Deuteranopia);
            bmpAuxDeuteranopia.Save("testImageColorblindDeuteranopia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranomalia, FiltroDaltonismo.Filtros.Deuteranomalia);
            //bmpAuxDeuteranomalia.Save("testImageColorblindDeuteranomalia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanopia, FiltroDaltonismo.Filtros.Tritanopia);
            //bmpAuxTritanopia.Save("testImageColorblindTritanopia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanomalia, FiltroDaltonismo.Filtros.Tritanomalia);
            //bmpAuxTritanomalia.Save("testImageColorblindTritanomalia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatopia, FiltroDaltonismo.Filtros.Acromatopia);
            //bmpAuxAcromatopia.Save("testImageColorblindAcromatopia.png", ImageFormat.Png);

            //instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatomalia, FiltroDaltonismo.Filtros.Acromatomalia);
            //bmpAuxAcromatomalia.Save("testImageColorblindAcromatomalia.png", ImageFormat.Png);

            //for (int i = 0; i < bmpAux.Height; i++)
            //{
            //    for(int j = 0; j < bmpAux.Width; j++)
            //    {
            //        instance.resultados.Add(instance.calculador.deltaE(bmpAux.GetPixel(j,i),bmp.GetPixel(j,i)));

            //    }
            //}
            instance.calculador.generaResults(ref bmpAux, ref bmpAuxDeuteranopia, 3);
            //instance.generateHeatMap(ref bmpAux,ref instance.resultados, bmpAux.Width, bmpAux.Height, 2.3);
            bmp.Dispose();
        }

        public static bool readFromFile()
        {
            return instance.savedData.readFromFile();
        }

        public static void writeToFile()
        {
            instance.savedData.writeToFile();
        }

        public static void addPos(float x, float y, float z)
        {
            instance.savedData.addToQueue(x, y, z);
        }
        public static void ClearList()
        {
            instance.savedData.clearQueue();
        }
        public static SavedData.PosGu returnValOfList(int val)
        {
            return instance.savedData.returnValueAt(val);
        }
        public static int listSize()
        {
            return instance.savedData.getListSize();
        }


    }
}
