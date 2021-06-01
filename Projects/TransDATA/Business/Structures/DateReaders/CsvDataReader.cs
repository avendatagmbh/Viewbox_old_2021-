using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Interfaces.DbStructure;
using System.IO;
using Business.Interfaces;

namespace Business.Structures.DateReaders {
    internal class CsvDataReader : IDataReader {

        internal CsvDataReader(string folder, IFile file) {
            Reader = new StreamReader(folder + "\\" + file.Name);
            File = file;
        }

        private bool IsCancelled { get; set; }
        private IFile File { get; set; }
        private StreamReader Reader { get; set; }
        public int FieldCount { get { throw new NotImplementedException(); } }

        public void Load(bool loadAllColumns) {
            throw new NotImplementedException();
        }

        public void Cancel() {
            IsCancelled = true;
        }

        public bool Read() {
            if (!IsCancelled && Reader != null || Reader.EndOfStream) return false;
            
            //string line = Reader.ReadLine();
            return true;
        }

        public object[] GetData() {
            //var x = "col1<DIV>col2";
            //var y = x.Split("<DIV>".ToArray(), StringSplitOptions.None);
            return null;
           
        }

        public void Dispose() {
            if (Reader != null) Reader.Close();
           
        }

        public void Close() {
            Reader.Close();   
        }
    }
}
