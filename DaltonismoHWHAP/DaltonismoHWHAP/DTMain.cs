using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;

namespace DaltonismoHWHAP
{
    public class DTMain
    {
        private static DTMain instance = null;
        private CapturadorPantalla capturadorPantalla;
        private FiltroDaltonismo filtroDaltonismo;
        private Calculador calculador;
        private SavedData savedData;
        private ComputeShader filtersComputeShader;
        private bool compshaderLoaded = false;

        private DTMain()
        {            
        }

        public static bool Init(float gravedad)
        {
            // No inicializamos si ya existe una instancia
            if (instance != null)
                return false;

            instance = new DTMain();

            instance.capturadorPantalla = new CapturadorPantalla();
            instance.filtroDaltonismo = new FiltroDaltonismo(gravedad);
            instance.calculador = new Calculador();
            instance.savedData = new SavedData();
           

            return true;
        }

        public static void GenerateImages(byte[] data, Dictionary<string, bool> filtros, int index, string folderName, RenderTexture sourceImage = null)
        {
            bool useGPU = false;
            if (sourceImage != null)
            {
                if (!instance.compshaderLoaded)
                {
                    instance.filtersComputeShader = Resources.Load<ComputeShader>("FiltrosDaltonismo");
                    if (instance.filtersComputeShader == null) Debug.Log("Error al cargar el compute shader");
                    else instance.compshaderLoaded = true;
                }
                useGPU = true;
                if (instance.filtersComputeShader == null) return;
            }

            // Crear un Bitmap completamente cargado desde los datos del stream
            Bitmap bmp;
            using (MemoryStream ms = new MemoryStream(data, 0, data.Length))
            {
                using (Bitmap temp = new Bitmap(ms))
                {
                    bmp = new Bitmap(temp); // Copia profunda e independiente del MemoryStream
                }
            }

            // Guardar la imagen original
            bmp.Save("original_from_unity.png", ImageFormat.Png);


            // Clonar para filtro
            Bitmap bmpAux = (Bitmap)bmp.Clone();
            bmpAux.Save("testImage.png", ImageFormat.Png);

            if (filtros["Protanopia"])
            {
                Bitmap bmpAuxProtanopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxProtanopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 0);
                else
                    instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanopia, FiltroDaltonismo.Filtros.Protanopia);

                bmpAuxProtanopia.Save("testImageColorblindProtanopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxProtanopia, 3, "Protanopia", index, folderName);
                bmpAuxProtanopia.Dispose();
            }

            if (filtros["Deuteranopia"])
            {
                Bitmap bmpAuxDeuteranopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxDeuteranopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 2);
                else
                    instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranopia, FiltroDaltonismo.Filtros.Deuteranopia);

                bmpAuxDeuteranopia.Save("testImageColorblindDeuteranopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxDeuteranopia, 3, "Deuteranopia", index, folderName);
                bmpAuxDeuteranopia.Dispose();
            }

            if (filtros["Tritanopia"])
            {
                Bitmap bmpAuxTritanopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxTritanopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 4);
                else
                    instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanopia, FiltroDaltonismo.Filtros.Tritanopia);

                bmpAuxTritanopia.Save("testImageColorblindTritanopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxTritanopia, 3, "Tritanopia", index, folderName);
                bmpAuxTritanopia.Dispose();
            }

            if (filtros["Acromatopia"])
            {
                Bitmap bmpAuxAcromatopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxAcromatopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 6);
                else
                    instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatopia, FiltroDaltonismo.Filtros.Acromatopia);

                bmpAuxAcromatopia.Save("testImageColorblindAcromatopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxAcromatopia, 3, "Acromatopia", index, folderName);
                bmpAuxAcromatopia.Dispose();
            }

            bmp.Dispose();
            bmpAux.Dispose();
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
        public static void AnalyzeImage(byte[] data, int length)
        {
            // Carga la imagen directamente desde los datos en memoria
            using (MemoryStream ms = new MemoryStream(data, 0, length))
            using (Bitmap bmp = new Bitmap(ms))
            {
                Console.WriteLine($"Imagen cargada: {bmp.Width}x{bmp.Height}");
            }
        }
    }
}
