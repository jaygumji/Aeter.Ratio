/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization
{
    public class EmptyGraphTraveller { }
    public class EmptyGraphTraveller<T> : EmptyGraphTraveller, IGraphTraveller<T>
    {
        public EmptyGraphTraveller() { }
        public void Travel(IWriteVisitor visitor, T graph)
        {
        }

        public void Travel(IReadVisitor visitor, T graph)
        {
        }

        public void Travel(IWriteVisitor visitor, object graph)
        {
        }

        public void Travel(IReadVisitor visitor, object graph)
        {
        }
    }
}

