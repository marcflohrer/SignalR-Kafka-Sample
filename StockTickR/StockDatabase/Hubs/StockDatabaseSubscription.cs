using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using StockDatabase.Models;
using StockDatabase.Repositories.Interfaces;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Abstracts;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;
using TableDependency.SqlClient.Where;

namespace StockDatabase.Hubs {
    public class StockDatabaseSubscription : IDatabaseSubscription {
        private bool disposedValue = false;
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger _logger;
        private SqlTableDependency<Stock> _tableDependency;

        public StockDatabaseSubscription (IHubContext<StockHub> hubContext, ILogger logger) {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void Configure (string connectionString) {
            _logger.Information ("Configure  _tableDependency...");
            _tableDependency = new SqlTableDependency<Stock> (
                connectionString, "Stocks", "dbs", null, null, null, DmlTriggerType.All, false, true);
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
                if (e.Entity.Symbol == "Apple") {
                    _logger.Information ("Changed stock price detected: " + e.Entity.Symbol + " : " + e.Entity.Price);
                }
                var changedEntity = e.Entity;
                _hubContext.Clients.All.SendAsync ("UpdateStocks", e.Entity);
            } else {
                _logger.Error ("ChangeType.None: Changed stock price detected: " + e.Entity.Symbol + " : " + e.Entity.Price + ", old: " + e.EntityOldValues.Price);
            }
        }

        private void TableDependency_OnError (object sender, ErrorEventArgs e) {
                _logger.Error ($"SqlTableDependency error: {e.Error.Message}");
                _hubContext.Clients.All.SendAsync ("UpdateStocksError", e.Error);
            }

            #region IDisposable

            ~StockDatabaseSubscription () {
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