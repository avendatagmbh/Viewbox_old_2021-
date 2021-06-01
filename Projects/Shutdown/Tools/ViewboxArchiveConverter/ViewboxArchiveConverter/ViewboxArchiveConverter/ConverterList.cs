using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ViewboxArchiveConverter
{
    internal class ConverterList : IFileConverter
    {
        public IFileConverter[] FileConverters { get; set; }

        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            Boolean b = true;
            foreach(IFileConverter converter in FileConverters) {
                if (converter != null)
                {
                    b = b && converter.Convert(fileConvertOptions);
                }
            }
            return b;
        }
    }
}
