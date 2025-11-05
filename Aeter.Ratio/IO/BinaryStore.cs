using System;
using System.IO;

namespace Aeter.Ratio.IO
{
    public abstract class DataStore : IDisposable
    {
        private readonly InstancePool<IDataReader> _readers;
        private readonly InstancePool<IDataWriter> _writers;

        public DataStore() {
            _readers = new InstancePool<IDataReader>(CreateReader);
            _writers = new InstancePool<IDataWriter>(CreateWriter);
        }

        private IDataReader CreateReader(object? state) => OnCreateReader();
        protected abstract IDataReader OnCreateReader();

        private IDataWriter CreateWriter(object? state) => OnCreateWriter();
        protected abstract IDataWriter OnCreateWriter();

        public void Dispose()
        {
        }
    }
}
