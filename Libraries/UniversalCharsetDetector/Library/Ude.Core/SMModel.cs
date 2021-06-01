using System;

namespace Ude.Core
{
    /// <summary>
    ///   State machine model
    /// </summary>
    public abstract class SMModel
    {
        public const int START = 0;
        public const int ERROR = 1;
        public const int ITSME = 2;
        private readonly int classFactor;
        private readonly string name;
        public int[] charLenTable;
        public BitPackage classTable;
        public BitPackage stateTable;

        public SMModel(BitPackage classTable, int classFactor,
                       BitPackage stateTable, int[] charLenTable, String name)
        {
            this.classTable = classTable;
            this.classFactor = classFactor;
            this.stateTable = stateTable;
            this.charLenTable = charLenTable;
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public int ClassFactor
        {
            get { return classFactor; }
        }

        public int GetClass(byte b)
        {
            return classTable.Unpack(b);
        }
    }
}