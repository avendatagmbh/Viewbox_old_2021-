using System;
using System.Globalization;
using System.Windows.Data;

namespace ViewboxBusiness.ProfileDb
{
	[ValueConversion(typeof(ViewscriptStates), typeof(string))]
	public class ViewscriptStatesToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (ViewscriptStates)value switch
			{
				ViewscriptStates.Ready => "Neuer View", 
				ViewscriptStates.CreatingIndex => "Erzeuge Indizes", 
				ViewscriptStates.CreateIndexError => "Fehler bei der Indexerstellung", 
				ViewscriptStates.CreatingTable => "Erstelle View", 
				ViewscriptStates.CreateTableError => "Fehler bei der Viewerstellung", 
				ViewscriptStates.CopyingTable => "Kopiere View in Zieldatenbank", 
				ViewscriptStates.CopyError => "Fehler beim Kopieren", 
				ViewscriptStates.Completed => "Vieweinspielung erfolgreich", 
				ViewscriptStates.Warning => "Warnung", 
				ViewscriptStates.CheckingReportParameters => "Checking report parameters (static view)", 
				ViewscriptStates.CheckingProcedureParameters => "Checking procedure parameters (dynamic view)", 
				ViewscriptStates.CheckingWhereCondition => "Checking report parameters (static view)", 
				ViewscriptStates.GettingIndexInfo => "Getting index data for tables", 
				ViewscriptStates.GeneratingDistinctValues => "Generating distinct data for parameters", 
				ViewscriptStates.CheckingReportParametersError => "Checking report parameters (static view) failed", 
				ViewscriptStates.CheckingProcedureParametersError => "Checking procedure parameters (dynamic view) failed", 
				ViewscriptStates.CheckingWhereConditionError => "Checking report parameters (static view)", 
				ViewscriptStates.GettingIndexInfoError => "Getting index data for tables failed", 
				ViewscriptStates.GeneratingDistinctValuesError => "Generating distinct data for parameters failed", 
				_ => string.Empty, 
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
