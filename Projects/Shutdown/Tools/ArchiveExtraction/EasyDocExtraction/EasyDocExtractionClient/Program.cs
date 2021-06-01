using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using EasyDocExtraction.Helper;
using EasyDocExtraction.DataAccess;
using EasyDocExtraction.Model;
using System.Data.Entity;
using System.IO;
using EasyDocExtraction;

namespace EasyDocExtractionClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Logger.Instance = log4net.LogManager.GetLogger(typeof(Program));
            // EF codefirst instruction
#if DEBUG
            Database.SetInitializer<EasyArchiveRepository>(new DropCreateDatabaseIfModelChanges<EasyArchiveRepository>());
#else
            //Database.SetInitializer<EasyArchiveRepository>(new DropCreateDatabaseIfModelChanges<EasyArchiveRepository>());
#endif

            // loops in the folder collection in the config file (easySection element)
            // foreach folder, extract, convert and save to DB
            foreach (var folderOrFile in ConfigurationHelper.GetEasyFoldersPath())
            {
                // gets the files in the folder if we have a folder otherwise suspects a valid file
                string[] files = Directory.Exists(folderOrFile) ? Directory.GetFiles(folderOrFile) : new string[] { folderOrFile };

                // for each DB file creates .net object from raw easy data
                foreach (var file in files)
                {
                    // creates a log file with rolling appenders for each db file in folder
                    var logFileName = folderOrFile.Split('\\').Last() + "_" + Path.GetFileNameWithoutExtension(file);
                    Logger.LoggingConfigurer.ConfigureLogging(Logger.Instance, logFileName);
                    Logger.LoggingConfigurer.RenameLogFile(logFileName);

                    Logger.Write("RETRIEVING ARCHIVE ... (file: {0})", logFileName);

                    EasyExtractionMain extractor = new EasyDocExtraction.EasyExtractionMain(file);

                    extractor.SaveByChunkAction = (archives) => 
                    {
                        Logger.Write("SAVING CHUNK OF {0} ARCHIVES FOR {1}. Please wait... ", archives.Count, logFileName);
                        EasyArchiveRepository.SaveAll(archives);
                        Logger.Write("SAVE SUCCEEDED. {0} archived saved. (file: {1})", archives.Count, logFileName);
                    };
                    extractor.GetArchives();

                    //var archives =new EasyDocExtraction.EasyExtractionMain(file).GetArchives();
                    //EasyArchiveRepository.SaveByChunk(archives);
                    //EasyArchiveRepository.SaveAll(archives);

                    Logger.Write(new string('-', 30));

                }

                Logger.Write("PROCESSING FOLDER {0} FINISHED", folderOrFile);
                Logger.Write(new string('-', 30));

            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteError("ERROR: UnhandledException occured, process will be stopped", (Exception)e.ExceptionObject);
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

        /// <summary>
        /// test displays the field name + the value of the repository
        /// </summary>
        static void Test() {

            var archivesTest = new EasyArchiveRepository().ArchivedItems.ToList();

            foreach (var a in archivesTest)
            {
                Console.Write(a.EasyFolder.Name + ", ID : " + a.ArchivedItemId);
                a.FieldValues.ToList().ForEach(fv => Console.WriteLine("\t\t" + fv.FieldDefinition.FieldName + ": " + fv.Value));

            }
            Console.ReadLine();
        }
    }
}
