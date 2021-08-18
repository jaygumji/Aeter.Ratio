/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Aeter.Ratio.DependencyInjection
{

    public static class InstanceFactory
    {
        public static T CreateLambda<T>(ConstructorInfo constructor, PropertyInfo[] properties)
        {
            var conArgs = constructor.GetParameters();
            var length = conArgs.Length + (properties?.Length ?? 0);

            var parameters = new ParameterExpression[length];

            var conParams = new ParameterExpression[conArgs.Length];
            for (var i = 0; i < conArgs.Length; i++) {
                var param = Expression.Parameter(conArgs[i].ParameterType);
                conParams[i] = param;
                parameters[i] = param;
            }
            var newExpr = Expression.New(constructor, conParams);
            Expression expr = newExpr;

            if (properties != null && properties.Length > 0) {
                var bindingExpressions = new MemberAssignment[properties.Length];
                var pi = conArgs.Length;
                for (var i = 0; i < properties.Length; i++) {
                    var property = properties[i];
                    var parameter = Expression.Parameter(property.PropertyType);
                    parameters[pi++] = parameter;
                    bindingExpressions[i] = Expression.Bind(property, parameter);
                }
                expr = Expression.MemberInit(newExpr, bindingExpressions);
            }

            var lambda = Expression.Lambda<T>(expr, parameters);
            return lambda.Compile();
        }
    }

    public class InstanceFactory<TInstance> : IInstanceFactory<TInstance>
    {
        private readonly Func<TInstance> _lambda;

        public InstanceFactory(ConstructorInfo constructor)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TInstance>>(constructor, null);
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke();
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke();
        }
       
    }


    public class InstanceFactory<TInstance, TP1> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TInstance>>(
                constructor, properties
            );

            _p1 = p1;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;
        private readonly IInstanceFactory<TP12> _p12;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11, IInstanceFactory<TP12> p12)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
            _p12 = p12;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;
        private readonly IInstanceFactory<TP12> _p12;
        private readonly IInstanceFactory<TP13> _p13;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11, IInstanceFactory<TP12> p12, IInstanceFactory<TP13> p13)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
            _p12 = p12;
            _p13 = p13;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;
        private readonly IInstanceFactory<TP12> _p12;
        private readonly IInstanceFactory<TP13> _p13;
        private readonly IInstanceFactory<TP14> _p14;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11, IInstanceFactory<TP12> p12, IInstanceFactory<TP13> p13, IInstanceFactory<TP14> p14)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
            _p12 = p12;
            _p13 = p13;
            _p14 = p14;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;
        private readonly IInstanceFactory<TP12> _p12;
        private readonly IInstanceFactory<TP13> _p13;
        private readonly IInstanceFactory<TP14> _p14;
        private readonly IInstanceFactory<TP15> _p15;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11, IInstanceFactory<TP12> p12, IInstanceFactory<TP13> p13, IInstanceFactory<TP14> p14, IInstanceFactory<TP15> p15)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
            _p12 = p12;
            _p13 = p13;
            _p14 = p14;
            _p15 = p15;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance(), _p15.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance(), _p15.GetInstance());
        }
    }


    public class InstanceFactory<TInstance, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15, TP16> : IInstanceFactory<TInstance>
    {
        private readonly Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15, TP16, TInstance> _lambda;
        private readonly IInstanceFactory<TP1> _p1;
        private readonly IInstanceFactory<TP2> _p2;
        private readonly IInstanceFactory<TP3> _p3;
        private readonly IInstanceFactory<TP4> _p4;
        private readonly IInstanceFactory<TP5> _p5;
        private readonly IInstanceFactory<TP6> _p6;
        private readonly IInstanceFactory<TP7> _p7;
        private readonly IInstanceFactory<TP8> _p8;
        private readonly IInstanceFactory<TP9> _p9;
        private readonly IInstanceFactory<TP10> _p10;
        private readonly IInstanceFactory<TP11> _p11;
        private readonly IInstanceFactory<TP12> _p12;
        private readonly IInstanceFactory<TP13> _p13;
        private readonly IInstanceFactory<TP14> _p14;
        private readonly IInstanceFactory<TP15> _p15;
        private readonly IInstanceFactory<TP16> _p16;

        public InstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, IInstanceFactory<TP1> p1, IInstanceFactory<TP2> p2, IInstanceFactory<TP3> p3, IInstanceFactory<TP4> p4, IInstanceFactory<TP5> p5, IInstanceFactory<TP6> p6, IInstanceFactory<TP7> p7, IInstanceFactory<TP8> p8, IInstanceFactory<TP9> p9, IInstanceFactory<TP10> p10, IInstanceFactory<TP11> p11, IInstanceFactory<TP12> p12, IInstanceFactory<TP13> p13, IInstanceFactory<TP14> p14, IInstanceFactory<TP15> p15, IInstanceFactory<TP16> p16)
        {
            _lambda = InstanceFactory.CreateLambda<Func<TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TP9, TP10, TP11, TP12, TP13, TP14, TP15, TP16, TInstance>>(
                constructor,
                properties
            );

            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            _p5 = p5;
            _p6 = p6;
            _p7 = p7;
            _p8 = p8;
            _p9 = p9;
            _p10 = p10;
            _p11 = p11;
            _p12 = p12;
            _p13 = p13;
            _p14 = p14;
            _p15 = p15;
            _p16 = p16;
        }

        object IInstanceFactory.GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance(), _p15.GetInstance(), _p16.GetInstance());
        }

        public TInstance GetInstance()
        {
            return _lambda.Invoke(_p1.GetInstance(), _p2.GetInstance(), _p3.GetInstance(), _p4.GetInstance(), _p5.GetInstance(), _p6.GetInstance(), _p7.GetInstance(), _p8.GetInstance(), _p9.GetInstance(), _p10.GetInstance(), _p11.GetInstance(), _p12.GetInstance(), _p13.GetInstance(), _p14.GetInstance(), _p15.GetInstance(), _p16.GetInstance());
        }
    }
}