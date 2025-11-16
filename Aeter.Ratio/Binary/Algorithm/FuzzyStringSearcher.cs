using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Binary.Algorithm
{
    /// <summary>
    /// Provides fuzzy prefix/substring search with configurable tolerance over a <see cref="StringStorage"/>.
    /// </summary>
    public sealed class FuzzyStringSearcher
    {
        private readonly StringStorage storage;

        public FuzzyStringSearcher(StringStorage storage)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public IEnumerable<StringSearchResult> Search(string searchText, int tolerance = 1)
        {
            if (string.IsNullOrWhiteSpace(searchText)) yield break;
            var query = FuzzySearchQuery.Parse(searchText);
            foreach (var entry in storage.Entries) {
                foreach (var (word, index) in Tokenize(entry)) {
                    if (query.Term.Length == 0) {
                        yield return new StringSearchResult(entry, word, index, 0);
                        continue;
                    }

                    if (query.SearchInsideWord) {
                        if (MatchesSubstring(word, query.Term, tolerance, out var distance)) {
                            yield return new StringSearchResult(entry, word, index, distance);
                        }
                        continue;
                    }

                    if (MatchesPrefix(word, query.Term, tolerance, out var prefixDistance)) {
                        yield return new StringSearchResult(entry, word, index, prefixDistance);
                    }
                }
            }
        }

        private static bool MatchesPrefix(string word, string term, int tolerance, out int distance)
        {
            if (term.Length == 0) {
                distance = 0;
                return true;
            }

            if (word.Length == 0) {
                distance = term.Length;
                return distance <= tolerance;
            }

            var compareLength = Math.Min(word.Length, term.Length + tolerance);
            var candidate = word.AsSpan(0, compareLength);
            distance = LevenshteinDistance.Compute(term.AsSpan(), candidate, tolerance);
            return distance <= tolerance;
        }

        private static bool MatchesSubstring(string word, string term, int tolerance, out int distance)
        {
            if (term.Length == 0) {
                distance = 0;
                return true;
            }

            if (word.Length == 0) {
                distance = term.Length;
                return distance <= tolerance;
            }

            var termSpan = term.AsSpan();
            var wordSpan = word.AsSpan();
            var minWindow = Math.Max(1, term.Length - tolerance);
            var maxWindow = term.Length + tolerance;

            for (var start = 0; start < word.Length; start++) {
                var remaining = word.Length - start;
                var maxLength = Math.Min(remaining, maxWindow);
                var minLength = Math.Min(remaining, minWindow);
                for (var length = minLength; length <= maxLength; length++) {
                    var candidate = wordSpan.Slice(start, length);
                    var current = LevenshteinDistance.Compute(termSpan, candidate, tolerance);
                    if (current <= tolerance) {
                        distance = current;
                        return true;
                    }
                }
            }

            distance = tolerance + 1;
            return false;
        }

        private static IEnumerable<(string Word, int Index)> Tokenize(string text)
        {
            var start = -1;
            for (var i = 0; i < text.Length; i++) {
                if (char.IsLetterOrDigit(text[i])) {
                    if (start < 0) start = i;
                    continue;
                }

                if (start >= 0) {
                    yield return (text.Substring(start, i - start), start);
                    start = -1;
                }
            }

            if (start >= 0) {
                yield return (text.Substring(start, text.Length - start), start);
            }
        }

        private readonly struct FuzzySearchQuery
        {
            private FuzzySearchQuery(string term, bool searchInsideWord)
            {
                Term = term;
                SearchInsideWord = searchInsideWord;
            }

            public string Term { get; }
            public bool SearchInsideWord { get; }

            public static FuzzySearchQuery Parse(string input)
            {
                var searchInside = input.StartsWith("*", StringComparison.Ordinal);
                var term = searchInside ? input[1..] : input;
                return new FuzzySearchQuery(term, searchInside);
            }
        }
    }
}
