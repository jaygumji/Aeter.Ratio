﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;

namespace Aeter.Ratio.Db.Modelling
{
    public class EnigmaSchemaEntityIndex
    {
        public string Name { get; set; } = string.Empty;
        public List<int> FieldIndexPath { get; set; } = new List<int>();
    }
}