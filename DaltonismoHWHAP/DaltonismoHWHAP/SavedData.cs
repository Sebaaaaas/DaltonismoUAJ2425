using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DaltonismoHWHAP {
    public class SavedData {
        public struct PosGu
        {
            public float _x;
            public float _y;
            public float _z;
        }
        private Queue<PosGu> lista;
        

        public SavedData()
        {
            lista = new Queue<PosGu>();
        }
        public void addToQueue(float x,float y,float z)
        {
            PosGu p;
            p._x = x;
            p._y = y;
            p._z = z;
            lista.Enqueue(p); //Si queremos hacerlo directamente es necesario System.Collections.IEnumerable 
        }

        public void clearQueue()
        {
            lista.Clear();
        }

        public bool readFromFile()
        {
            StreamReader sr = new StreamReader("PosList.txt");
            
            if (sr != null) { 

                string coord;

                coord = sr.ReadLine();
                if(coord == null)
                {
                    return false; //Si está vacío devuelve false
                }
                float nx, ny, nz;
                while (coord != null)
                {
                    
                    if (coord == "newpos:")
                    {
                        nx=float.Parse(sr.ReadLine());
                        ny=float.Parse(sr.ReadLine());
                        nz=float.Parse(sr.ReadLine());
                        addToQueue(nx,ny,nz);
                    }
                    
                    //Read the next line
                    coord = sr.ReadLine();
                }
                sr.Close();
            }
            else
            {
                return false; //Si no existe devuelve false
            }
            return true;
            
        }

        public void writeToFile()
        {
            StreamWriter sw = new StreamWriter("PosList.txt");
            if (lista.Count > 0)
            {
                PosGu posToSave = lista.ElementAt(0);
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
        }

        public PosGu returnValueAt(int elem)
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

        public int getListSize()
        {
            return lista.Count;
        }

        
        
    }
}
