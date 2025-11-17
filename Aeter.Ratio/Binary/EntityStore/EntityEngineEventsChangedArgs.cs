/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    public class EntityEngineEventsChangedArgs(Guid entityId, object? entity, ReadOnlyMemory<byte> payload, EntityChangeType changeType) : AsyncEventArgs
    {
        public Guid EntityId { get; } = entityId;
        public object? Entity { get; } = entity;
        public ReadOnlyMemory<byte> Payload { get; } = payload;
        public EntityChangeType ChangeType { get; } = changeType;
    }
}
