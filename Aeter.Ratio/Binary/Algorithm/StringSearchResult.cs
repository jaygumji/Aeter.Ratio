namespace Aeter.Ratio.Binary.Algorithm
{
    public sealed class StringSearchResult
    {
        public StringSearchResult(string source, string word, int wordIndex, int distance)
        {
            Source = source;
            Word = word;
            WordIndex = wordIndex;
            Distance = distance;
        }

        public string Source { get; }
        public string Word { get; }
        public int WordIndex { get; }
        public int Distance { get; }
    }
}
