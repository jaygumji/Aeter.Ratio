/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//using System.IO;
//using System.Linq;
//using Aeter.Ratio.IO;
//using Xunit;

//namespace Aeter.Ratio.Test
//{
//    public class DualBinaryStoreTests
//    {

//        [Fact]
//        public void ReadWriteTest()
//        {
//            var leftTestData = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
//            var rightTestData = new byte[] {1, 3, 3, 7};

//            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.dat");
//            if (File.Exists(path)) File.Delete(path);

//            long leftOffset;
//            long rightOffset;
//            using (var store = new DualBinaryStore(new MemoryStreamProvider(path), 0, 1024 * 1024))
//            {
//                Assert.True(store.Left.TryWrite(leftTestData, out leftOffset));
//                Assert.True(store.Right.TryWrite(rightTestData, out rightOffset));

//                var leftData = store.ReadLeft(leftOffset, leftTestData.Length);
//                Assert.True(leftTestData.SequenceEqual(leftData));

//                var rightData = store.ReadRight(rightOffset, rightTestData.Length);
//                Assert.True(rightTestData.SequenceEqual(rightData));
//            }

//            using (var store = new DualBinaryStore(new FileSystemStreamProvider(path), 0, 1024 * 1024))
//            {
//                var leftData = store.ReadLeft(leftOffset, leftTestData.Length);
//                Assert.True(leftTestData.SequenceEqual(leftData));

//                var rightData = store.ReadRight(rightOffset, rightTestData.Length);
//                Assert.True(rightTestData.SequenceEqual(rightData));
//            }
//        }

//    }
//}
