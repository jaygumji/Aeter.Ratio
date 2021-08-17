/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Db
{
    public interface IEnigmaCommand
    {

        void Add(string name, BufferedCommandHandler handler);
        void Modify(string name, BufferedCommandHandler handler);
        void Remove(string name, BufferedCommandHandler handler);

    }

    public delegate void BufferedCommandHandler(BinaryWriteBuffer writeBuffer);
}