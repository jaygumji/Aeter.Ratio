/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Data.Tracking
{
    /// <summary>
    /// Entity entry representing the current state of the entity
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    /// <seealso cref="IEntityEntry" />
    public class EntityEntry<TEntity> : IEntityEntry, IEditableEntityEntry
    {
        private EntityState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEntry{TEntity}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        public EntityEntry(object key, TEntity entity, EntityState state)
        {
            Key = key;
            Entity = entity;
            _state = state;
            EntityType = typeof (TEntity);
        }

        /// <summary>Gets the key.</summary>
        /// <value>The key.</value>
        public object Key { get; }

        /// <summary>Gets the entity.</summary>
        /// <value>The entity.</value>
        public TEntity Entity { get; }

        public Type EntityType { get; }

        /// <summary>Gets the state.</summary>
        /// <value>The state.</value>
        public EntityState State => _state;

        object IEntityEntry.Entity => Entity;

        void IEditableEntityEntry.ModifyState(EntityState newState)
        {
            _state = newState;
        }
    }
}