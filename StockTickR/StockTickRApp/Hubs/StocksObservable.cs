using System;
using System.Collections.Generic;
using Serilog;
using StockTickR.Models;

namespace StockTickRApp.Hubs {
    public class ObservableWrapper<T> : IObservable<T> {
        private List<IObserver<T>> observers;
        public ObservableWrapper () => observers = new List<IObserver<T>> ();
        public IDisposable Subscribe (IObserver<T> observer) {
            Log.Information ("Subscribing: " + observer);
            if (!observers.Contains (observer)) {
                Log.Information ("Adding observer: " + observer);
                observers.Add (observer);
                Log.Information ("Number of observers: " + observers.ToArray ().Length);
            }
            return new Unsubscriber<T> (observers, observer);
        }
        public void OnNext (T entity) {
            var numberOfObservers = observers.ToArray ().Length;
            if (numberOfObservers == 0) {
                Log.Error ("Number of observers: " + numberOfObservers);
            } else {
                Log.Information ("Number of observers: " + numberOfObservers);
            }

            foreach (var observer in observers) {
                observer.OnNext (entity);
            }
        }
        public void MarketClosed () {
            foreach (var observer in observers) {
                observer.OnCompleted ();
            }
            observers.Clear ();
        }
    }
    internal class Unsubscriber<T> : IDisposable {
        private readonly List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        internal Unsubscriber (List<IObserver<T>> observers, IObserver<T> observer) {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose () {
            if (_observers.Contains (_observer)) {
                _observers.Remove (_observer);
            }
        }
    }
}