using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ViewboxArchiveConverter
{
    public class ImageConverter
    {

        public int pageCount = 0;
        Acrobat.CAcroPDDoc pdfDoc = new Acrobat.AcroPDDoc();
        Acrobat.CAcroPDPage pdfPage = null;
        Acrobat.CAcroRect pdfRect = new Acrobat.AcroRect();
        Acrobat.AcroPoint pdfPoint = new Acrobat.AcroPoint();

        internal static bool ThumnailDelegate() { return false; }

        /// <summary>  
        /// Converts the specified image into a JPEG format  
        /// </summary>  
        /// <param name="fileName">The file path of the image to convert</param>  
        /// <returns>An Image with JPEG data if successful; otherwise null</returns>  
        public static Image ConvertToThumbnail(string fileName, int width, int height)
        {
            Image retVal = null;
            using (FileStream fs = File.OpenRead(fileName))
            {
                retVal = ConvertToThumbnail(fs, width, height);
                fs.Close();
            }

            return retVal;
        }

        /// <summary>  
        /// Converts the specified image into a JPEG format  
        /// </summary>  
        /// <param name="imgStream">The stream of the image to convert</param>  
        /// <returns>An Image with JPEG data if successful; otherwise null</returns>  
        public static Image ConvertToThumbnail(Stream imgStream, int width, int height)
        {
            Image retVal = null;
            Stream retStream = new MemoryStream();
            Image.GetThumbnailImageAbort myThumnailDelegate = new Image.GetThumbnailImageAbort(ThumnailDelegate);
            using (Image img = Image.FromStream(imgStream, true, true))
            {
                img.Save(retStream, ImageFormat.Jpeg);
                retStream.Flush();
            }
            retVal = Image.FromStream(retStream, true, true);
            //the files we have have to be resized to work properly
            retVal = retVal.GetThumbnailImage(width, height, myThumnailDelegate, IntPtr.Zero);
            return retVal;
        }

        public static string[] ConvertTiffToJpeg(string fileName)
        {
            using (Image imageFile = Image.FromFile(fileName))
            {
                FrameDimension frameDimensions = new FrameDimension(
                    imageFile.FrameDimensionsList[0]);

                // Gets the number of pages from the tiff image (if multipage) 
                int frameNum = imageFile.GetFrameCount(frameDimensions);
                string[] jpegPaths = new string[frameNum];

                for (int frame = 0; frame < frameNum; frame++)
                {
                    // Selects one frame at a time and save as jpeg. 
                    imageFile.SelectActiveFrame(frameDimensions, frame);
                    using (Bitmap bmp = new Bitmap(imageFile))
                    {
                        jpegPaths[frame] = String.Format("{0}\\{1}{2}.jpg",
                            Path.GetDirectoryName(fileName),
                            Path.GetFileNameWithoutExtension(fileName),
                            frame);
                        bmp.Save(jpegPaths[frame], ImageFormat.Jpeg);
                    }
                }

                return jpegPaths;
            }
        }

        #region Convert
        /// 
        /// Converting PDF Files TO Specified Image Format
        /// 
        /// sourceFileName : Source PDF File Path
        /// DestinationPath : Destination PDF File Path
        /// outPutImageFormat: Type Of Exported Image
        /// Returns Count Of Exported Images
        public int Convert(string sourceFileName, string DestinationPath, ImageFormat outPutImageFormat)
        {
            if (pdfDoc.Open(sourceFileName))
            {
                // pdfapp.Hide();
                pageCount = pdfDoc.GetNumPages();

                for (int i = 0; i < pageCount; i++)
                {
                    pdfPage = (Acrobat.CAcroPDPage)pdfDoc.AcquirePage(i);

                    pdfPoint = (Acrobat.AcroPoint)pdfPage.GetSize();
                    pdfRect.Left = 0;
                    pdfRect.right = pdfPoint.x;
                    pdfRect.Top = 0;
                    pdfRect.bottom = pdfPoint.y;

                    pdfPage.CopyToClipboard(pdfRect, 500, 110, 100);

                    string outimg = "";
                    string filename = sourceFileName.Substring(
                               sourceFileName.LastIndexOf("\\"));

                    if (pageCount == 1)
                        outimg = DestinationPath + "\\" + filename +
                          "." + outPutImageFormat.ToString();
                    else
                        outimg = DestinationPath + "\\" + filename +
                          "_" + i.ToString() + "." + outPutImageFormat.ToString();

                    Clipboard.GetImage().Save(outimg, outPutImageFormat);

                }

                //Dispose();
            }
            else
            {
                //Dispose();
                throw new System.IO.FileNotFoundException(sourceFileName + " Not Found!");
            }
            return pageCount;
        }
        #endregion
    } 
}
