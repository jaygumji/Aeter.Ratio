/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.EntityStore
{
    public class EntityEngineEvents
    {
        protected readonly AsyncEvent<EntityEngineEventsChangedArgs> entityChanged = new();

        public void Register(AsyncEventDelegate<EntityEngineEventsChangedArgs> entityChangedHandler) => entityChanged.Register(entityChangedHandler);
        public void Unregister(AsyncEventDelegate<EntityEngineEventsChangedArgs> entityChangedHandler) => entityChanged.Unregister(entityChangedHandler);
    }
}
