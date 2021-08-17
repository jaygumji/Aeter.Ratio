/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Testing.Fakes.Entities
{
    public class Identifier
    {
        public int Id { get; set; }
        public ApplicationType Type { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Identifier;
            if (other == null) return false;
            return Type == other.Type && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Type.GetHashCode();
        }
    }
}