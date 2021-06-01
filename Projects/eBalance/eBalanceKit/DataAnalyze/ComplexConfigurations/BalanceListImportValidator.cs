using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DataAnalyze.Model;
using DataAnalyze.Strategies.Analyzer;
using System.Data;
using eBalanceKitResources.Localisation;

namespace DataAnalyze.ComplexConfigurations
{
    public class BalanceListImportValidator
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceListImportValidator" /> class.
        /// </summary>
        public BalanceListImportValidator()
        { }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// The container that holds the anomalies happened on importing.
        /// </summary>
        private static AnomalyContainer container = new AnomalyContainer();

        /// <summary>
        /// Gets the list of errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public static List<Anomaly> Errors
        {
            get { return container.GetProblems(); }
        }

        /// <summary>
        /// The input matrix, that needed to be processed.
        /// </summary>
        private MatrixBase<object> _inputMatrix;

        /// <summary>
        /// Gets or sets the input matrix.
        /// </summary>
        /// <value>
        /// The input matrix.
        /// </value>
        public MatrixBase<object> InputMatrix
        {
            get { return _inputMatrix; }
            set
            {
                _inputMatrix = value;
                _outputMatrix = new MatrixBase<object>(_inputMatrix.ColumnCount, _inputMatrix.RowCount);
            }
        }

        /// <summary>
        /// The output matrix, that contains the processed and valid datas.
        /// </summary>
        private MatrixBase<object> _outputMatrix;

        /// <summary>
        /// Gets the output matrix.
        /// </summary>
        /// <value>
        /// The output matrix.
        /// </value>
        public MatrixBase<object> OutputMatrix
        {
            get { return _outputMatrix; }
            private set { _outputMatrix = value; }
        }

        private List<int> _requiredFields = new List<int>();

        /// <summary>
        /// Gets or sets the required fields.
        /// </summary>
        /// <value>
        /// The required fields.
        /// </value>
        public List<int> RequiredFields
        {
            get { return _requiredFields;} 
            set { _requiredFields = value; }
        }

        private List<int> _uniqueFields = new List<int>();
        /// <summary>
        /// Gets or sets the unique fields.
        /// </summary>
        /// <value>
        /// The unique fields.
        /// </value>
        public List<int> UniqueFields
        {
            get { return _uniqueFields; }
            set { _uniqueFields = value; }
        }

        private List<int> _numericFields = new List<int>();

        /// <summary>
        /// Gets or sets the numeric fields.
        /// </summary>
        /// <value>
        /// The numeric fields.
        /// </value>
        public List<int> NumericFields
        {
            get { return _numericFields; }
            set { _numericFields = value; }
        }

        private List<int> _convertToNegativeFields = new List<int>();

        /// <summary>
        /// Gets or sets the convert to negative fields.
        /// </summary>
        /// <value>
        /// The convert to negative fields.
        /// </value>
        public List<int> ConvertToNegativeFields
        {
            get { return _convertToNegativeFields; }
            set { _convertToNegativeFields = value; }
        }

