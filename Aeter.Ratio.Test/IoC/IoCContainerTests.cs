/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IoC;
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Test.Fakes.IoC;
using Aeter.Ratio.Test.IoC.Fakes;
using Aeter.Ratio.Testing.Fakes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.Test.Fakes.Entities;
using Xunit;

namespace Aeter.Ratio.Test.IoC
{
    public class IoCContainerTests
    {

        [Fact]
        public void CreateParameterlessInstance()
        {
            var factory = new IoCFactory(new IoCRegistratorMock(), new FactoryTypeProvider());
            for (var i = 0; i < 20; i++) {
                var instance = factory.GetInstance(typeof(DataBlock), throwError: true);
                Assert.NotNull(instance);
            }
        }

        [Fact]
        public void CreateDependentInstance()
        {
            var container = new IoCContainer();
            container.Register<ICoreCalculator, CoreCalculator>();
            container.Register<ICoreValidator>(new CoreValidator(4));
            container.Register(CoreConfig.Default);
            container.Register<ICommandEvents, EmptyCommandEvents>();

            var core = container.GetInstance<Core>();
            Assert.Null(core.Initializer);
            Assert.NotNull(core.Events);
            core.Calculate(4, 6, 5);
        }

        [Fact]
        public void SingleScope()
        {
            var container = new IoCContainer();
            AmDisposable disp;
            using (new IoCScope()) {
                disp = container.GetInstance<AmDisposable>();
                Assert.Equal(0, disp.DisposeCalled);

                var disp2 = container.GetInstance<AmDisposable>();
                Assert.Same(disp, disp2);
            }
            Assert.Equal(1, disp.DisposeCalled);
        }

        [Fact]
        public void NestedScope()
        {
            var container = new IoCContainer();
            AmDisposable disp;
            using (new IoCScope()) {
                disp = container.GetInstance<AmDisposable>();
                Assert.Equal(0, disp.DisposeCalled);

                var disp2 = container.GetInstance<AmDisposable>();
                Assert.Same(disp, disp2);

                AmDisposable disp3;
                using (new IoCScope()) {
                    disp3 = container.GetInstance<AmDisposable>();
                    Assert.NotSame(disp, disp3);

                    var disp4 = container.GetInstance<AmDisposable>();
                    Assert.Same(disp3, disp4);
                }
                Assert.Equal(1, disp3.DisposeCalled);
            }
            Assert.Equal(1, disp.DisposeCalled);
        }

        [Fact]
        public void MustBeScoped()
        {
            var container = new IoCContainer();
            container.Register<ICoreCalculator, CoreCalculator>()
                .MustBeScoped();

            var exception = Assert.Throws<InvalidOperationException>(() => {
                var calc = container.GetInstance<ICoreCalculator>();
                Assert.NotNull(calc);
            });

            Assert.Contains("could not be created since it requires a scope", exception.Message);
        }

        [Fact]
        public void TwoThreadsScoping()
        {
            var t1Ready = new ManualResetEvent(false);
            var t2Ready = new ManualResetEvent(false);

            Exception exception1 = null;
            Exception exception2 = null;

            var t1 = new Thread(state => exception1 = ThreadedScopeThreadRun(t1Ready, t2Ready));
            var t2 = new Thread(state => exception2 = ThreadedScopeThreadRun(t2Ready, t1Ready));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            t1Ready.Dispose();
            t2Ready.Dispose();

            if (exception1 != null) {
                ExceptionDispatchInfo.Capture(exception1).Throw();
            }
            if (exception2 != null) {
                ExceptionDispatchInfo.Capture(exception2).Throw();
            }
        }

        [Fact]
        public async Task TwoAsyncScoping()
        {
            var t1Ready = new ManualResetEvent(false);
            var t2Ready = new ManualResetEvent(false);

            Exception exception1 = null;
            Exception exception2 = null;

            var t1 = Task.Factory.StartNew(() => exception1 = ThreadedScopeThreadRun(t1Ready, t2Ready));
            var t2 = Task.Factory.StartNew(() => exception2 = ThreadedScopeThreadRun(t2Ready, t1Ready));

            await t1;
            await t2;

            t1Ready.Dispose();
            t2Ready.Dispose();

            if (exception1 != null) {
                ExceptionDispatchInfo.Capture(exception1).Throw();
            }
            if (exception2 != null) {
                ExceptionDispatchInfo.Capture(exception2).Throw();
            }
        }

        private Exception ThreadedScopeThreadRun(ManualResetEvent set, ManualResetEvent wait)
        {
            try {
                var container = new IoCContainer();
                AmDisposable disp;
                using (var scope = new IoCScope()) {
                    disp = container.GetInstance<AmDisposable>();
                    Assert.Equal(0, disp.DisposeCalled);
                    Assert.Equal(1, scope.InstanceCount);

                    set.Set();
                    wait.WaitOne();

                    Assert.Equal(1, scope.InstanceCount);

                    var disp2 = container.GetInstance<AmDisposable>();
                    Assert.Same(disp, disp2);
                    Assert.Equal(1, scope.InstanceCount);
                }
                Assert.Equal(1, disp.DisposeCalled);
            }
            catch (Exception exception) {
                return exception;
            }
            return null;
        }

    }
}
