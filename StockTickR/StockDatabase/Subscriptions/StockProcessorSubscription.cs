using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Serilog;
using StockProcessor.Hubs;
using StockProcessor.Models;
using StockProcessor.Repositories.Interfaces;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace StockProcessor.Subscriptions {
    public class StockProcessorSubscription : IDatabaseSubscription {
        private bool disposedValue = false;
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger _logger;
        private SqlTableDependency<Stock> _tableDependency;

        public IConfigurationRoot Configuration {
            get;
        }

        public StockProcessorSubscription (IHubContext<StockHub> hubContext, IConfigurationRoot configuration, ILogger logger) {
            _hubContext = hubContext;
            Configuration = configuration;
            _logger = logger;
        }

        public void Configure (string connectionString) {
            _logger.Information ("Configure  _tableDependency...");
            _tableDependency = new SqlTableDependency<Stock> (
                connectionString, "Stocks", "dbs", null, null, null, DmlTriggerType.All, false, false);
            _tableDependency.OnChanged += TableDependency_Changed;
            _tableDependency.OnError += TableDependency_OnError;
            Start ();
        }

        public void Start () {
            _tableDependency.Start ();
            _logger.Information ("Waiting to receive notifications...");
        }

        private void TableDependency_Changed (object sender, RecordChangedEventArgs<Stock> e) {
            if (e.ChangeType != ChangeType.None) {
                WatchOneStock (e.Entity, Configuration.GetValue<String> ("STOCK_TO_WATCH"), "DB-STREAM");
                var changedEntity = e.Entity;
                _hubContext.Clients.All.SendAsync ("UpdateStocks", e.Entity);
            } else {
                _logger.Error ("ChangeType.None: Changed stock price detected: " + e.Entity.Symbol + " : " + e.Entity.Price + ", old: " + e.EntityOldValues.Price + ", id = " + e.Entity.Id);
            }
        }

        private void WatchOneStock (Stock stock, string stockName, string prefix) {
            if (stock.Symbol == stockName) {
                _logger.Information ("[" + prefix + "] " + stock.Symbol + " : " + stock.Price + ", id = " + stock.Id + ", " + stock.Change + ", " + stock.DayHigh + ", " + stock.DayLow + ", " + stock.DayLow + ", " + stock.LastChange + ", " + stock.PercentChange + ", " + stock.UpdateTime);
            }
        }

        private void TableDependency_OnError (object sender, ErrorEventArgs e) {
                _logger.Error ($"SqlTableDependency error: {e.Error.Message}");
                _hubContext.Clients.All.SendAsync ("UpdateStocksError", e.Error);
            }

            #region IDisposable

            ~StockProcessorSubscription () {
                Dispose (false);
            }

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _tableDependency.Stop ();
                }
                disposedValue = true;
            }
        }

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        #endregion
    }
}