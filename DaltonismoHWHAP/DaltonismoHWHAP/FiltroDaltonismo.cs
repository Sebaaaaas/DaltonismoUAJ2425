using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace DaltonismoHWHAP
{
    public class FiltroDaltonismo
    {
        public FiltroDaltonismo() { }

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
    }

}
