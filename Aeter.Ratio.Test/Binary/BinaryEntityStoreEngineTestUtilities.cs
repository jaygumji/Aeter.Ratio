/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.EntityStore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aeter.Ratio.Test.Binary
{
    internal static class BinaryEntityStoreEngineTestUtilities
    {
        public static string CreateTempDirectory()
        {
            for (var i = 0; i < 3; i++) {
                var path = Path.Combine(Path.GetTempPath(), "aeter_ratio_engine_" + Guid.NewGuid().ToString("N"));
                try {
                    Directory.CreateDirectory(path);
                    return path;
                }
                catch (UnauthorizedAccessException) when (i < 2) {
                    continue;
                }
            }

            throw new InvalidOperationException("Unable to create temporary working directory for BinaryEntityStoreEngine tests.");
        }

        public static void TryDeleteDirectory(string path)
        {
            try {
                if (Directory.Exists(path)) {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch (UnauthorizedAccessException) {
                // ignored
            }
        }

        public static Task<BinaryEntityStoreEngine> CreateEngineAsync(string workingDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
            var storePath = Path.Combine(workingDirectory, Guid.NewGuid().ToString("N") + ".store");
            return BinaryEntityStoreEngine.CreateAsync(storePath, BinaryBufferPool.Default);
        }
    }
}
