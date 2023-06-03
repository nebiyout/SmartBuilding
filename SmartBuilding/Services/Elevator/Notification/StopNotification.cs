using SmartBuilding.Contracts;
using SmartBuilding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator
{
    public class StopNotification : IObserver<bool>
    {
        public void OnNext(bool value)
        {
            if (value)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Elevator Stopped");
            }
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"An error occurred: {error.Message}");
        }

        public void OnCompleted()
        {
            Console.WriteLine("Completed");
        }
    }
}
