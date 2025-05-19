# Proyecto  final UAJ

Repositorio del Proyecto final de la asignatura de Usabilidad y Análisis de Juegos del grupo 4. Consiste en una herramiente en forma de DLL para Unity para simular y detectar
zonas conflictivas en imágenes para diferentes tipos de daltonismo.

## Pasos a seguir para utilizar la herramienta
- Compilar la solución.
- Importar a los Assets de Unity el archivo DaltonismoHWHAP.dll, generado en la carpeta bin del proyecto. Si desea utilizar la aceleración por hardware, se debe importar también el archivo "FiltrosDaltonismo.compute", ubicado en la raíz del proyecto, a una carpeta "Resources" (crear en caso de que no exista) dentro de los Assets del proyecto de Unity.
- Desde Unity, inicializar la herramienta con el método estático DTMain.Init()
- Para cada imagen que se desee analizar, llamar a DTMain.GenerateImages. En caso de que se desee utilizar la GPU, se debe proporcionar mediante un parámetro adicional la imagen a analizar en formato RenderTexture.


