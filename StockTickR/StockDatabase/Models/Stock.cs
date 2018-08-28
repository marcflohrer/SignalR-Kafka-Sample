using System;
using StockDatabase.Models.Core;

namespace StockDatabase.Models {
    public class Stock : BaseEntity<int> {
        public string Symbol { get; set; }

        public decimal DayOpen { get; private set; }

        public decimal DayLow { get; private set; }

        public decimal DayHigh { get; private set; }

        public decimal LastChange { get; private set; }

        public DateTime UpdateTime { get; private set; }

        public decimal Change {
            get {
                return Price - DayOpen;
            }
        }

        public double PercentChange {
            get {
                return (double) Math.Round (Change / Price, 4);
            }
        }
        decimal _price;

        public decimal Price {
            get {
                return _price;
            }
            set {
                if (_price == value) {
                    return;
                }

                LastChange = value - _price;
                _price = value;

                ResetDayOpenFromYesterday ();

                if (DayOpen == 0) {
                    DayOpen = _price;
                }

                if (_price < DayLow || DayLow == 0) {
                    DayLow = _price;
                }
                if (_price > DayHigh) {
                    DayHigh = _price;
                }
                UpdateTime = DateTime.Now;
            }
        }

        private void ResetDayOpenFromYesterday () {
            if (UpdatedYesterdayOrEarlier (UpdateTime)) {
                DayOpen = 0;
            }
        }

        private bool UpdatedYesterdayOrEarlier (DateTime time) {
            return DateTime.Today - time.Date >= TimeSpan.FromDays (1);
        }
    }
}