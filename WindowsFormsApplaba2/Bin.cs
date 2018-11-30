using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplaba2
{
    public class Bin
    {
        public static float[,,] array;
        public Bin() { }

        public void readUNIT8(string path, int size)
        {
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
                array = new float[size, size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                        for (int k = 0; k < size; k++)
                            array[i, j, k] = reader.ReadSByte();
                }
            }

        }
        public void readFLOAT32(string path, int size)
        {
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
                array = new float[size, size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                        for (int k = 0; k < size; k++)
                            array[i, j, k] = reader.ReadSingle();
                }
            }

        }
    }
}
