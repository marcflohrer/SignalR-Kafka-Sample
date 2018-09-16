using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Serilog;
using StockTickR.Clients;
using StockTickR.Models;
using StockTickRApp.Hubs;

namespace StockTickR.Hubs {
    public class StockTickerHub : Hub {
        private MarketState MarketState { get; set; }
        private CancellationTokenSource cancelToken = new CancellationTokenSource ();

        public IHubContext<StockTickerHub> Hub { get; }
        public StockClient StockClient { get; }
        private ILogger _logger { get; }

        public HubConnection StockHubConnection { get; }
        static ObservableWrapper<Stock> StocksObservable = new ObservableWrapper<Stock> ();

        public ChannelReader<Stock> StreamStocks () => StocksObservable.AsChannelReader (10);

        public StockTickerHub (StockHubConnection stockHubConnection, IHubContext<StockTickerHub> hub, StockClient stockClient, ILogger logger) {
            StockHubConnection = stockHubConnection.HubConnection;
            Hub = hub;
            StockClient = stockClient;
            _logger = logger;
            var conf = new Dictionary<string, object> { { "group.id", "test-consumer-group" },
                    { "bootstrap.servers", "stocktickr_kafka1_1:9092" },
                    { "auto.commit.interval.ms", 5000 },
                    { "auto.offset.reset", "earliest" }
                };

            using (var consumer = new Consumer<string, Stock> (conf, new KeyDeserializer(), new StockDeserializer())) {
                consumer.OnMessage += (_, msg) => OnMessage (msg);
                consumer.OnError += (_, error) => OnError (error);
                consumer.OnConsumeError += (_, msg) => OnConsumeError (msg);
                consumer.Subscribe ("Stocks");
            }        
        }

        private static void OnConsumeError (Message msg) {
            Console.WriteLine ($"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");
        }

        private static void OnError (Error error) {
            Console.WriteLine ($"Error: {error}");
        }

        private static void OnMessage (Message<string, Stock> msg) {
            Console.WriteLine ($"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
            StocksObservable.OnNext (msg.Value);
        }

        public string GetMarketState () => MarketState.ToString ();

        public IEnumerable<Stock> GetAllStocks () {
            return StockClient.GetAllStocks ().GetAwaiter ().GetResult ();
        }

        public async Task OpenMarket () {
            MarketState = MarketState.Open;
            await Hub.Clients.All.SendAsync ("marketOpened");
            var pauseBetweenKeepAlive = TimeSpan.FromSeconds (10);
            StocksObservable = new ObservableWrapper<Stock> ();
            await StockHubConnection.StartAsync (cancelToken.Token);
            StockHubConnection.On<Stock> ("UpdateStocks", (stock) => {
                WatchOneStock (stock);
                StocksObservable.OnNext (stock);
            });

            StockHubConnection.On<Exception> ("UpdateStocksError", (ex) => {
                _logger.Error (ex, "Error receiving 'UpdateStocks' stream: ");
            });

            StockHubConnection.Closed += ex => {
                _logger.Error (ex, "An error occurred receiving entities of type Stock: {0}");
                StockHubConnection.StopAsync ();
                KeepAlive (pauseBetweenKeepAlive, _logger);
                return Task.CompletedTask;
            };
            KeepAlive (pauseBetweenKeepAlive, _logger);
        }

        private void KeepAlive (TimeSpan pauseBetweenKeepAlive, ILogger logger) {
            StockHubConnection.StartAsync ().ContinueWith (
                continuationAction: task => {
                    logger.Information ("KeepLive: " + task.Status.ToString ());
                    if (task.IsFaulted || task.IsCanceled) {
                        logger.Information ("Connection reconnect");
                        Task.Delay (pauseBetweenKeepAlive).ContinueWith (t => {
                            KeepAlive (pauseBetweenKeepAlive, logger);
                        });
                    }
                }
            );
        }

        public async Task CloseMarket () {
            cancelToken.Cancel ();
            await Hub.Clients.All.SendAsync ("marketClosed");
        }

        private void WatchOneStock (Stock stock) {
            var stockNameToWatch = Environment.GetEnvironmentVariable ("STOCK_TO_WATCH") ?? "Acme Inc.";
            if (stock.Symbol == stockNameToWatch) {
                _logger.Information ("[Create] " + stock.Symbol + " : " + stock.Price + ", id = " + stock.Id + ", " + stock.Change + ", " + stock.DayHigh + ", " + stock.DayLow + ", " + stock.DayLow + ", " + stock.LastChange + ", " + stock.PercentChange);
            }
        }
    }

    public enum MarketState {
        Closed,
        Open
    }

    public class StockDeserializer : IDeserializer<Stock>
    {
        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public Stock Deserialize(string topic, byte[] bytes)
        {
            return JsonConvert.DeserializeObject<Stock>(Encoding.UTF8.GetString(bytes));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StockDeserializer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class KeyDeserializer : IDeserializer<String>
    {
        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public String Deserialize(string topic, byte[] bytes)
        {
            return JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(bytes));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StockDeserializer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
