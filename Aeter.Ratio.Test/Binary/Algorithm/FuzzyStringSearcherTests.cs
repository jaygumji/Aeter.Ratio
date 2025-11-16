using System.Linq;
using Aeter.Ratio.Binary.Algorithm;
using Xunit;

namespace Aeter.Ratio.Test.Binary.Algorithm
{
    public class FuzzyStringSearcherTests
    {
        [Fact]
        public void Search_ReturnsWordsStartingWithTerm()
        {
            var storage = new StringStorage();
            storage.Add("hello world");
            storage.Add("helpful words");
            var searcher = new FuzzyStringSearcher(storage);

            var matches = searcher.Search("hel", tolerance: 1).ToList();

            Assert.Equal(2, matches.Count);
            Assert.All(matches, match => Assert.StartsWith("he", match.Word));
        }

        [Fact]
        public void Search_RespectsTolerance()
        {
            var storage = new StringStorage();
            storage.Add("alpha beta gamma");
            var searcher = new FuzzyStringSearcher(storage);

            var matches = searcher.Search("alhpa", tolerance: 1).ToList();

            Assert.Single(matches);
            Assert.Equal("alpha", matches[0].Word);
            Assert.Equal(1, matches[0].Distance);
        }

        [Fact]
        public void Search_CanMatchInsideWords()
        {
            var storage = new StringStorage();
            storage.Add("concatenate");
            var searcher = new FuzzyStringSearcher(storage);

            var matches = searcher.Search("*cat", tolerance: 0).ToList();

            Assert.Single(matches);
            Assert.Equal("concatenate", matches[0].Source);
            Assert.Equal("concatenate", matches[0].Word);
        }
    }
}
