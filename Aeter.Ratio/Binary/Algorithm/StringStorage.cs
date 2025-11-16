using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Binary.Algorithm
{
    /// <summary>
    /// Simple in-memory store for text documents that exposes the contents for fast string searching.
    /// </summary>
    public sealed class StringStorage
    {
        private readonly List<string> entries = new();

        public IReadOnlyList<string> Entries => entries;

        public void Add(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            entries.Add(text);
        }
    }
}
