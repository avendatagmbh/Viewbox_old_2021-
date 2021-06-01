using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConvertUnixToWin {
    class Program {
        
        static void Main(string[] args) {

            BinaryReader reader = new BinaryReader(new FileStream(@"C:\Users\mid.AV\Documents\DartTest\CD3\in\DD_BK_VD_01_16_2007.txt", FileMode.Open));
            BinaryWriter writer = new BinaryWriter(new FileStream(@"C:\Users\mid.AV\Documents\DartTest\CD3\in\DD_BK_VD_01_16_2007_2.txt", FileMode.Create));
            
            try {
                while (true) {
                    Byte b = reader.ReadByte();
                    if (b.Equals((int)'\n')) {
                        writer.Write((Byte)13);
                        writer.Write((Byte)10);
                    } else writer.Write(b);
                }
            } catch (Exception ex) {
                writer.Close();
            }
        }
    }
}
