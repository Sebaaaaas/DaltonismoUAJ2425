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

            return true;
        }

        public static void captureScreen()
        {
            Bitmap bmp = instance.capturadorPantalla.captureScreen();
            bmp.Save("testImage.png", ImageFormat.Png);

            instance.filtroDaltonismo.filtroTest(bmp);
            bmp.Save("testImageColorblind.png", ImageFormat.Png);

            bmp.Dispose();
        }

    }
}
