/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Aeter.Ratio.Collections;
using Aeter.Ratio.Data.Tracking;
using Aeter.Ratio.Db.Linq;

namespace Aeter.Ratio.Db
{
    public class EnigmaQueryableSet<TEntity> : IQueryableSet<TEntity>
    {
        private readonly IEntityChangeTracker _changeTracker;
        private readonly IKeyExtractor _keyExtractor;
        private readonly IQueryProvider _queryProvider;

        public EnigmaQueryableSet(IEntityChangeTracker changeTracker, IKeyExtractor keyExtractor, EnigmaQueryExecutor queryExecutor)
        {
            _changeTracker = changeTracker;
            _keyExtractor = keyExtractor;
            _queryProvider = new EnigmaQueryProvider(queryExecutor);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        Type IQueryable.ElementType => typeof (TEntity);
        Expression IQueryable.Expression => Expression.Constant(this);
        IQueryProvider IQueryable.Provider => _queryProvider;

        public void Add(TEntity entity)
        {
            var key = _keyExtractor.ExtractValue(entity);
            _changeTracker.TrackNew(key, entity);
        }

        public void Remove(TEntity entity)
        {
            var key = _keyExtractor.ExtractValue(entity);
            _changeTracker.TrackRemoved(key, entity);
        }

        public TEntity Get(object key)
        {
            //_changeTracker.Track(key, entity);
            throw new NotImplementedException();
        }

        public bool TryGet(object key, out TEntity entity)
        {
            //_changeTracker.Track(key, entity);
            throw new System.NotImplementedException();
        }
    }
}