        private static List<String> _columns = new List<String>();
        /// <summary>
        /// Gets or sets the convert to negative fields.
        /// </summary>
        /// <value>
        /// The convert to negative fields.
        /// </value>
        public static List<String> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            /*
            * According to Sebastian Vetter H 2012.10.29. 11:52
            * Do not show “The whole row is empty“ as error. Just skip it silent.
            */
            get { return container.HasErrors; }
        }

        /// <summary>
        /// Gets the skippable anomalies.
        /// </summary>
        /// <value>
        /// The skippable anomalies.
        /// </value>
        public static List<Anomaly> SkippableAnomalies
        {
            get { return container.DisplayableAnomalies.OrderBy(q => q.RowNumber).ToList(); }
        }

        /// <summary>
        /// Gets the skippable anomalies table.
        /// </summary>
        /// <value>
        /// The skippable anomalies table.
        /// </value>
        public static DataTable SkippableAnomaliesTable
        {
            get { return container.GetProblemTable(); }
        }

        public static NumberFormatInfo NumberFormatInfo;
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the validate.
        /// </summary>
        public void DoValidate()
        {
            container = new AnomalyContainer();

            AnalyzeRowLevel();
            AnalyzeColumnLevel();
            DoCopy();
        }

        /// <summary>
        /// Detects the delimiter.
        /// </summary>
        /// <param name="number">The number.</param>
        private void DetectDelimiter(String number)
        {
            if (NumberFormatInfo == null)
            {
                NumberFormatInfo = new NumberFormatInfo();
                foreach (char ch in number.Reverse().ToList())
                {
                    if (ch == '.' || ch == ',')
                    {
                        NumberFormatInfo.NumberDecimalSeparator = ch.ToString();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Does the copy.
        /// </summary>
        private void DoCopy()
        {
            int copiedRows = -1;
            _outputMatrix = new MatrixBase<object>(_inputMatrix.ColumnCount, _inputMatrix.RowCount - container.ErrorCount);
            

            // Iterate through the rows.
            for ( int i = 0; i < _inputMatrix.RowCount; i++ )
            {
                // Should we have to skip this row?
                if (container.SkippableRows.Contains(i))
                {
                    continue;
                }
                else
                {
                    copiedRows++;
                }

                // Copy the row
                for (int j = 0; j < _inputMatrix.ColumnCount; j++)
                {
                    // Perform a conversion between positive and negative values
                    // (for Haben (H) conversion)
                    if (ConvertToNegativeFields.Contains(j))
                    {
                        DetectDelimiter(_inputMatrix[j, i].ToString());
                        _outputMatrix[j, copiedRows] = (-1 * Convert.ToDouble(_inputMatrix[j, i], NumberFormatInfo)).ToString(NumberFormatInfo);
                    }
                    else
                    {
                        // Regular row
                        _outputMatrix[j, copiedRows] = _inputMatrix[j, i];
                        
                    }
                }
            }
        }

        /// <summary>
        /// Analyzes in the column level.
        /// </summary>
        private void AnalyzeColumnLevel()
        {
            CheckRequiredFields();
            CheckUniqueFields();
            
            // Check for numeric fields
            // also check for Haben (H) fields if they have same sign
            List<object> creditValues = new List<object>();

            foreach (int numericField in NumericFields)
            {
                for (int i = 0; i < InputMatrix.RowCount; i++)
                {
                    IsNumericAnalyze numAn = new IsNumericAnalyze();
                    numAn.Analyze(InputMatrix[numericField, i]);

                    // If the row is not numeric, but it needed to be...
                    // I can't check for FALSE equality, because it may return INDETERMINABLE.
                    if (numAn.Result.ResultType != Result.True)
                    {
                        Anomaly anomaly = new Anomaly()
                        {
                            Problem = ProblemType.NumericFieldIsNotNumeric,
                            RowNumber = i,
                            Severity = Severity.Error,
                            NiceProblem = ResourcesValidation.NumericFieldIsNotNumeric,
                            RawData = InputMatrix.GetRow(i)
                        };

                        container.Add(anomaly);
                    }
                    else if (ConvertToNegativeFields.Contains(numericField) && !container.SkippableRows.Contains(i))
                    {
                        creditValues.Add(InputMatrix[numericField, i]);
                    }
                }
            }

            // We must split these two cases, because it may
            // contain positive and negative values,
            // which means lack of csv integrity.
            bool positiveCreditValues = new DataAnalyzer(Strategy.IsPositiveOrZero, creditValues, Treshold.OnlyTrueAccepted).DoAnalyze().Valid;
            bool negativeCreditValues = new DataAnalyzer(Strategy.IsNegativeOrZero, creditValues, Treshold.OnlyTrueAccepted).DoAnalyze().Valid;

            // Drop the list of positive fields
            if (positiveCreditValues)
            {
                _convertToNegativeFields = new List<int>();
            }
        }

        /// <summary>
        /// Check for unique fields
        /// </summary>
        private void CheckUniqueFields()
        {
            foreach (int uniqueField in UniqueFields)
            {
                PrimaryKeyAnalyze.Reset();

                for (int i = 0; i < InputMatrix.RowCount; i++)
                {
                    // If the row is NOT unique...
                    PrimaryKeyAnalyze an = new PrimaryKeyAnalyze();
                    an.Analyze(InputMatrix[uniqueField, i]);

                    if (an.Result.ResultType == Result.False)
                    {
                        Anomaly anomaly = new Anomaly()
                                          {
                                              Problem = ProblemType.DuplicatedPrimaryKey,
                                              RowNumber = i,
                                              Severity = Severity.Error,
                                              NiceProblem = ResourcesValidation.DuplicatedAccountNumber,
                                              RawData = InputMatrix.GetRow(i)
                                          };

                        container.Add(anomaly);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the required fields.
        /// </summary>
        private void CheckRequiredFields()
        {
            foreach (int requiredField in RequiredFields)
            {
                for (int i = 0; i < InputMatrix.RowCount; i++)
                {
                    IsEmptyAnalyze an = new IsEmptyAnalyze();
                    an.Analyze(InputMatrix[requiredField, i]);

                    // If the row is empty...
                    if (an.Result.ResultType != Result.True) {
                        continue;
                    }

                    if (NumericFields.Contains(requiredField))
                    {
                        InputMatrix[requiredField, i] = 0;

                        Anomaly anomaly = new Anomaly {
                                              Problem = ProblemType.RequestedFieldIsMissing,
                                              RowNumber = i,
                                              Severity = Severity.Warning,
                                              Description = ResourcesValidation.MoneyValueReset,
                                              NiceProblem = ResourcesValidation.InvalidMoneyFormat,
                                              RawData = InputMatrix.GetRow(i)
                                          };

                        container.Add(anomaly);
                    }
                    else
                    {
                        Anomaly anomaly = new Anomaly {
                                              Problem = ProblemType.RequestedFieldIsMissing,
                                              RowNumber = i,
                                              Severity = Severity.Error,
                                              NiceProblem = ResourcesValidation.NoAccountGiven,
                                              RawData = InputMatrix.GetRow(i)
                                          };

                        container.Add(anomaly);
                    }
                }
            }
        }

        /// <summary>
        /// Analyzes the data in the row level.
        /// </summary>
        private void AnalyzeRowLevel()
        {
            for (int j = 0; j < InputMatrix.RowCount; j++)
            {
                List<Object> rowItems = new List<object>();
                for (int i = 0; i < InputMatrix.ColumnCount; i++)
                {
                    rowItems.Add(InputMatrix[i, j]);
                }

                // If the whole row was empty...
                if ((!new DataAnalyzer(Strategy.IsEmpty, rowItems, Treshold.OnlyTrueAccepted).DoAnalyze().Valid)) {
                    continue;
                }

                Anomaly anomaly = new Anomaly()
                                  {
                                      Problem = ProblemType.WholeRowIsEmpty,
                                      RowNumber = j,
                                      Severity = Severity.Information,
                                      NiceProblem = ResourcesValidation.WholeRowWasEmpty,
                                      RawData = InputMatrix.GetRow(j)
                                  };

                container.Add(anomaly);
            }
        }
        #endregion Methods
    }
}
