/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Testing.Fakes.Entities.Cars;
using System.Linq;

namespace Aeter.Ratio.Test.Db.Linq
{
    public class EnigmaExpressionVisitorTests
    {

        public void Tree1()
        {
            var cars = new FakeEnigmaQueryableSet<Car>();
            var q = (
                from c in cars
                where c.RegistrationNumber == "X"
                select c);
            //var q = (
            //    from x in )
        }

    }
}
