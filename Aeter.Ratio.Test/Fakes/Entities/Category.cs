/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Testing.Fakes.Entities
{
    public class Category
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public byte[]? Image { get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as Category;
            if (other == null) return false;

            return Name == other.Name && Description == other.Description && BlobComparer.AreEqual(Image, other.Image);
        }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode())
                   ^ (Description == null ? 0 : Description.GetHashCode())
                   ^ BlobComparer.GetBlobHashCode(Image);
        }
    }
}