/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.Threading;
using Xunit;

namespace Aeter.Ratio.Test.Threading
{
    public class InvocationTests
    {
        [Fact]
        public void RunSync_VoidTask_CompletesBeforeReturn()
        {
            var flag = false;

            Invocation.RunSync(async () => {
                await Task.Yield();
                flag = true;
            });

            Assert.True(flag);
        }

        [Fact]
        public void RunSync_VoidTask_RunsOnCallingThread()
        {
            var callingThreadId = Thread.CurrentThread.ManagedThreadId;
            var observedThreadId = -1;

            Invocation.RunSync(async () => {
                await Task.Yield();
                observedThreadId = Thread.CurrentThread.ManagedThreadId;
            });

            Assert.Equal(callingThreadId, observedThreadId);
        }

        [Fact]
        public void RunSync_VoidTaskWithState_PassesState()
        {
            var captured = string.Empty;

            Invocation.RunSync(async state => {
                await Task.Yield();
                captured = (string)state;
            }, "expected");

            Assert.Equal("expected", captured);
        }

        [Fact]
        public void RunSync_TaskWithResult_ReturnsResult()
        {
            var result = Invocation.RunSync(async () => {
                await Task.Yield();
                return 42;
            });

            Assert.Equal(42, result);
        }

        [Fact]
        public void RunSync_TaskWithGenericState_ReturnsResult()
        {
            var result = Invocation.RunSync<int, string>(async state => {
                await Task.Yield();
                return state.Length;
            }, "state");

            Assert.Equal(5, result);
        }

        [Fact]
        public void RunSync_TaskThrows_PropagatesException()
        {
            Assert.Throws<InvalidOperationException>(() => Invocation.RunSync(async () => {
                await Task.Yield();
                throw new InvalidOperationException();
            }));
        }

        [Fact]
        public void RunSync_NullAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Invocation.RunSync((Func<Task>)null!));
        }
    }
}
