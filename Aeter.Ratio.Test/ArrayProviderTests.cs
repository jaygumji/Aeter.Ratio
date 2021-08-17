/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Aeter.Ratio.Test
{
    public class ArrayProviderTests
    {
        [Fact]
        public void FromEmptyCollectionTest()
        {
            var target = new List<int>();
            var actual = ArrayProvider.ToArray(target);

            Assert.Empty(actual);
        }

        [Fact]
        public void FromCollectionTest()
        {
            var target = new List<int> { 1, 2, 3 };
            var actual = ArrayProvider.ToArray(target);

            Assert.Equal(3, actual.Length);
            Assert.True(target.SequenceEqual(actual));
        }

        [Fact]
        public void FromEmpty2DCollectionTest()
        {
            var target = new List<ICollection<int>>();
            var actual = ArrayProvider.To2DArray(target);

            Assert.Equal(2, actual.Rank);
            Assert.Equal(0, actual.GetLength(0));
            Assert.Equal(0, actual.GetLength(1));
        }

        [Fact]
        public void From2DCollectionTest()
        {
            var target = new List<ICollection<int>> { new List<int> { 1, 2 }, new List<int> { 3, 4 }, new List<int> { 5, 6 } };
            var actual = ArrayProvider.To2DArray(target);

            Assert.Equal(2, actual.Rank);
            Assert.Equal(3, actual.GetLength(0));
            Assert.Equal(2, actual.GetLength(1));

            int r0 = 0, r1 = 0;
            foreach (var c0 in target) {
                foreach (var value in c0) {
                    var actualValue = actual[r0, r1];
                    Assert.Equal(value, actualValue); //, string.Format("Value at [{0}, {1}] was not expected, expected {2} but found {3}", r0, r1, value, actualValue));
                    r1++;
                }
                r0++;
                r1 = 0;
            }
        }

        [Fact]
        public void FromEmpty3DCollectionTest()
        {
            var target = new List<ICollection<ICollection<int>>>();
            var actual = ArrayProvider.To3DArray(target);

            Assert.Equal(3, actual.Rank);
            Assert.Equal(0, actual.GetLength(0));
            Assert.Equal(0, actual.GetLength(1));
            Assert.Equal(0, actual.GetLength(2));
        }

        [Fact]
        public void From3DCollectionTest()
        {
            var target = new List<ICollection<ICollection<int>>> {
                new List<ICollection<int>> {
                    new List<int> {10, 11, 12, 13},
                    new List<int> {14, 15, 16, 17},
                    new List<int> {18, 19, 20, 21},
                    new List<int> {22, 23, 24, 25},
                    new List<int> {26, 27, 28, 29},
                    new List<int> {30, 31, 32, 33}
                },
                new List<ICollection<int>> {
                    new List<int> {34, 35, 36, 37},
                    new List<int> {38, 39, 40, 41},
                    new List<int> {42, 43, 44, 45},
                    new List<int> {46, 47, 48, 49},
                    new List<int> {50, 51, 52, 53},
                    new List<int> {54, 55, 56, 57}
                },
                new List<ICollection<int>> {
                    new List<int> {58, 59, 60, 61},
                    new List<int> {62, 63, 64, 65},
                    new List<int> {66, 67, 68, 69},
                    new List<int> {70, 71, 72, 73},
                    new List<int> {74, 75, 76, 77},
                    new List<int> {78, 79, 80, 81}
                }
            };
            var actual = ArrayProvider.To3DArray(target);

            Assert.Equal(3, actual.Rank);
            Assert.Equal(3, actual.GetLength(0));
            Assert.Equal(6, actual.GetLength(1));
            Assert.Equal(4, actual.GetLength(2));

            int r0 = 0, r1 = 0, r2 = 0;
            foreach (var c0 in target) {
                foreach (var c1 in c0) {
                    foreach (var value in c1) {
                        var actualValue = actual[r0, r1, r2];
                        Assert.Equal(value, actualValue); //, string.Format("Value at [{0}, {1}, {2}] was not expected, expected {3} but found {4}", r0, r1, r2, value, actualValue));
                        r2++;
                    }
                    r1++;
                    r2 = 0;
                }
                r0++;
                r1 = 0;
            }
        }

    }
}
