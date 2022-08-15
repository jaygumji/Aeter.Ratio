/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Aeter.Ratio.Reflection
{
    public class MissingMemberException : Exception
    {
        public MissingMemberException()
        {
        }

        public MissingMemberException(string? message) : base(message)
        {
        }

        public MissingMemberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MissingMemberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static MissingMemberException MissingMethod(Type type, string methodName)
        {
            return new MissingMemberException($"Missing method '{methodName}' on type '{type.FullName}'");
        }

        public static MissingMemberException MissingField(Type type, string fieldName)
        {
            return new MissingMemberException($"Missing field '{fieldName}' on type '{type.FullName}'");
        }

        public static MissingMemberException MissingProperty(Type type, string propertyName)
        {
            return new MissingMemberException($"Missing property '{propertyName}' on type '{type.FullName}'");
        }

        public static MissingMemberException MissingConstructor(Type type, Type[] parameterTypes)
        {
            return new MissingMemberException($"Missing constructor ({string.Join(", ", parameterTypes.Select(x => x.Name))}) on type '{type.FullName}'");
        }
    }
}
