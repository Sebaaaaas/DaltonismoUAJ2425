using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaltonismoHWHAP
{
    public class DTMain
    {
        private static DTMain instance = null;
        private CapturadorPantalla capturadorPantalla;
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

            return true;
        }

        public static void captureScreen()
        {
            instance.capturadorPantalla.captureScreen();
        }

    }
}
