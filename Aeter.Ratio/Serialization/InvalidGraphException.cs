﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization
{
    public class InvalidGraphException : Exception
    {
        public InvalidGraphException(string message) : base(message)
        {
        }

        public static InvalidGraphException NoParameterLessConstructor(Type type)
        {
            return new InvalidGraphException(string.Format("Type {0} is used, but does not contain a parameterless constructor", type.FullName));
        }

        public static InvalidGraphException ComplexTypeWithoutTravellerDefined(Type type)
        {
            return new InvalidGraphException(string.Format("Use of complex type {0} detected, but no traveller has been defined", type.FullName));
        }

        public static InvalidGraphException MissingCollectionAddMethod(Type type)
        {
            return new InvalidGraphException(string.Format("Collection of type {0} is used in the graph, but the collection is missing an Add method that takes the element type as parameter", type.FullName));
        }

        public static InvalidGraphException NoDictionaryValue(string name)
        {
            return new InvalidGraphException(string.Format("Failed to read the value of a dictionary key value pair of property {0}", name));
        }

        public static InvalidGraphException DuplicateProperties(Type type, PropertyInfo property)
        {
            return new InvalidGraphException(string.Format("Duplicate properties with name {0} found on type {1}", property.Name, type.FullName));
        }
    }
}