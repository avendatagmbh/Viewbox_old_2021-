using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitManagement.Logs {
    public class LogPageInfo {
        public LogPageInfo() {
            this.CurrentPage = 0;
            this.TimestampPages = new List<Tuple<DateTime, DateTime>>() { new Tuple<DateTime, DateTime>(new DateTime(1970, 1, 1, 0, 0, 0), DateTime.Now) };
        }

        public int CurrentPage{get;private set;}
        private List<Tuple<DateTime, DateTime>> TimestampPages;


        public void PreviousPage() {
            if (CurrentPage == 0)
                return;
            CurrentPage--;
        }

        public void NextPage() {
            if (CurrentPage <= TimestampPages.Count) {
                CurrentPage++;
            }
        }

        public DateTime CurrentBeginTimestamp() {
            if (CurrentPage < TimestampPages.Count)
                return TimestampPages[CurrentPage].Item1;
            if (CurrentPage-1 < TimestampPages.Count)
                return TimestampPages[CurrentPage].Item2;
            return new DateTime(1970, 1, 1, 0, 0, 0);
        }

        public DateTime CurrentEndTimestamp() {
            if (CurrentPage < TimestampPages.Count)
                return TimestampPages[CurrentPage].Item2;
            return DateTime.Now;
        }

        internal void UpdatePage(DateTime dateTimeBegin, DateTime dateTimeEnd) {
            while (CurrentPage >= TimestampPages.Count)
                TimestampPages.Add(new Tuple<DateTime, DateTime>(new DateTime(1970, 1, 1, 0, 0, 0), DateTime.Now));
            TimestampPages[CurrentPage] = new Tuple<DateTime, DateTime>(dateTimeBegin, dateTimeEnd);
        }
    }
}
