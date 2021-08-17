/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization
{
    public interface IGraphTraveller
    {
        void Travel(IWriteVisitor visitor, object graph);
        void Travel(IReadVisitor visitor, object graph);
    }

    public interface IGraphTraveller<in T> : IGraphTraveller
    {
        void Travel(IWriteVisitor visitor, T graph);
        void Travel(IReadVisitor visitor, T graph);
    }

}
