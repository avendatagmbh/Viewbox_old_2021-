using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ViewboxArchiveConverter
{
    internal class TextToThumbnailConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            try
            {
                String newFileName = GetThumbnailFullName(fileConvertOptions);

                if (fileConvertOptions.ContinueProcessing)
                {
                    if (File.Exists(newFileName)) return true;
                }

                //throw new NotImplementedException();

                string text = string.Empty;
                string archiveFileName = fileConvertOptions.ArchiveFileName;
                if (fileConvertOptions.ArchiveFileName.Contains(".md"))
                    archiveFileName = fileConvertOptions.ArchiveFileName.Replace(".md", "");

                string oldFileName = Path.Combine(fileConvertOptions.CurrentDirectory, archiveFileName);
                using (StreamReader sr = new StreamReader(oldFileName, Encoding.Default))
                {
                    text = sr.ReadToEnd();
                    sr.Close();
                }

                Image img = ConvertTextToImage(fileConvertOptions, text);
                if (img != null)
                {
                    img = img.GetThumbnailImage(fileConvertOptions.TumbnailWidth, fileConvertOptions.TumbnailHeight, ImageConverter.ThumnailDelegate, IntPtr.Zero);
                    img.Save(newFileName, ImageFormat.Jpeg);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Image ConvertTextToImage(FileConvertOptions fileConvertOptions, string text)
        {
            Bitmap objBmpImage = new Bitmap(fileConvertOptions.TumbnailWidth, fileConvertOptions.TumbnailHeight);
  
            Font objFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel);
  
            Graphics objGraphics = Graphics.FromImage(objBmpImage);
            objBmpImage = new Bitmap(objBmpImage);
            objGraphics = Graphics.FromImage(objBmpImage);
            objGraphics.Clear(Color.White);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.DrawString(text, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), 0, 0);
            objGraphics.Flush();
  
            return (objBmpImage);
        }
    }
}
