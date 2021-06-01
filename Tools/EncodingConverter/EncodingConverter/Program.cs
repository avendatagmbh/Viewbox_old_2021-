using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EncodingConverter {
    class Program {
        static void Main(string[] args) {
            //string path = @"Q:\Projekte\Kundenprojekte\Smith International\2009-2012\FMS Ergebnis";
            //string path = @"C:\Users\beh\Documents\FMS\2010";
            //string path = @"Q:\Projekte\Kundenprojekte\Smith International\2009-2012\FMS Ergebnis\2012";
            string path =
                @"Q:\Projekte\Kundenprojekte\Smith International\2009-2012\FMS Ergebnis\2012\Für IDEA";
            //List<string> files = new List<string>(){path + @"\2009\Für IDEA\einzeln\APR-Asset Status Value Report.csv"};
            List<string> files = new List<string>();
            WalkDirectoryTree(new DirectoryInfo(path), files, 0);

            files = (from file in files where file.ToLower().Contains("für idea") select file).ToList();

            foreach (var file in files) {
                using (StreamReader reader = new StreamReader(file, Encoding.UTF8)) {
                    string newFile = file.ToLower().Replace("für idea", "für idea_fertig\\test_bes");
                    if(newFile.ToLower() == file.ToLower()) throw new Exception("Overwriting file");

                    Directory.CreateDirectory(new FileInfo(newFile).DirectoryName);
                    using (var writer = new StreamWriter(newFile, false, Encoding.Default)) {
                        writer.Write(reader.ReadToEnd());
                    }
                }
            }
        }

        static void WalkDirectoryTree(System.IO.DirectoryInfo root, List<string> result, int level) {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            if (level == 1)
                Console.WriteLine(root.FullName);
            // First, process all the files directly under this folder
            try {
                files = root.GetFiles("*.csv");
            }
                // This is thrown if even one of the files requires permissions greater
                // than the application provides.
            catch (UnauthorizedAccessException e) {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                Console.WriteLine(e.Message);
            } catch (System.IO.DirectoryNotFoundException e) {
                Console.WriteLine(e.Message);
            }

            if (files != null) {
                foreach (System.IO.FileInfo fi in files) {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    //Console.WriteLine(fi.FullName);
                    //sqlFiles.Add(fi.FullName);
                    //writer.WriteLine(fi.FullName);
                    result.Add(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs) {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, result, level + 1);
                }
            }
        }

    }
}
