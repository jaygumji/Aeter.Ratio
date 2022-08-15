/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Test.Serialization.Bson
{
    public class SmallBlock
    {
        public Guid Id { get; set; }
        public int No { get; set; }
        public string? Category { get; set; }

        public static SmallBlock Filled()
        {
            return new SmallBlock {
                Id = Guid.NewGuid(),
                No = 1,
                Category = "Mini"
            };
        }
    }
}