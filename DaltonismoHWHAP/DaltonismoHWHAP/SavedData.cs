using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DaltonismoHWHAP {
    public class SavedData {
        public struct Posicion
        {
            public float _x;
            public float _y;
            public float _z;
        }
        private Queue<Posicion> lista;

        const string filename = "positionList.txt";

        public SavedData()
        {
            lista = new Queue<Posicion>();
        }
        public void AddToQueue(float x,float y,float z)
        {
            Posicion p;
            p._x = x;
            p._y = y;
            p._z = z;
            lista.Enqueue(p); //Si queremos hacerlo directamente es necesario System.Collections.IEnumerable 
        }

        public void ClearQueue()
        {
            lista.Clear();
        }

        // Intentamos leer de un archivo las posiciones que se quieren analizar para tenerlas guardadas. Devolvemos false si no se ha encontrado el archivo.
        public bool ReadFromFile()
        {
            try
            {
                StreamReader sr = new StreamReader(filename);

                string coord;

                coord = sr.ReadLine();
                if (coord == null)
                {
                    return false; //Si está vacío devuelve false
                }
                float nx, ny, nz;
                while (coord != null)
                {

                    if (coord == "newpos:")
                    {
                        nx = float.Parse(sr.ReadLine());
                        ny = float.Parse(sr.ReadLine());
                        nz = float.Parse(sr.ReadLine());
                        AddToQueue(nx, ny, nz);
                    }

                    //Read the next line
                    coord = sr.ReadLine();
                }
                sr.Close();

                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }            
        }

        // Intentamos escribir en un archivo las posiciones que se quieren analizar para tenerlas guardadas. Devolvemos false si no se ha encontrado el archivo.
        public bool WriteToFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(filename);
                if (lista.Count > 0)
                {
                    Posicion posToSave = lista.ElementAt(0);
                    int i = 0;
                    while (i + 1 < lista.Count)
                    {
                        sw.WriteLine("newpos:");
                        sw.WriteLine(posToSave._x);
                        sw.WriteLine(posToSave._y);
                        sw.WriteLine(posToSave._z);

                        posToSave = lista.ElementAt(i + 1);
                        i++;
                    }

                    //Last position
                    sw.WriteLine("newpos:");
                    sw.WriteLine(posToSave._x);
                    sw.WriteLine(posToSave._y);
                    sw.WriteLine(posToSave._z);

                }
                else
                {
                    sw.WriteLine("empty");
                }

                sw.Close();

                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public Posicion ReturnValueAt(int elem)
        {
            if (elem < lista.Count && elem >= 0)
            {
                return lista.ElementAt(elem);
            }
            else
            {
                return lista.First();
            }
        }

        public int GetListSize()
        {
            return lista.Count;
        }

        
        
    }
}
