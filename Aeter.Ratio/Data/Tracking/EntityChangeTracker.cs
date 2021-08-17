/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;

namespace Aeter.Ratio.Data.Tracking
{
    public interface IEntityChangeTracker
    {
        IEnumerable<IEntityEntry> Entries { get; }
        void TrackNew<T>(object key, T entity);
        void Track<T>(object key, T entity);
        void TrackRemoved<T>(object key, T entity);
        void TrackRemoved<T>(object key);
    }

    public class EntityChangeTracker : IEntityChangeTracker
    {
        private readonly Dictionary<object, IEntityEntry> _entries;

        public EntityChangeTracker()
        {
            _entries = new Dictionary<object, IEntityEntry>();
        }

        public IEnumerable<IEntityEntry> Entries => _entries.Values;

        public void TrackNew<T>(object key, T entity)
        {
            var entry = new EntityEntry<T>(key, entity, EntityState.New);
            _entries.Add(key, entry);
        }

        public void Track<T>(object key, T entity)
        {
            var entry = new EntityEntry<T>(key, entity, EntityState.Intact);
            _entries.Add(key, entry);
        }

        public void TrackRemoved<T>(object key, T entity)
        {
            IEntityEntry entry;
            if (_entries.TryGetValue(key, out entry)) {
                var editable = (IEditableEntityEntry) entry;
                editable.ModifyState(EntityState.Removed);
            }
            else {
                entry = new EntityEntry<T>(key, entity, EntityState.Removed);
                _entries.Add(key, entry);
            }
        }

        public void TrackRemoved<T>(object key)
        {
            TrackRemoved(key, default(T));
        }
    }
}