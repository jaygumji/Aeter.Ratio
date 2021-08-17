/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Data.Tracking;
using Aeter.Ratio.Db.Modelling;
using System;

namespace Aeter.Ratio.Db
{
    public class UnitOfWork : IEnigmaUnitOfWork, IDisposable
    {
        private readonly IEnigmaConnection _connection;
        private readonly EntityChangeTracker _changeTracker;
        private readonly EnigmaSchemaModel _model;
        private readonly IBinaryConverterProvider _binaryConverterProvider;

        public UnitOfWork(IEnigmaConnection connection)
        {
            _connection = connection;
        }

        protected virtual void OnModelBuilding(IEnigmaSchemaModelBuilder modelBuilder)
        {
        }

        public void CommitChanges()
        {
            var cmd = _connection.CreateCommand();
            foreach (var entry in _changeTracker.Entries) {
                var entityBinaryConverter = _binaryConverterProvider.Get(entry.EntityType);
                var schema = _model.SchemaFor(entry.EntityType);

                if (entry.State == EntityState.New)
                    cmd.Add(schema.Name, buffer => entityBinaryConverter.EntityConverter.Convert(entry.Entity, buffer));
                else if (entry.State == EntityState.Modified)
                    cmd.Modify(schema.Name, buffer => entityBinaryConverter.EntityConverter.Convert(entry.Entity, buffer));
                else
                    cmd.Remove(schema.Name, buffer => entityBinaryConverter.KeyConverter.Convert(entry.Key, buffer));
            }
            (cmd as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            (_connection as IDisposable)?.Dispose();
        }
    }
}
