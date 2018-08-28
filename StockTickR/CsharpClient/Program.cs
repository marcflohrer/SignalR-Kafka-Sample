using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CsharpClient {
    /// <remarks><attribution license="cc4" from="Microsoft" modified="false" /><para>Languages typically do not require a class to declare inheritance from <see cref="T:System.Object" /> because the inheritance is implicit.</para><para>Because all classes in the .NET Framework are derived from <see cref="T:System.Object" />, every method defined in the <see cref="T:System.Object" /> class is available in all objects in the system. Derived classes can and do override some of these methods, including: </para><list type="bullet"><item><para><see cref="M:System.Object.Equals(System.Object)" /> - Supports comparisons between objects.</para></item><item><para><see cref="M:System.Object.Finalize" /> - Performs cleanup operations before an object is automatically reclaimed.</para></item><item><para><see cref="M:System.Object.GetHashCode" /> - Generates a number corresponding to the value of the object to support the use of a hash table.</para></item><item><para><see cref="M:System.Object.ToString" /> - Manufactures a human-readable text string that describes an instance of the class.</para></item></list><format type="text/html"><h2>Performance Considerations</h2></format><para>If you are designing a class, such as a collection, that must handle any type of object, you can create class members that accept instances of the <see cref="T:System.Object" /> class. However, the process of boxing and unboxing a type carries a performance cost. If you know your new class will frequently handle certain value types you can use one of two tactics to minimize the cost of boxing. </para><list type="bullet"><item><para>Create a general method that accepts an <see cref="T:System.Object" /> type, and a set of type-specific method overloads that accept each value type you expect your class to frequently handle. If a type-specific method exists that accepts the calling parameter type, no boxing occurs and the type-specific method is invoked. If there is no method argument that matches the calling parameter type, the parameter is boxed and the general method is invoked. </para></item><item><para>Design your type and its members to use generics. The common language runtime creates a closed generic type when you create an instance of your class and specify a generic type argument. The generic method is type-specific and can be invoked without boxing the calling parameter. </para></item></list><para>Although it is sometimes necessary to develop general purpose classes that accept and return <see cref="T:System.Object" /> types, you can improve performance by also providing a type-specific class to handle a frequently used type. For example, providing a class that is specific to setting and getting Boolean values eliminates the cost of boxing and unboxing Boolean values.</para></remarks><summary><attribution license="cc4" from="Microsoft" modified="false" /><para>Supports all classes in the .NET Framework class hierarchy and provides low-level services to derived classes. This is the ultimate base class of all classes in the .NET Framework; it is the root of the type hierarchy.</para></summary>
    public static class Program {
#pragma warning disable RECS0154 // Parameter wird niemals verwendet.
        static async Task Main (string[] args)
#pragma warning restore RECS0154 // Parameter wird niemals verwendet.
        {
            var connection = new HubConnectionBuilder ()
                .WithUrl ("http://stocktickr:8081/stocks")
                .ConfigureLogging (logging => {
                    logging.AddConsole ();
                })
                .AddMessagePackProtocol ()
                .Build ();

            await connection.StartAsync ();

            Console.WriteLine ("[Info] " + DateTime.Now + " Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource ();
            Console.CancelKeyPress += (sender, a) => {
                a.Cancel = true;
                cts.Cancel ();
            };

            connection.Closed += e => {
                Console.WriteLine ("[Info] " + DateTime.Now + " Connection closed with error: {0}", e);

                cts.Cancel ();
                return Task.CompletedTask;
            };

            connection.On ("marketOpened", () => {
                Console.WriteLine ("[Info] " + DateTime.Now + " Market opened");
            });

            connection.On ("marketClosed", () => {
                Console.WriteLine ("[Info] " + DateTime.Now + " Market closed");
            });

            connection.On ("marketReset", () => {
                // We don't care if the market rest
            });

            var channel = await connection.StreamAsChannelAsync<Stock> ("StreamStocks", CancellationToken.None);
            while (await channel.WaitToReadAsync () && !cts.IsCancellationRequested) {
                while (channel.TryRead (out var stock)) {
                    Console.WriteLine ($"[Info] {stock.Symbol} {stock.Price}");
                }
            }
        }
    }

    public class Stock {
        public int Id {
            get; set;
        }

        public string Symbol {
            get; set;
        }

        public decimal DayOpen {
            get; private set;
        }

        public decimal DayLow {
            get; private set;
        }

        public decimal DayHigh {
            get; private set;
        }

        public decimal LastChange {
            get; private set;
        }

        public decimal Change {
            get; set;
        }

        public double PercentChange {
            get; set;
        }

        public decimal Price {
            get; set;
        }
    }
}