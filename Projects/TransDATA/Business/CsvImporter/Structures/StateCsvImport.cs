using System;

namespace Business.CsvImporter.Structures {
    public class StateCsvImport {

        /// <summary>
        /// Initializes a new instance of the <see cref="StateCsvImport"/> class.
        /// </summary>
        /// <param name="totalTables">The total tables.</param>
        /// <param name="totalBytes">The total bytes.</param>
        public StateCsvImport(int totalTables, long totalBytes) {
            this.TotalTables = totalTables;
            this.ProcessedTables = 0;

            this.TotalBytes = totalBytes;
            this.ProcessedBytes = 0;

            mLocked = false;
        }

        /// <summary>
        /// Gets or sets the total tables.
        /// </summary>
        /// <value>The total tables.</value>
        public int TotalTables { get; private set; }

        /// <summary>
        /// Gets or sets the processed tables.
        /// </summary>
        /// <value>The processed tables.</value>
        public int ProcessedTables { get; private set; }

        /// <summary>
        /// Gets or sets the total bytes.
        /// </summary>
        /// <value>The total bytes.</value>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// Gets or sets the processed bytes.
        /// </summary>
        /// <value>The processed bytes.</value>
        public long ProcessedBytes { get; private set; }

        /// <summary>
        /// Gets the processed bytes percent.
        /// </summary>
        /// <value>The processed bytes percent.</value>
        public double ProcessedBytesPercent {
            get {
                if (TotalBytes == 0) {
                    return 0;
                } else {
                    return Math.Min(1, (double)ProcessedBytes / (double)TotalBytes);
                }
            }
        }

        private bool mLocked;

        public void Lock() {
            mLocked = true;
        }

        public void Unlock() {
            mLocked = false;
        }

        public void IncProcessedTables() {
            while (this.mLocked) System.Threading.Thread.Sleep(100);
            this.ProcessedTables++;
        }

        public void IncProcessedBytes(long bytes) {
            while (this.mLocked) System.Threading.Thread.Sleep(100);
            this.ProcessedBytes += bytes;
        }
    }
}
