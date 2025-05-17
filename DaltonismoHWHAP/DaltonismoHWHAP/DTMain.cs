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
        public static void SetColorBlindnessComputeShaders()
        {
            instance.filtersComputeShader = Resources.Load<ComputeShader>("FiltrosDaltonismo");
            if (instance.filtersComputeShader == null) Debug.Log("Error al cargar el compute shader");
        }

        public static void captureScreen(byte[] data, int length, Dictionary<string, bool> filtros, int index)
        {

            //main

            // Crear un Bitmap completamente cargado desde los datos del stream
            Bitmap bmp;
            using (MemoryStream ms = new MemoryStream(data, 0, length))
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
                instance.filtroDaltonismo.SimularFiltro(bmpAuxProtanopia, FiltroDaltonismo.Filtros.Protanopia);
                bmpAuxProtanopia.Save("testImageColorblindProtanopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxProtanopia, 3, "Protanopia", index);
                bmpAuxProtanopia.Dispose();

            }

            if (filtros["Deuteranopia"])
            {
                Bitmap bmpAuxDeuteranopia = (Bitmap)bmp.Clone();
                instance.filtroDaltonismo.SimularFiltro(bmpAuxDeuteranopia, FiltroDaltonismo.Filtros.Deuteranopia);
                bmpAuxDeuteranopia.Save("testImageColorblindDeuteranopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxDeuteranopia, 3, "Deuteranopia", index);
                bmpAuxDeuteranopia.Dispose();

            }

            if (filtros["Tritanopia"])
            {
                Bitmap bmpAuxTritanopia = (Bitmap)bmp.Clone();
                instance.filtroDaltonismo.SimularFiltro(bmpAuxTritanopia, FiltroDaltonismo.Filtros.Tritanopia);
                bmpAuxTritanopia.Save("testImageColorblindTritanopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxTritanopia, 3, "Tritanopia", index);
                bmpAuxTritanopia.Dispose();

            }

            if (filtros["Acromatopia"])
            {
                Bitmap bmpAuxAcromatopia = (Bitmap)bmp.Clone();
                instance.filtroDaltonismo.SimularFiltro(bmpAuxAcromatopia, FiltroDaltonismo.Filtros.Acromatopia);
                bmpAuxAcromatopia.Save("testImageColorblindAcromatopia.png", ImageFormat.Png);
                instance.calculador.generaResults(ref bmpAux, ref bmpAuxAcromatopia, 3, "Acromatopia", index);
                bmpAuxAcromatopia.Dispose();

            }


            bmp.Dispose();
            bmpAux.Dispose();
        }
        public static void ProcessImageOnGPU(RenderTexture sourceImage, int type)
        {
            if (instance.filtersComputeShader == null) return;
            int kernelHandle = instance.filtersComputeShader.FindKernel("CSFiltrosDaltonismo");

            RenderTexture resultTexture = new RenderTexture(sourceImage.width, sourceImage.height, 0);
            resultTexture.enableRandomWrite = true;
            resultTexture.Create();

            instance.filtersComputeShader.SetTexture(kernelHandle, "Source", sourceImage);
            instance.filtersComputeShader.SetTexture(kernelHandle, "Result", resultTexture);
            instance.filtersComputeShader.SetInts("SourceTextureSize", sourceImage.width, sourceImage.height);
            instance.filtersComputeShader.SetInt("type", type);

            int threadGroupsX = Mathf.CeilToInt(sourceImage.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(sourceImage.height / 8.0f);

            instance.filtersComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

            Texture2D tex = new Texture2D(resultTexture.width, resultTexture.height, TextureFormat.RGB24, false);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = resultTexture;

            tex.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0);
            tex.Apply();

            RenderTexture.active = previous;

            byte[] pngData = tex.EncodeToPNG();
            Bitmap bmp;
            using (MemoryStream ms = new MemoryStream(pngData, 0, pngData.Length))
            {
                using (Bitmap temp = new Bitmap(ms))
                {
                    bmp = new Bitmap(temp); // Copia profunda e independiente del MemoryStream
                }
            }

            bmp.Save("testImageGPU" + type + ".png", ImageFormat.Png);
            bmp.Dispose();

            UnityEngine.Object.Destroy(tex);
            resultTexture.Release();
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
