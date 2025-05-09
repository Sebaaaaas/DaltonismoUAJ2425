using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DaltonismoHWHAP {
    internal class SavedData {
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

        public void readFromFile()
        {
            StreamReader sr = new StreamReader("/Assets/PosList.txt");
            
            if (sr != null) { 

                string coord;

                coord = sr.ReadLine();
                float nx, ny, nz;
                while (coord != null)
                {
                    int i = 0;
                    string x, y, z;
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
            
        }

        public void writeToFile()
        {
            StreamWriter sw = new StreamWriter("/Assets/PosList.txt");
            PosGu posToSave=lista.First();
            Queue<PosGu> listarec = lista;
            while ((posToSave._x != lista.Last()._x)&& (posToSave._y != lista.Last()._y)
                && (posToSave._z != lista.Last()._z))
            {
                sw.WriteLine("newpos:");
                sw.WriteLine(posToSave._x);
                sw.WriteLine(posToSave._y);
                sw.WriteLine(posToSave._z);
                listarec.Dequeue();
                posToSave = listarec.First();
            }
            
            //Last position
            sw.WriteLine("newpos:");
            sw.WriteLine(posToSave._x);
            sw.WriteLine(posToSave._y);
            sw.WriteLine(posToSave._z);

            sw.Close();
        }

        
        
    }
}
