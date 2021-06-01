using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
	public class CsvReader
	{
		public delegate TextReader CreateReaderDelegate(string filename, Encoding encoding);

		public string Filename { get; set; }

		public char Separator { get; set; }

		public bool HeadlineInFirstRow { get; set; }

		public char StringsOptionallyEnclosedBy { get; set; }

		public string LastError { get; private set; }

		public CreateReaderDelegate CreateReader { get; set; }

		public CsvReader(string filename)
		{
			Filename = filename;
			Separator = ',';
			StringsOptionallyEnclosedBy = '"';
			HeadlineInFirstRow = true;
			CreateReader = (string s, Encoding encoding) => new StreamReader(new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), encoding);
		}

		private string NormalizeHeader(string header)
		{
			return header.Trim().Replace('/', '_').Replace('.', '_');
		}

		public DataTable GetCsvData(int limit, Encoding encoding)
		{
			LastError = null;
			DataTable csvData = new DataTable();
			TextReader reader = null;
			StringBuilder value = new StringBuilder();
			int lines = 0;
			try
			{
				reader = CreateReader(Filename, encoding);
				int lastColumnCount = -1;
				string csvLine2;
				while ((csvLine2 = reader.ReadLine()) != null)
				{
					if (limit == 0 || lines < limit)
					{
						csvLine2 = csvLine2.Trim();
						lines++;
						if (csvLine2.Length == 0)
						{
							continue;
						}
						int curColumnCount = 0;
						if (lastColumnCount < 0)
						{
							List<string> values2 = new List<string>();
							bool inTextMode2 = false;
							value.Clear();
							for (int j = 0; j < csvLine2.Length; j++)
							{
								if (inTextMode2)
								{
									if (csvLine2[j].Equals(StringsOptionallyEnclosedBy))
									{
										inTextMode2 = false;
									}
									else
									{
										value.Append(csvLine2[j]);
									}
								}
								else if (csvLine2[j].Equals(Separator))
								{
									curColumnCount++;
									if (HeadlineInFirstRow)
									{
										if (csvData.Columns.Contains(NormalizeHeader(value.ToString())))
										{
											int k = 1;
											string name = NormalizeHeader(value.ToString()) + "(" + k + ")";
											while (csvData.Columns.Contains(name))
											{
												k++;
												name = NormalizeHeader(value.ToString()) + "(" + k + ")";
											}
											csvData.Columns.Add(name);
										}
										else
										{
											csvData.Columns.Add(NormalizeHeader(value.ToString()));
										}
									}
									else
									{
										csvData.Columns.Add("Spalte " + curColumnCount);
										values2.Add(value.ToString());
									}
									value.Clear();
								}
								else if (csvLine2[j].Equals(StringsOptionallyEnclosedBy))
								{
									inTextMode2 = true;
								}
								else
								{
									value.Append(csvLine2[j]);
								}
							}
							if (value.Length > 0 || curColumnCount < lastColumnCount || csvLine2.Last() == Separator)
							{
								curColumnCount++;
								if (HeadlineInFirstRow)
								{
									if (csvData.Columns.Contains(NormalizeHeader(value.ToString())))
									{
										int l = 1;
										string name2 = NormalizeHeader(value.ToString()) + "(" + l + ")";
										while (csvData.Columns.Contains(name2))
										{
											l++;
											name2 = NormalizeHeader(value.ToString()) + "(" + l + ")";
										}
										csvData.Columns.Add(name2);
									}
									else
									{
										csvData.Columns.Add(NormalizeHeader(value.ToString()));
									}
								}
								else
								{
									csvData.Columns.Add("Spalte " + curColumnCount);
									values2.Add(value.ToString());
								}
							}
							if (!HeadlineInFirstRow)
							{
								csvData.Rows.Add(values2);
							}
							lastColumnCount = curColumnCount;
							continue;
						}
						object[] values = new object[lastColumnCount];
						bool inTextMode = false;
						value.Clear();
						for (int i = 0; i < csvLine2.Length; i++)
						{
							if (inTextMode)
							{
								if (csvLine2[i].Equals(StringsOptionallyEnclosedBy))
								{
									inTextMode = false;
								}
								else
								{
									value.Append(csvLine2[i]);
								}
							}
							else if (csvLine2[i].Equals(Separator))
							{
								values[curColumnCount] = value.ToString();
								value.Clear();
								curColumnCount++;
							}
							else if (csvLine2[i].Equals(StringsOptionallyEnclosedBy))
							{
								inTextMode = true;
							}
							else
							{
								value.Append(csvLine2[i]);
							}
						}
						if (value.Length > 0 || curColumnCount < lastColumnCount)
						{
							values[curColumnCount] = value.ToString();
							curColumnCount++;
						}
						if (curColumnCount == lastColumnCount)
						{
							csvData.Rows.Add(values);
							continue;
						}
						throw new Exception("Invalid csv file!");
					}
					return csvData;
				}
				return csvData;
			}
			catch (Exception)
			{
				LastError = "Fehler beim Einlesen der CSV-Datei in Zeile " + lines;
				csvData = new DataTable();
				lines = 0;
				reader?.Close();
				reader = CreateReader(Filename, Encoding.UTF7);
				csvData.Columns.Add("Datenfehler (falsches Trennzeichen?)");
				string csvLine;
				while ((csvLine = reader.ReadLine()) != null)
				{
					if (limit == 0 || lines < limit)
					{
						lines++;
						csvData.Rows.Add(csvLine);
						continue;
					}
					return csvData;
				}
				return csvData;
			}
			finally
			{
				reader?.Close();
			}
		}

		public string DetectCommonFileEnd(int limit, Encoding encoding)
		{
			using TextReader reader = CreateReader(Filename, encoding);
			StringBuilder line = new StringBuilder();
			List<string> lines = new List<string>();
			int currentChar;
			while ((currentChar = reader.Read()) != -1)
			{
				line.Append((char)currentChar);
				if (currentChar == 10)
				{
					lines.Add(line.ToString());
					if (lines.Count == limit)
					{
						break;
					}
					line.Clear();
				}
			}
			StringBuilder lineEndReversed = new StringBuilder();
			if (lines.Count <= 1 || lines[0].Length == 0)
			{
				return "";
			}
			for (int reverseIndex = 0; reverseIndex < lines[0].Length; reverseIndex++)
			{
				char curChar = '\0';
				foreach (string curLine in lines)
				{
					if (reverseIndex >= curLine.Length)
					{
						return string.Join("", lineEndReversed.ToString().Reverse());
					}
					if (curLine == lines[0])
					{
						curChar = curLine[curLine.Length - 1 - reverseIndex];
					}
					else if (curChar != curLine[curLine.Length - 1 - reverseIndex])
					{
						return string.Join("", lineEndReversed.ToString().Reverse());
					}
				}
				lineEndReversed.Append(curChar);
			}
			return string.Join("", lineEndReversed.ToString().Reverse());
		}
	}
}
