/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Runtime.ExceptionServices;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class IntermediateGraphTraveller
    {
        private IGraphTraveller? _instance;

        public Type Type { get; }
        public Type TravellerType { get; private set; }
        public IVisitArgsFactory VisitArgsFactory { get; }
        public object?[] Parameters { get; }
        public DynamicTravellerBuilder? Builder { get; private set; }
        public Exception? Exception { get; private set; }

        public IntermediateGraphTraveller(DynamicTravellerBuilder builder, IVisitArgsFactory visitArgsFactory)
            : this(builder.Type, builder.DynamicTraveller.TravellerType, visitArgsFactory, visitArgsFactory)
        {
            Builder = builder;
        }

        public IntermediateGraphTraveller(Type type, Type travellerType, IVisitArgsFactory visitArgsFactory, params object?[] parameters)
        {
            Type = type;
            TravellerType = travellerType;
            VisitArgsFactory = visitArgsFactory;
            Parameters = parameters;
        }

        public IGraphTraveller Instance
        {
            get {
                if (Exception != null) ExceptionDispatchInfo.Capture(Exception).Throw();
                if (_instance != null) return _instance;
                if (Builder != null) {
                    _instance = Builder.DynamicTraveller.GetInstance(VisitArgsFactory);
                }
                else if (TravellerType.BaseType == typeof(EmptyGraphTraveller)) {
                    return (IGraphTraveller)Activator.CreateInstance(TravellerType)!;
                }
                else {
                    _instance = (IGraphTraveller)Activator.CreateInstance(TravellerType, Parameters)!;
                }
                return _instance;
            }
        }

        public void FinishContructing()
        {
            if (Exception != null) ExceptionDispatchInfo.Capture(Exception).Throw();

            if (Builder != null) {
                try {
                    Builder.BuildTraveller();
                    TravellerType = Builder.DynamicTraveller.TravellerType;
                }
                catch (Exception exception) {
                    Exception = exception;
                }
            }
        }
    }
}