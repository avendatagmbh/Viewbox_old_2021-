using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Model;
using EasyDocExtraction.Converter;
using EasyDocExtraction.DataAccess;

namespace EasyDocExtraction.Factory
{
    public class DocumentTypeFactory
    {
        static List<DocumentType> _documentTypes;
   
        private DocumentTypeFactory() { 
            
        }

        public static DocumentType GetDocumentTypeFromPath(string documentPath) 
        {
            if (_documentTypes == null)
            {
                using (var dbContext = new EasyArchiveRepository())
                {
                    _documentTypes = dbContext.DocumentTypes.ToList();
                }
            }
            DocumentType docType = _documentTypes.Find(d => DocumentTypeConverter.GetNameFromPath(documentPath) == d.Name.ToLower());
            if (docType == null)
            {
                docType = DocumentTypeConverter.GetDocumentTypeDocumentPath(documentPath);

                using(var dbContext =  new EasyArchiveRepository())
                {
                    dbContext.DocumentTypes.Add(docType);
                    dbContext.SaveChanges();
                    _documentTypes = dbContext.DocumentTypes.ToList();
                }
            }
            return docType;
        }
    }
}
