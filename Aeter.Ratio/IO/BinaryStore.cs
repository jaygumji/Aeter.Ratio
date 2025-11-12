using System;
using System.IO;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Base class for stores that cache reusable <see cref="IDataReader"/> and <see cref="IDataWriter"/> instances.
    /// </summary>
    public abstract class DataStore : IDisposable
    {
        private readonly InstancePool<IDataReader> _readers;
        private readonly InstancePool<IDataWriter> _writers;

        /// <summary>
        /// Initializes internal pools used to cache reader and writer instances.
        /// </summary>
        public DataStore() {
            _readers = new InstancePool<IDataReader>(CreateReader);
            _writers = new InstancePool<IDataWriter>(CreateWriter);
        }

        private IDataReader CreateReader(object? state) => OnCreateReader();
        /// <summary>
        /// Creates a new reader instance when the pool requires one.
        /// </summary>
        protected abstract IDataReader OnCreateReader();

        private IDataWriter CreateWriter(object? state) => OnCreateWriter();
        /// <summary>
        /// Creates a new writer instance when the pool requires one.
        /// </summary>
        protected abstract IDataWriter OnCreateWriter();

        /// <summary>
        /// Releases any pooled resources. Override to dispose derived state.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
