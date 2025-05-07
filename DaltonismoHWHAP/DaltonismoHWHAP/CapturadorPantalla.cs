using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace DaltonismoHWHAP
{
    public class CapturadorPantalla
    {
        public CapturadorPantalla()
        {
        }

        // Importante llamar posteriormente a Dispose del bitmap y de graphics
        public Bitmap captureScreen()
        {

            Bitmap bmp = new Bitmap(1920, 1080);

            Graphics g = Graphics.FromImage(bmp);
                
            g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            
            g.Dispose();

            return bmp;
        }

    }

    
}
