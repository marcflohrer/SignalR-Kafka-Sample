using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using StockProcessor.Models;
using StockProcessor.Repositories.Interfaces;

namespace StocksDatabase.Controllers {
    [Route ("[controller]")]
    public class StocksController : Controller {

        private Serilog.ILogger _logger {
            get;
        }

        public StocksController (Serilog.ILogger logger) {
            _logger = logger;
        }

        // GET: /stocks/
        [HttpGet]
        public IEnumerable<Stock> Get () {
            return null;
        }

        // GET: /stocks/1
        [HttpGet ("{id:int}")]
        public Stock Get (int id) {
            return null;
        }

        // POST: stocks/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost ("{id}")]
        public IActionResult PostCreate ([FromBody] Stock stock) {
            if (stock == null) {
                return View (stock);
            }
            var dbEntity = GetDbEntity (stock);
            if (dbEntity == null) {
                if (ModelState.IsValid) {
                    WatchOneStock (stock, "Insert Single Stock");
                    return InsertToDatabase (stock);
                }
            } else if (ModelState.IsValid) {
                dbEntity.Price = stock.Price;
                WatchOneStock (stock, "Update Single Stock");
                return UpdateStockInDatabase (stock);
            }
            return View (stock);
        }

        private IActionResult UpdateStockInDatabase (Stock stock) {
            return null;
        }

        private void WatchOneStock (Stock stock, string prefix) {
            var stockToWatch = Environment.GetEnvironmentVariable ("STOCK_TO_WATCH") ?? "Acme Inc.";
            if (stock.Symbol == stockToWatch) {
                _logger.Information ("[" + prefix + "] " + stock.Symbol + " : " + stock.Price + ", id = " + stock.Id + ", " + stock.Change + ", " + stock.DayHigh + ", " + stock.DayLow + ", " + stock.DayLow + ", " + stock.LastChange + ", " + stock.PercentChange + ", " + stock.UpdateTime);
            }
        }

        private IActionResult InsertToDatabase (Stock stock) {
            return null;
        }

        // POST: stocks/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public IActionResult PostCreate ([FromBody] IEnumerable<Stock> stocks) {
            if (stocks == null || !stocks.Any ()) {
                return View (stocks);
            }

            List<Stock> stocksFromDb = GetStocksFromDb (stocks).ToList ();
            UpdateStockPricesInDbEntities (stocks, stocksFromDb);
            stocksFromDb.ForEach (stock => WatchOneStock (stock, "WITH DB CONTEXT + PRICE UPDATE"));

            SaveToDatabase (stocksFromDb);

            return View (stocks);
        }

        private void SaveToDatabase (List<Stock> stocksFromDb) {
            stocksFromDb.ForEach (stock => PostCreate (stock));
        }

        private void UpdateStockPricesInDbEntities (IEnumerable<Stock> stocks, List<Stock> stocksFromDb) {
            stocksFromDb.ForEach (stock => UpdatePrice (stock, stocks.First (s => s.Symbol == stock.Symbol).Price));
        }

        private IEnumerable<Stock> GetStocksFromDb (IEnumerable<Stock> stocks) {
            var stocksFromDb = new List<Stock> ();
            stocks.ToList ().ForEach (stock => stocksFromDb.Add (GetDbEntityOrNewEntity (stock)));
            return stocksFromDb;
        }

        private Stock UpdatePrice (Stock dbEntity, Decimal price) {
            dbEntity.Price = price;
            return dbEntity;
        }

        private Stock GetDbEntityOrNewEntity (Stock newEntity) {
            return GetDbEntity (newEntity) ?? newEntity;
        }

        private Stock GetDbEntity (Stock stock) {
            return null;
        }

        private IActionResult ExecuteTransaction (Action action) {
            return RedirectToAction ("Index");
        }
    }
}