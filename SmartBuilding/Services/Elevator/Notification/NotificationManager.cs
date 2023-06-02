using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core.Dto;
using SmartBuilding.Utils.PubSub;
using SmartBuilding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator.Notification
{
    public static class NotificationManager<T> where T : class
    {
        private static List<Observable<T>> observables = new List<Observable<T>>();

        private static Observable<T> GetObservable()
        {
            var observable = observables.FirstOrDefault<Observable<T>>();

            if (observable == null)
            {
                observable = Activator.CreateInstance<Observable<T>>();
                observables.Add(observable);
            }

            return observable;
        }

        public static void Subscribe<K>(K k) where K : IObserver<T>
        {
            var observable = GetObservable();

            if (observable.Observers.Any(i=>i.GetType().Equals(k.GetType())))
                return;

            observable.Subscribe(k);
        }

        public static void Notify(T t)
        {
            var observable = GetObservable();
            observable.Notify(t);
        }
    }
}
