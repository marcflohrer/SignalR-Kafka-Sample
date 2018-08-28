using System;
using StockDatabase.Models.Core;

namespace StockDatabase.Models {
    public class Stock : BaseEntity<int> {
        public string Symbol { get; set; }

        public decimal Price { get; set; }
    }
}