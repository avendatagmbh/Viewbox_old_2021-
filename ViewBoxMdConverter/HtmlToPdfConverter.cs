using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace ViewboxMdConverter
{
	internal class HtmlToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			string htmlText = File.ReadAllText(input);
			Document document = new Document();
			PdfWriter.GetInstance(document, new FileStream(output, FileMode.Create));
			document.Open();
			new HTMLWorker(document).Parse(new StringReader(htmlText));
			document.Close();
			return true;
		}
	}
}
