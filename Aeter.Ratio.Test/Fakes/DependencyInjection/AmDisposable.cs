﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Test.Fakes.DependencyInjection
{
    public class AmDisposable : IDisposable
    {
        public int DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled++;
        }
    }
}
