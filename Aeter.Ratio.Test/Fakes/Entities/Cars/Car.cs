/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Testing.Fakes.Entities.Cars
{
    public class Car
    {

        public Car()
        {
            Compartments = new List<Compartment>();
        }

        public string? RegistrationNumber { get; set; }
        public Nationality Nationality { get; set; }
        public int EstimatedValue { get; set; }
        public DateTime EstimatedAt { get; set; }

        public CarEngine? Engine { get; set; }
        public CarModel? Model { get; set; }
        public ICollection<Compartment> Compartments { get; set; }
    }
}
