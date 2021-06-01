using System;

namespace DataAnalyze.Model {
    public class MatrixDataSource<T> {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixDataSource{T}" /> class.
        /// </summary>
        public MatrixDataSource() : this(0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixDataSource{T}" /> class.
        /// </summary>
        /// <param name="rank">The rank.</param>
        public MatrixDataSource(Int32 rank) : this(rank, rank) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixDataSource{T}" /> class.
        /// </summary>
        /// <param name="Columns">The columns.</param>
        /// <param name="Rows">The rows.</param>
        public MatrixDataSource(Int32 Columns, Int32 Rows) { _data = new T[Columns,Rows]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixDataSource{T}" /> class.
        /// </summary>
        /// <param name="dataArray">The data array.</param>
        public MatrixDataSource(T[,] dataArray) {
            Int32 RowCount = dataArray.GetLength(0);
            Int32 ColumnCount = dataArray.GetLength(1);
            _data = new T[ColumnCount,RowCount];

            for (Int32 i = 0; i < ColumnCount; i++) {
                for (Int32 j = 0; j < RowCount; j++) {
                    _data[i, j] = dataArray[i, j];
                }
            }
        }
        #endregion

        #region Properties
        private readonly T[,] _data;
        #endregion

        #region Members
        /// <summary>
        /// Gets or sets the <see cref="`0" /> with the specified column.
        /// </summary>
        /// <value>
        /// The <see cref="`0" />.
        /// </value>
        /// <param name="Column">The column.</param>
        /// <param name="Row">The row.</param>
        /// <returns></returns>
        public T this[Int32 Column, Int32 Row] { get { return _data[Column, Row]; } set { _data[Column, Row] = value; } }

        /// <summary>
        /// Gets the column count.
        /// </summary>
        /// <value>
        /// The column count.
        /// </value>
        public Int32 ColumnCount { get { return _data.GetLength(0); } }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        /// <value>
        /// The row count.
        /// </value>
        public Int32 RowCount { get { return _data.GetLength(1); } }

        /// <summary>
        /// Gets the default view.
        /// </summary>
        /// <value>
        /// The default view.
        /// </value>
        public MatrixBase<T> DefaultView { get { return new MatrixBase<T>(this); } }
        #endregion
    }
}