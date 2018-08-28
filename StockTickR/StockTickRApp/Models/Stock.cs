using System;
using StockDatabase.Models.Core;

namespace StockTickR.Models {
    public class Stock : BaseEntity {
        public string Symbol { get; set; }

        public decimal DayOpen { get; set; }

        public decimal DayLow { get; set; }

        public decimal DayHigh { get; set; }

        public decimal LastChange { get; set; }

        public decimal Change { get; set; }

        public double PercentChange { get; set; }

        public decimal Price { get; set; }
    }
}