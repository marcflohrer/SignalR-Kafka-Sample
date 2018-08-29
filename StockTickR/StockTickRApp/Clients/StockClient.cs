using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using StockTickR.Models;

namespace StockTickR.Clients {
    public class StockClient {

        public StockClient (Uri connection) {
            Connection = connection;
        }

        private void InitHttpClient () {
            _httpClient = new HttpClient {
                BaseAddress = Connection
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear ();
            _httpClient.DefaultRequestHeaders.Accept.Add (_mediaType);
        }

        private HttpClient _httpClient;
        MediaTypeWithQualityHeaderValue _mediaType = new MediaTypeWithQualityHeaderValue ("application/json");

        public Uri Connection {
            get;
        }

        public async Task<IEnumerable<Stock>> GetAllStocks () {
            if (_httpClient == null) {
                InitHttpClient ();
            }
            var response = await _httpClient.GetAsync ("stocks/");
            response.EnsureSuccessStatusCode ();
            return await response.Content.ReadAsAsync<List<Stock>> ();
        }
    }
}