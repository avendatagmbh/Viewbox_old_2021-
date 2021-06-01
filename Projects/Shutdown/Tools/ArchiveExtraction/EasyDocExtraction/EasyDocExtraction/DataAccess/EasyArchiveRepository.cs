using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using EasyDocExtraction.Model;
using System.Data.Entity.Validation;
using EasyDocExtraction.Helper;

namespace EasyDocExtraction.DataAccess
{
    public class EasyArchiveRepository:  DbContext
    {
        public DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public DbSet<FieldValue> FieldValues { get; set; }
        public DbSet<ArchivedItem> ArchivedItems { get; set; }
        public DbSet<ArchivedDocument> ArchivedDocuments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<EasyFolder> EasyFolders { get; set; }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            return -1;
        }
        public static void SaveByChunk(List<ArchivedItem> archives)
        {
            int chunkSize = ConfigurationHelper.GetDataChunckSize();
            int count = chunkSize;
            IEnumerable<ArchivedItem> subSet = archives.Take(chunkSize);
            while (subSet.Count() >0) 
            {
                Logger.Write("Saving by chunk of {0} archives.", chunkSize);
                SaveAll(subSet.ToList());
                Logger.Write("Saved chunk of {0} archives succeeded.", chunkSize);

                subSet = archives.Skip(count).Take(chunkSize);
                count += chunkSize;
            }
        }
        public static void SaveAll(List<ArchivedItem> allArchives)
        {

            using (var repository = new EasyArchiveRepository())
            {
                repository.Configuration.AutoDetectChangesEnabled = false; // this improves performances
                foreach(var archive in allArchives)
                {
                    repository.Set<ArchivedItem>().Add(archive);
                }

                repository.SaveChanges();
            }
        }
    }
}
