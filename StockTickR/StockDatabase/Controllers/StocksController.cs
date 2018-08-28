using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using StockDatabase.Models;
using StockDatabase.Repositories.Interfaces;

namespace StocksDatabase.Controllers {
    [Route ("[controller]")]
    public class StocksController : Controller {

        public IUnitOfWork UnitOfWork { get; }
        private Serilog.ILogger _logger { get; }

        public StocksController (IUnitOfWork unitOfWork, Serilog.ILogger logger) {
            UnitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: /stocks/
        [HttpGet]
        public IEnumerable<Stock> Get () {
            _logger.Debug ("public IEnumerable<Stock> Get");
            return UnitOfWork.Stocks.GetAll ();
        }

        // GET: /stocks/1
        [HttpGet ("{id:int}")]
        public Stock Get (int id) {
            _logger.Debug ("public Stock Get");
            return UnitOfWork.Stocks.Get (id);
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
                if (ModelState.IsValid)
                {
                    WatchOneStock(stock, "Apple", "Insert Single Stock");
                    return InsertToDatabase(stock);
                }
            } else if (ModelState.IsValid)
            {
                dbEntity.Price = stock.Price;
                WatchOneStock(stock, "Apple", "Update Single Stock");
                return UpdateStockInDatabase(stock);
            }
            return View (stock);
        }

        private IActionResult UpdateStockInDatabase(Stock stock)
        {
            return ExecuteTransaction(() => UnitOfWork.Stocks.Update(stock));
        }

        private void WatchOneStock(Stock stock, string stockName, string prefix)
        {
            if (stock.Symbol == stockName)
            {
                _logger.Information("["+prefix+"] " + stock.Symbol + " : " + stock.Price + ", id = " + stock.Id + ", " +stock.Change + ", " + stock.DayHigh + ", " + stock.DayLow + ", " + stock.DayLow + ", " + stock.LastChange + ", " + stock.PercentChange + ", " + stock.UpdateTime);
            }
        }

        private IActionResult InsertToDatabase(Stock stock)
        {
            return ExecuteTransaction(() => UnitOfWork.Stocks.Insert(stock));
        }

        // POST: stocks/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public IActionResult PostCreate ([FromBody] IEnumerable<Stock> stocks)
        {
            if (stocks == null || !stocks.Any())
            {
                return View(stocks);
            }
            
            stocks.ToList().ForEach(stock => WatchOneStock(stock, "Apple", "NO DB CONTEXT"));

            List<Stock> stocksFromDb = GetStocksFromDb(stocks).ToList();            
            stocksFromDb.ForEach(stock => WatchOneStock(stock, "Apple", "WITH DB CONTEXT"));

            UpdateStockPricesInDbEntities(stocks, stocksFromDb);            
            stocksFromDb.ForEach(stock => WatchOneStock(stock, "Apple", "WITH DB CONTEXT + PRICE UPDATE"));

            SaveToDatabase(stocksFromDb);

            return View(stocks);
        }

        private void SaveToDatabase(List<Stock> stocksFromDb)
        {
            stocksFromDb.ForEach(stock => PostCreate(stock));
        }

        private void UpdateStockPricesInDbEntities(IEnumerable<Stock> stocks, List<Stock> stocksFromDb)
        {
            stocksFromDb.ForEach(stock => UpdatePrice(stock, stocks.First(s => s.Symbol == stock.Symbol).Price));
        }

        private IEnumerable<Stock> GetStocksFromDb(IEnumerable<Stock> stocks)
        {
            var stocksFromDb = new List<Stock>();
            stocks.ToList().ForEach(stock => stocksFromDb.Add(GetStockDbEntity(stock)));
            return stocksFromDb;
        }

        private Stock UpdatePrice(Stock dbEntity, Decimal price)
        {
            dbEntity.Price = price;
            return dbEntity;
        }

        private Stock GetStockDbEntity(Stock stock)
        {
            return GetDbEntity(stock) ?? stock;
        }

        private Stock GetDbEntity(Stock stock)
        {
            return UnitOfWork.Stocks.GetStockBySymbol(stock.Symbol);
        }

        private IActionResult ExecuteTransaction (Action action) {
            action.Invoke ();
            UnitOfWork.Complete ();
            return RedirectToAction ("Index");
        }
    }
}