using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Acrobat;

namespace Viewbox.Job
{
	public class ImageConverter
	{
		private readonly CAcroPDDoc pdfDoc = (AcroPDDoc)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("FF76CB60-2E68-101B-B02E-04021C009402")));

		private readonly CAcroRect pdfRect = (AcroRect)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("6D12C400-4E34-101B-9CA8-9240CE2738AE")));

		public int pageCount;

		private CAcroPDPage pdfPage;

		private AcroPoint pdfPoint = (AcroPoint)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("335E7240-6B49-101B-9CA8-9240CE2738AE")));

		private static bool ThumnailDelegate()
		{
			return false;
		}

		public static Image ConvertToThumbnail(string fileName)
		{
			Image retVal = null;
			using (FileStream fs = File.OpenRead(fileName))
			{
				retVal = ConvertToThumbnail(fs);
				fs.Close();
			}
			return retVal;
		}

		public static Image ConvertToThumbnail(Stream imgStream)
		{
			Image retVal = null;
			Stream retStream = new MemoryStream();
			Image.GetThumbnailImageAbort myThumnailDelegate = ThumnailDelegate;
			using (Image img = Image.FromStream(imgStream, useEmbeddedColorManagement: true, validateImageData: true))
			{
				img.Save(retStream, ImageFormat.Jpeg);
				retStream.Flush();
			}
			retVal = Image.FromStream(retStream, useEmbeddedColorManagement: true, validateImageData: true);
			return retVal.GetThumbnailImage(retVal.Width, retVal.Height, myThumnailDelegate, IntPtr.Zero);
		}

		public static string[] ConvertTiffToJpeg(string fileName)
		{
			using Image imageFile = Image.FromFile(fileName);
			FrameDimension frameDimensions = new FrameDimension(imageFile.FrameDimensionsList[0]);
			int frameNum = imageFile.GetFrameCount(frameDimensions);
			string[] jpegPaths = new string[frameNum];
			for (int frame = 0; frame < frameNum; frame++)
			{
				imageFile.SelectActiveFrame(frameDimensions, frame);
				using Bitmap bmp = new Bitmap(imageFile);
				jpegPaths[frame] = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}{frame}.jpg";
				bmp.Save(jpegPaths[frame], ImageFormat.Jpeg);
			}
			return jpegPaths;
		}

		public int Convert(string sourceFileName, string DestinationPath, ImageFormat outPutImageFormat)
		{
			if (pdfDoc.Open(sourceFileName))
			{
				pageCount = pdfDoc.GetNumPages();
				for (int i = 0; i < pageCount; i++)
				{
					pdfPage = (CAcroPDPage)(dynamic)pdfDoc.AcquirePage(i);
					pdfPoint = (AcroPoint)(dynamic)pdfPage.GetSize();
					pdfRect.Left = 0;
					pdfRect.right = pdfPoint.x;
					pdfRect.Top = 0;
					pdfRect.bottom = pdfPoint.y;
					pdfPage.CopyToClipboard(pdfRect, 500, 110, 100);
					string outimg = "";
					string filename = sourceFileName.Substring(sourceFileName.LastIndexOf("\\"));
					outimg = ((pageCount != 1) ? (DestinationPath + "\\" + filename + "_" + i.ToString() + "." + outPutImageFormat) : (DestinationPath + "\\" + filename + "." + outPutImageFormat));
					Clipboard.GetImage().Save(outimg, outPutImageFormat);
				}
				return pageCount;
			}
			throw new FileNotFoundException(sourceFileName + " Not Found!");
		}
	}
}
