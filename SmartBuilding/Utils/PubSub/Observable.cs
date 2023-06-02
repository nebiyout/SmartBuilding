using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Utils.PubSub
{
    public class Observable<T> : IObservable<T>
    {
        private List<IObserver<T>> observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        public void Notify(T value)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(value);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
