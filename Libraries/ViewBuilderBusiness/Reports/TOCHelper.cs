using System.Collections.Generic;
using System.Linq;
using EO.Pdf;
using EO.Pdf.Acm;

namespace ViewBuilderBusiness.Reports
{
    internal class TOCHelper
    {
        //public List<Item> Items = new List<Item>();
        public List<Item> Items = new List<Item>();
        private PdfBookmark parentBookmark;

        public static PdfDocument Document { get; set; }
        public int PageCount { get; set; }

        public AcmContent GetTitle(string index, string title, int pageIndex)
        {
            var text = index + " " + title;
            var content = new AcmBlock(new AcmBold(new AcmText(text)));
            var bookmark = new PdfBookmark(text);
            // adds
            Items.Add(new Item
                          {
                              Index = index,
                              PageIndex = pageIndex + 1,
                              Title = title,
                              Bookmark = bookmark,
                              Content = content
                          });
            return content;
        }

        public AcmContent GetTOC()
        {
            PdfBookmark parentBookmark = null;
            var tocTable = new EOPdfTableHelper(new[] {5f, 1f}) {ShowBorder = false};

            foreach (var item in Items)
            {
                var text = item.Index + " " + item.Title;
                tocTable.AddRow(new[] {text, item.PageIndex.ToString()});
                item.Bookmark.Destination = item.Content.CreateDestination();
                // creates a 2 level bookmark hierarchy 
                if (item.Index.Length > 2 && parentBookmark != null)
                {
                    parentBookmark.ChildNodes.Add(item.Bookmark);
                }
                else
                {
                    parentBookmark = item.Bookmark;
                    Document.Bookmarks.Add(item.Bookmark);
                }
            }
            return tocTable.GetTable().SetProperty("Style.FontSize", 10f);
//            PdfBookmark parentBookmark = null;
//            foreach (var item in Items)
//            {
//                var text = (item.Index + " " + item.Title + " ").PadRight(70, '_') +
//                                        (item.PageIndex.ToString() + " ").PadLeft(5, '_');
//
//                var content = new AcmBlock(new AcmBold(new AcmText(text))).SetProperty("Style.FontSize", 10f);
//
//                item.Bookmark.Destination = item.Content.CreateDestination();
//
//                // creates a 2 level bookmark hierarchy 
//                if (item.Index.Length > 2 && parentBookmark != null)
//                {
//                    parentBookmark.ChildNodes.Add(item.Bookmark);
//                }
//                else
//                {
//                    parentBookmark = item.Bookmark;
//                    TOCHelper.Document.Bookmarks.Add(item.Bookmark);
//                }
//
//                yield return content;
//            }
        }

        /// <summary>
        ///   Creates the toc table (part of 39 items) that fits within the given page index.
        /// </summary>
        /// <param name="tocPageIndex"> </param>
        /// <returns> </returns>
        internal AcmContent GetTOC(int tocPageIndex)
        {
            var tocTable = new EOPdfTableHelper(new[] {5f, 1f}) {ShowBorder = false};
            var takeCount = 39;
            var skipCount = tocPageIndex == 0 ? 0 : 39;
            foreach (var item in Items.Skip(skipCount).Take(takeCount))
            {
                var text = item.Index + " " + item.Title;
                tocTable.AddRow(new[] {text, item.PageIndex.ToString()});
                item.Bookmark.Destination = item.Content.CreateDestination();
                // creates a 2 level bookmark hierarchy 
                if (item.Bookmark.Parent == null)
                {
                    if (item.Index.Length > 2)
                    {
                        if (parentBookmark != null)
                            parentBookmark.ChildNodes.Add(item.Bookmark);
                    }
                    else
                    {
                        parentBookmark = item.Bookmark;
                        Document.Bookmarks.Add(item.Bookmark);
                    }
                }
            }
            return tocTable.GetTable().SetProperty("Style.FontSize", 10f);
        }

        #region Nested type: Item

        public class Item
        {
            public string Index { get; set; }
            public string Title { get; set; }
            public int PageIndex { get; set; }
            public PdfBookmark Bookmark { get; set; }
            public AcmContent Content { get; set; }
        }

        #endregion
    }
}