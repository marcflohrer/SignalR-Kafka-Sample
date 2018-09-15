using StockProcessor.Models;
using StockProcessor.Repositories.Core;

namespace StockProcessor.Repositories.Interfaces {
    public interface IStockRepository : IRepository<Stock, int> {
        Stock Insert (Stock stock);
        void Update (Stock stock);
        void Delete (int symbol);
        Stock GetStockBySymbol (string symbol);
    }
}