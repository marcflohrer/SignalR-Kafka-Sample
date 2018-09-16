using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using StockProcessor.Models;

namespace StockTickR.Clients {
    public class StockClient {
        private readonly HttpClient _client;
        readonly MediaTypeWithQualityHeaderValue _mediaType = new MediaTypeWithQualityHeaderValue ("application/json");
        private readonly Dictionary<string, decimal> cache = new Dictionary<string, decimal> ();

        private readonly string topicName = "Stocks";
        Producer<string, string> producer;

        public StockClient () {
            _client = new HttpClient {
                BaseAddress = new Uri ("http://stockprocessor:8082/")
            };
            _client.DefaultRequestHeaders.Accept.Clear ();
            _client.DefaultRequestHeaders.Accept.Add (_mediaType);

            string[] brokerList = {
                "http://stocktickr_kafka1_1:9092/",
                "http://stocktickr_kafka2_1:9092/",
                "http://stocktickr_kafka3_1:9092/"
                };

            producer = new Producer<string, string> (
                new Dictionary<string, object> { { "bootstrap.servers", brokerList } },
                new StringSerializer (Encoding.UTF8), new StringSerializer (Encoding.UTF8)
                );
        }

        public IEnumerable<Stock> Get () {
            var response = _client.GetAsync ("stocks/").GetAwaiter ().GetResult ();
            Console.WriteLine (DateTime.Now + " Get /stocks: " + response.Content.ReadAsStringAsync ().GetAwaiter ().GetResult ());
            response.EnsureSuccessStatusCode ();
            return response.Content.ReadAsAsync<List<Stock>> ().GetAwaiter ().GetResult ();
        }

        public ErrorCode[] AddRange (IEnumerable<Stock> stocks) {
            ErrorCode[] result = new ErrorCode[stocks.Count<Stock> ()];
            List<Stock> stocksThatChanged = FindStocksThatChanged (stocks);
            if (stocksThatChanged.Any ()) {
                int i = 0;
                foreach (var stock in stocks) {
                    var deliveryReport = producer.ProduceAsync (topicName, stock.Symbol, JsonConvert.SerializeObject (stock, typeof (Stock), new JsonSerializerSettings () {
                        Culture = CultureInfo.InvariantCulture
                    })).GetAwaiter ().GetResult ();
                    result[i++] = deliveryReport.Error.Code;
                    Console.WriteLine (
                        deliveryReport.Error.Code == ErrorCode.NoError
                            ? $"delivered to: {deliveryReport.TopicPartitionOffset}"
                            : $"failed to deliver message: {deliveryReport.Error.Reason}"
                    );
                    if (deliveryReport.Error.Code == ErrorCode.NoError) {
                        UpdateCache (stocksThatChanged);
                    }
                }
            }
            return result;
        }

        private void WatchOneStock (Stock stock) {
            var stockToWatch = Environment.GetEnvironmentVariable ("STOCK_TO_WATCH") ?? "Acme Inc.";
            if (stock.Symbol == stockToWatch) {
                Console.WriteLine (DateTime.Now + " [Information] " + stock.Symbol + " : " + stock.Price);
            }
        }

        private HttpResponseMessage PostAsJsonUntilDbIsReadyAsync (List<Stock> stocksThatChanged, HttpResponseMessage response) {
            while (response == null || response.StatusCode != HttpStatusCode.OK) {
                try {
                    response = _client.PostAsJsonAsync ("stocks/", stocksThatChanged).GetAwaiter ().GetResult ();
                    if (response.StatusCode != HttpStatusCode.OK) {
                        Console.WriteLine (DateTime.Now + " [Error] StockClient.AddRange: " + response.Content + " : " + response.StatusCode);
                    }
                } catch (Exception ex) {
                    Console.WriteLine (DateTime.Now + " [Error] StockClient.AddRange: " + ex.Message + "\n" + ex.StackTrace);
                    WaitForMsSqlServerToBoot ();
                }
            }

            return response;
        }

        private static void WaitForMsSqlServerToBoot () {
            Thread.Sleep (20000);
        }

        private List<Stock> FindStocksThatChanged (IEnumerable<Stock> stocks) {
            var stocksThatChanged = new List<Stock> ();

            foreach (var stock in stocks) {
                if (IsChanged (stock)) {
                    stocksThatChanged.Add (stock);
                }
            }

            return stocksThatChanged;
        }

        private void UpdateCache (IEnumerable<Stock> stocks) {
            foreach (var stock in stocks) {
                if (cache.ContainsKey (stock.Symbol)) {
                    try {
                        cache.Remove (stock.Symbol);
                    } catch (Exception ex) {
                        Console.WriteLine (DateTime.Now + " [Error] StockClient.UpdateCache (Remove(key)): " + ex.Message);
                    }
                }
                try {
                    cache.Add (stock.Symbol, stock.Price);
                } catch (Exception ex) {
                    Console.WriteLine (DateTime.Now + " [Error] StockClient.UpdateCache (Add(key): " + ex.Message);
                }

            }
        }

        private bool IsChanged (Stock stock) {
            if (cache.ContainsKey (stock.Symbol)) {
                return cache[stock.Symbol] != stock.Price;
            } else {
                return true;
            }
        }
    }
}