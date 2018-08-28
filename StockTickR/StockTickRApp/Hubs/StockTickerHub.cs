using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
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
                if (stock.Symbol == "Apple") {
                    _logger.Information ("OnNext: " + stock.Symbol + " : " + stock.Price);
                }
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
    }

    public enum MarketState {
        Closed,
        Open
    }
}