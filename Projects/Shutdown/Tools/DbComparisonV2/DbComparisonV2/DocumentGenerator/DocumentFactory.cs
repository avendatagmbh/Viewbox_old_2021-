using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBComparisonBusiness;
using DocumentGenerator;
using MSExcel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using DBComparisonBusiness.Business;

namespace DbComparisonV2.DocumentGenerator
{
    public class DocumentFactory
    {
        /// <summary>
        /// Factory method returning a instance of TDocType loaded with the comparison result.
        /// </summary>
        /// <typeparam name="TDocType"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IDocument GetFrom<TDocType, TIComparisonResult>(TIComparisonResult result) where TDocType : IDocument
        {
            // create an instance from the constructor with IComparisonResult argument
            return (TDocType)typeof(TDocType).GetConstructor(new[] { typeof(TIComparisonResult) }).Invoke(new[] { (object)result });
        }
    }
    public interface IDocument: IDisposable
    {
        void WriteToFile(string filename);
    }
}
