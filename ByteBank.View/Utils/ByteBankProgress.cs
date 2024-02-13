using System;
using System.Threading;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
    public class ByteBankProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        private readonly TaskScheduler _scheduler;

        public ByteBankProgress(Action<T> value)
        {
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _handler = value;
        }

        public void Report(T value)
        {
            Task.Factory.StartNew(() =>
                _handler(value),
                CancellationToken.None,
                TaskCreationOptions.None,
                _scheduler
            );
        }
    }
}
