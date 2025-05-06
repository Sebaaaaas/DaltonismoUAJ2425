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
        Graphics graphics;

        public CapturadorPantalla()
        {

        }
        public void captureScreen()
        {
            using (Bitmap bmp = new Bitmap(800, 600))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);

                }

                bmp.Save("testImage.png", ImageFormat.Png);
            }

            
        }
    }

    
}
