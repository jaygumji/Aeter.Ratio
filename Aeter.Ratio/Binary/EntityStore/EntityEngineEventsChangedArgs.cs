/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    public class EntityEngineEventsChangedArgs(object entity, BinaryReadBuffer buffer, EntityChangeType changeType) : AsyncEventArgs
    {
        public object Entity { get; } = entity;
        public BinaryReadBuffer Buffer { get; } = buffer;
        public EntityChangeType ChangeType { get; } = changeType;
    }
}
