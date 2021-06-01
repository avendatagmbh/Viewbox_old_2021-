using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxMdConverter
{
	internal class TextToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			Document document = new Document();
			using (FileStream stream = File.OpenWrite(output))
			{
				PdfWriter.GetInstance(document, stream);
				document.Open();
				File.ReadAllLines(input).ToList().ForEach(delegate(string line)
				{
					document.Add(new Paragraph(line));
				});
				document.Close();
			}
			return true;
		}
	}
}
