using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;

namespace DaltonismoHWHAP
{
    public class DTMain
    {
        private static DTMain instance = null;
        private FiltroDaltonismo filtroDaltonismo;
        private Calculador calculador;
        private SavedData savedData;
        private ComputeShader filtersComputeShader;
        private bool compshaderLoaded = false;

        private DTMain()
        {            
        }

        /// <summary>
        /// Initializes the tool. Must be called before using this tool.
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            // No inicializamos si ya existe una instancia
            if (instance != null)
                return false;

            instance = new DTMain();

            instance.filtroDaltonismo = new FiltroDaltonismo();
            instance.calculador = new Calculador();
            instance.savedData = new SavedData();
           

            return true;
        }
        /// <summary>
        /// Generates the simulated images and their corresponding heatmaps for the types 
        /// of color blindness specified in the filters variable.
        /// </summary>
        /// <param name="data"> Array which contains the information of the PNG original image. </param>
        /// <param name="filters"> Dictionary that specifies which types of color blindness are to be analyzed.  </param>
        /// <param name="index"> Integer used to enumerate the different images to be analyzed. </param>
        /// <param name="folderName"> The path where images will be saved. </param>
        /// <param name="sourceImage"> Optional parameter to use hardware acceleration. Must include the image to be analyzed in RenderTexture format. </param>
        public static void GenerateImages(byte[] data, Dictionary<string, bool> filters, int index, string folderName, RenderTexture sourceImage = null)
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


            string folderPath = "Analisis_Daltonismo/" + folderName + "/Captura" + index;
            Directory.CreateDirectory(folderPath);
            
            // Guardar la imagen original
            string filePath = Path.Combine(folderPath, "original_from_unity.png");
            bmp.Save(filePath, ImageFormat.Png);

            // Clonar para filtro
            Bitmap bmpAux = (Bitmap)bmp.Clone();

            if (filters["Protanopia"])
            {
                Bitmap bmpAuxProtanopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxProtanopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 0);
                else
                    instance.filtroDaltonismo.SimulateFilter(bmpAuxProtanopia, FiltroDaltonismo.Filtros.Protanopia);

                filePath = Path.Combine(folderPath, "testImageColorblindProtanopia.png");
                bmpAuxProtanopia.Save(filePath, ImageFormat.Png);
                instance.calculador.GenerateResults(ref bmpAux, ref bmpAuxProtanopia, 3, "Protanopia", index, folderName);
                bmpAuxProtanopia.Dispose();
            }

            if (filters["Deuteranopia"])
            {
                Bitmap bmpAuxDeuteranopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxDeuteranopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 1);
                else
                    instance.filtroDaltonismo.SimulateFilter(bmpAuxDeuteranopia, FiltroDaltonismo.Filtros.Deuteranopia);

                filePath = Path.Combine(folderPath, "testImageColorblindDeuteranopia.png");
                bmpAuxDeuteranopia.Save(folderPath, ImageFormat.Png);
                instance.calculador.GenerateResults(ref bmpAux, ref bmpAuxDeuteranopia, 3, "Deuteranopia", index, folderName);
                bmpAuxDeuteranopia.Dispose();
            }

            if (filters["Tritanopia"])
            {
                Bitmap bmpAuxTritanopia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxTritanopia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 2);
                else
                    instance.filtroDaltonismo.SimulateFilter(bmpAuxTritanopia, FiltroDaltonismo.Filtros.Tritanopia);

                filePath = Path.Combine(folderPath, "testImageColorblindTritanopia.png");
                bmpAuxTritanopia.Save(folderPath, ImageFormat.Png);
                instance.calculador.GenerateResults(ref bmpAux, ref bmpAuxTritanopia, 3, "Tritanopia", index, folderName);
                bmpAuxTritanopia.Dispose();
            }

            if (filters["Acromatopsia"])
            {
                Bitmap bmpAuxAcromatopsia = (Bitmap)bmp.Clone();

                if (useGPU)
                    bmpAuxAcromatopsia = instance.filtroDaltonismo.SimulateFilterOnGPU(sourceImage, instance.filtersComputeShader, 3);
                else
                    instance.filtroDaltonismo.SimulateFilter(bmpAuxAcromatopsia, FiltroDaltonismo.Filtros.Acromatopsia);

                filePath = Path.Combine(folderPath, "testImageColorblindAcromatopsia.png");
                bmpAuxAcromatopsia.Save(folderPath, ImageFormat.Png);
                instance.calculador.GenerateResults(ref bmpAux, ref bmpAuxAcromatopsia, 3, "Acromatopsia", index, folderName);
                bmpAuxAcromatopsia.Dispose();
            }

            bmp.Dispose();
            bmpAux.Dispose();
        }

        public static bool ReadFromFile()
        {
            return instance.savedData.ReadFromFile();
        }

        public static void WriteToFile()
        {
            instance.savedData.WriteToFile();
        }

        public static void AddPos(float x, float y, float z)
        {
            instance.savedData.AddToQueue(x, y, z);
        }
        public static void ClearList()
        {
            instance.savedData.ClearQueue();
        }
        public static SavedData.Posicion ReturnValOfList(int val)
        {
            return instance.savedData.ReturnValueAt(val);
        }
        public static int ListSize()
        {
            return instance.savedData.GetListSize();
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
