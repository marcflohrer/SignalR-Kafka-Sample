using System;
using StockProcessor.Models.Core;

namespace StockProcessor.Models {
    public class Stock {
        public string Symbol { get; set; }

        public decimal Price { get; set; }
    }
}