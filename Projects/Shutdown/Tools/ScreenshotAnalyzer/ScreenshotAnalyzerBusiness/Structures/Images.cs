// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageHelper;
using ScreenshotAnalyzerBusiness.Engine;
using Utils;
using Point = System.Windows.Point;

namespace ScreenshotAnalyzerBusiness.Structures {
    public static class Images {
        public static BitmapSource ToBitmapSource(this Image source) {
            Bitmap bitmap = new Bitmap(source);

            var bitSrc = bitmap.ToBitmapSource();

            bitmap.Dispose();
            bitmap = null;

            return bitSrc;
        }

        public static BitmapSource ToBitmapSource(this Bitmap source) {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            } catch (Win32Exception) {
                bitSrc = null;
            } finally {
                //NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }


        public static Bitmap ImageFromRectangle(Image image, OcrRectangle rect) {
            Bitmap newImage = new Bitmap((int)rect.Width, (int)rect.Height);
            Graphics graphics = Graphics.FromImage(newImage);
            graphics.DrawImage(image, new Rectangle(0, 0, (int)rect.Width, (int)rect.Height), new Rectangle((int)rect.UpperLeft.X, (int)rect.UpperLeft.Y, (int)rect.Width, (int)rect.Height), GraphicsUnit.Pixel);
            return newImage;
        }

        public static List<string> ExtractText(string path, IEnumerable<OcrRectangle> rectangles) {
            List<string> result = new List<string>();

            int Status;
            int jobNo = 0;
            string Msg = "";
            Bitmap BMP;
            TOCRdeclares.TOCRRESULTS Results = new TOCRdeclares.TOCRRESULTS();
            TOCRdeclares.TOCRJOBINFO JobInfo = new TOCRdeclares.TOCRJOBINFO();

            JobInfo.ProcessOptions.DisableCharacter = new short[256];

            IntPtr MMFhandle = IntPtr.Zero;

            BMP = new Bitmap(path);

            foreach (var rect in rectangles) {
                Bitmap newImage = ImageFromRectangle(BMP, rect);
                
                //MMFhandle = TOCRdeclares.ConvertBitmapToMMF(BMP);
                MMFhandle = TOCRdeclares.ConvertBitmapToMMF2(newImage, true, true);

                if (!(MMFhandle.Equals(IntPtr.Zero))) {
                    TOCRdeclares.TOCRSetConfig(TOCRdeclares.TOCRCONFIG_DEFAULTJOB, TOCRdeclares.TOCRCONFIG_DLL_ERRORMODE,
                                               TOCRdeclares.TOCRERRORMODE_MSGBOX);
                    JobInfo.InputFile = "";
                    JobInfo.JobType = TOCRdeclares.TOCRJOBTYPE_MMFILEHANDLE;
                    Status = TOCRdeclares.TOCRInitialise(ref jobNo);
                    if (Status == TOCRdeclares.TOCR_OK) {
                        JobInfo.PageNo = MMFhandle.ToInt32();
                        if (TOCRdeclares.OCRWait(jobNo, JobInfo)) {
                            if (TOCRdeclares.GetResults(jobNo, ref Results)) {
                                TOCRdeclares.FormatResults(Results, ref Msg);
                                    //TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
                                result.Add(Msg);
                            }
                        }

                        TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
                    }
                }
            }
            return result;
        }

        public static List<string> ExtractTextExperimental(string path, IEnumerable<OcrRectangle> rectangles) {
            List<string> result = new List<string>();

            int Status;
            int jobNo = 0;
            Bitmap BMP;
            TOCRdeclares.TOCRRESULTS Results = new TOCRdeclares.TOCRRESULTS();
            TOCRdeclares.TOCRJOBINFO JobInfo = new TOCRdeclares.TOCRJOBINFO();

            JobInfo.ProcessOptions.DisableCharacter = new short[256];
            //XPos and YPos refer to the positions on the original page BEFORE any rotation or deskewing  is performed (whether manual or automatic).
            JobInfo.ProcessOptions.StructId = 1;
            //JobInfo.ProcessOptions.LineRemoveOff = 1;
            //JobInfo.ProcessOptions.SectioningOn = 1;

            IntPtr MMFhandle = IntPtr.Zero;

            BMP = new Bitmap(path);
            MMFhandle = TOCRdeclares.ConvertBitmapToMMF2(BMP, true, true);

            if (!(MMFhandle.Equals(IntPtr.Zero))) {
                TOCRdeclares.TOCRSetConfig(TOCRdeclares.TOCRCONFIG_DEFAULTJOB, TOCRdeclares.TOCRCONFIG_DLL_ERRORMODE,
                                           TOCRdeclares.TOCRERRORMODE_MSGBOX);
                JobInfo.InputFile = "";
                JobInfo.JobType = TOCRdeclares.TOCRJOBTYPE_MMFILEHANDLE;
                Status = TOCRdeclares.TOCRInitialise(ref jobNo);
                if (Status == TOCRdeclares.TOCR_OK) {
                    JobInfo.PageNo = MMFhandle.ToInt32();
                    if (TOCRdeclares.OCRWait(jobNo, JobInfo)) {
                        if (TOCRdeclares.GetResults(jobNo, ref Results)) {
                            foreach (var rect in rectangles) {

                                //Currently problems with multiline text
                                int startIndex = -1, endIndex = -1, lastEndIndex = -1;
                                StringBuilder text = new StringBuilder();
                                for (int i = 0; i < Results.Item.Length; ++i) {
                                    TOCRdeclares.TOCRRESULTSITEM item = Results.Item[i];
                                    if (item.XPos >= rect.UpperLeft.X && item.XPos + item.XDim <= rect.LowerRight.X &&
                                        item.YPos >= rect.UpperLeft.Y && item.YPos + item.YDim <= rect.LowerRight.Y) {
                                        if(startIndex == -1)
                                            startIndex = i;
                                    } else if (startIndex != -1 && !(item.XPos == 0 && item.XDim == 0 && item.YDim == 0 && item.YPos == 0)) {
                                        //Skip trailing space
                                        endIndex = (Results.Item[i-1].OCRCha == 32) ? i-1 : i;

                                        //Account for newlines
                                        AddCharacters(Results, text, endIndex, lastEndIndex, startIndex);
                                        lastEndIndex = endIndex;
                                        startIndex = -1;
                                        endIndex = -1;
                                    }
                                }
                                //Need to read all chars to the end
                                if (startIndex != -1 && endIndex == -1) {
                                    AddCharacters(Results, text, Results.Item.Length, lastEndIndex, startIndex);
                                }

                                result.Add(text.ToString().Trim(new char[]{' ', '\r', '\n'}));
                            }
                            //TOCRdeclares.FormatResults(Results, ref Msg);
                            //TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
                            //result.Add(Msg);
                        }
                    }

                    TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
                }
            }

            return result;
        }

        private static void AddCharacters(TOCRdeclares.TOCRRESULTS Results, StringBuilder text, int endIndex, int lastEndIndex,
                                          int startIndex) {
            if (lastEndIndex != -1) {
                for (int j = lastEndIndex; j < startIndex; ++j)
                    if (Results.Item[j].OCRCha == 13) {
                        text.Append(Environment.NewLine);
                        break;
                    }
            }

            for (int j = startIndex; j < endIndex; ++j) {
                if (Results.Item[j].OCRCha == 13) text.Append(Environment.NewLine);
                else text.Append(Convert.ToChar(Results.Item[j].OCRCha));
            }
        }


        public static string ExtractImage(string path, Rectangle rect) {
            Image oldImage = Image.FromFile(path);
            //int width = 400;
            //int height = (int) (rect.Height*400.0f/rect.Width);
            //int height = 100;
            int height = rect.Height;
            int width = (int)((rect.Width * (double) height) / rect.Height);
            Image newImage = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(newImage);
            graphics.DrawImage(oldImage, new Rectangle(0, 0, width, height), rect, GraphicsUnit.Pixel);
            //newImage.Save("test_extracted.png");
            //String result = Marshal.PtrToStringAnsi(OCRpart("test_extracted.png", -1, 0, 0, width, height));
            newImage.Save("test_extracted.tiff", ImageFormat.Tiff);
            String result = OcrFile("test_extracted.tiff");
            return result;
        }

        public static List<OcrRectangle> ExtractCharRectangles() {
            int jobNo = 0;
            int Status;
            TOCRdeclares.TOCRRESULTSEX Results = new TOCRdeclares.TOCRRESULTSEX();
            TOCRdeclares.TOCRJOBINFO JobInfo = new TOCRdeclares.TOCRJOBINFO();

            JobInfo.ProcessOptions.DisableCharacter = new short[256];

            TOCRdeclares.TOCRSetConfig(TOCRdeclares.TOCRCONFIG_DEFAULTJOB, TOCRdeclares.TOCRCONFIG_DLL_ERRORMODE, TOCRdeclares.TOCRERRORMODE_MSGBOX);

            JobInfo.InputFile = "test_extracted.tiff";
            JobInfo.JobType = TOCRdeclares.TOCRJOBTYPE_TIFFFILE;

            List<OcrRectangle> result = new List<OcrRectangle>();

            Status = TOCRdeclares.TOCRInitialise(ref jobNo);
            if (Status == TOCRdeclares.TOCR_OK) {
                if (TOCRdeclares.OCRWait(jobNo, JobInfo)) {
                    if (TOCRdeclares.GetResults(jobNo, ref Results)) {
                        foreach (var item in Results.Item) {
                            result.Add(new OcrRectangle(new Point(item.XPos, item.YPos), new Point(item.XPos+item.XDim,item.YPos+item.YDim)));
                        }
                    }
                }

                TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
            }

            return result;
        }

        //private static int _jobNo = 0;
        private static string OcrFile(string file) {
            int jobNo = 0;
            int Status;
            string Msg = "";
            TOCRdeclares.TOCRRESULTSEX Results = new TOCRdeclares.TOCRRESULTSEX();
            TOCRdeclares.TOCRJOBINFO JobInfo = new TOCRdeclares.TOCRJOBINFO();

            JobInfo.ProcessOptions.DisableCharacter = new short[256];

            TOCRdeclares.TOCRSetConfig(TOCRdeclares.TOCRCONFIG_DEFAULTJOB, TOCRdeclares.TOCRCONFIG_DLL_ERRORMODE, TOCRdeclares.TOCRERRORMODE_MSGBOX);

            //JobInfo.InputFile = mSample_TIF_file;
            //JobInfo.JobType = TOCRJOBTYPE_TIFFFILE;

            // or
            JobInfo.InputFile = file;
            JobInfo.JobType = TOCRdeclares.TOCRJOBTYPE_TIFFFILE;
            
            Status = TOCRdeclares.TOCRInitialise(ref jobNo);
            if (Status == TOCRdeclares.TOCR_OK) {
                if (TOCRdeclares.OCRWait(jobNo, JobInfo)) {
                    if (TOCRdeclares.GetResults(jobNo, ref Results)) {
                        if (TOCRdeclares.FormatResults(Results, ref Msg)) {
                            TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
                            return Msg;
                        }
                    }
                }

                TOCRdeclares.TOCRShutdown(TOCRdeclares.TOCRSHUTDOWNALL);
            }
            
            return string.Empty;
        }


        //[StructLayout(LayoutKind.Sequential)]
        //public struct ScreenshotInformation {
        //    public int FoundX;
        //    public int FoundY;
        //    [MarshalAs(UnmanagedType.LPStr)] public string Path;
        //}
        //[DllImport("ImageHelperDll.dll")]
        //static extern void registerScreenshots(string refPath, ScreenshotInformation[] screenshots, int count);
        //static extern void registerScreenshots(StringBuilder test);

        public static ScreenshotInfo[] FindImageInImages(Screenshot refScreenshot, OcrRectangle anchorRect, ScreenshotGroup @group, ProgressCalculator progress) {
            ScreenshotInfo[] infos = new ScreenshotInfo[group.Screenshots.Count];

            ImageHelperClass imageHelper = new ImageHelperClass(refScreenshot.Path, (int)anchorRect.UpperLeft.X, (int)anchorRect.UpperLeft.Y, (int)anchorRect.Width, (int)anchorRect.Height);
            for (int i = 0; i < infos.Length; ++i) {
                if (!group.Screenshots[i].AnchorRemained || !refScreenshot.AnchorRemained)
                    infos[i] = imageHelper.registerScreenshot(group.Screenshots[i].Path);
                progress.StepDone();
            }
            return infos;
        }

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRpart")]
        static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
    }
}
