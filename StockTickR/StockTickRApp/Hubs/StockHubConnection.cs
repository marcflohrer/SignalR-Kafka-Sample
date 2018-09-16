using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace StockTickRApp.Hubs {
    public class StockHubConnection {
        public HubConnection HubConnection {
            get;
        }
        public StockHubConnection(Uri uri) => HubConnection = new HubConnectionBuilder()
                .WithUrl(CreateHubUri(uri))
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

        private static string CreateHubUri (Uri connection) {
            UriBuilder uri = new System.UriBuilder (uri: connection);
            uri.Path += "stockshub/";
            var hubUri = uri.ToString ();
            return hubUri;
        }

    }
}