// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Reflection;
using GitHubExtension.Helpers;

namespace GitHubExtension.Test.Helpers;

[TestClass]
public class WeakEventTests
{
    public sealed class TestEventArgs : EventArgs
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void Subscribe_ShouldRegisterHandler()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        var invocationCount = 0;
        EventHandler<TestEventArgs> handler = (sender, e) => invocationCount++;
        var args = new TestEventArgs { Value = 42 };

        // Act
        weakEvent.Subscribe(handler);
        weakEvent.Raise(this, args);

        // Assert
        Assert.AreEqual(1, invocationCount, "Handler should be called once");
    }

    [TestMethod]
    public void Unsubscribe_ShouldUnregisterHandler()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        var callCount = 0;
        EventHandler<TestEventArgs> handler = (sender, e) => callCount++;

        // Act
        weakEvent.Subscribe(handler);
        weakEvent.Unsubscribe(handler);
        weakEvent.Raise(this, new TestEventArgs { Value = 42 });

        // Assert
        Assert.AreEqual(0, callCount, "Handler should not be called after removal");
    }

    [TestMethod]
    public void Raise_ShouldInvokeAllHandlers()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        int callCount1 = 0, callCount2 = 0;
        int lastValue1 = 0, lastValue2 = 0;

        EventHandler<TestEventArgs> handler1 = (sender, e) =>
        {
            callCount1++;
            lastValue1 = e.Value;
        };

        EventHandler<TestEventArgs> handler2 = (sender, e) =>
        {
            callCount2++;
            lastValue2 = e.Value;
        };

        var args = new TestEventArgs { Value = 42 };

        // Act
        weakEvent.Subscribe(handler1);
        weakEvent.Subscribe(handler2);
        weakEvent.Raise(this, args);

        // Assert
        Assert.AreEqual(1, callCount1, "First handler should be called once");
        Assert.AreEqual(1, callCount2, "Second handler should be called once");
        Assert.AreEqual(42, lastValue1, "First handler should receive correct value");
        Assert.AreEqual(42, lastValue2, "Second handler should receive correct value");
    }

    [TestMethod]
    public void WeakEventSource_ShouldAddHandler()
    {
        // Test that handlers are added correctly
        var weakEvent = new WeakEventSource<TestEventArgs>();
        EventHandler<TestEventArgs> handler = (sender, e) => { };

        weakEvent.Subscribe(handler);

        var fieldInfo = typeof(WeakEventSource<TestEventArgs>).GetField("_delegates", BindingFlags.NonPublic | BindingFlags.Instance);
        var delegates = (IList)fieldInfo!.GetValue(weakEvent)!;

        Assert.AreEqual(1, delegates.Count, "Handler should be added");
    }

    private sealed class InstanceSubscriber
    {
        public int CallCount { get; private set; }

        public void Handler(object? sender, TestEventArgs e)
        {
            CallCount++;
        }
    }

    [TestMethod]
    public void WeakReferenceSanityCheck()
    {
        WeakReference weakRef;

        weakRef = new Func<WeakReference>(() =>
        {
            var obj = new object();
            return new WeakReference(obj);
        })();

        Assert.IsTrue(weakRef.IsAlive, "Weak reference should be alive after creation");

        // Force garbage collection
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        Assert.IsFalse(weakRef.IsAlive, "Weak reference should not be alive after GC");
    }

    [TestMethod]
    public void WeakEventSource_ShouldRemoveDeadReferences()
    {
        // Arrange
        var weakEvent = new Func<WeakEventSource<TestEventArgs>>(() =>
        {
            var weakEventSource = new WeakEventSource<TestEventArgs>();
            var subscriber = new InstanceSubscriber();
            weakEventSource.Subscribe(subscriber.Handler);
            return weakEventSource;
        })();

        // Verify initial state with reflection
        var fieldInfo = typeof(WeakEventSource<TestEventArgs>).GetField("_delegates", BindingFlags.NonPublic | BindingFlags.Instance);
        var delegates = (IList)fieldInfo!.GetValue(weakEvent)!;
        Assert.AreEqual(1, delegates.Count, "Should have 1 delegate before GC");

        // Force garbage collection
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        // Trigger cleanup by raising the event
        weakEvent.Raise(this, new TestEventArgs());

        // Get updated delegates list
        delegates = (IList)fieldInfo!.GetValue(weakEvent)!;

        // Dead references should be removed during Raise
        Assert.AreEqual(0, delegates.Count, "Dead reference should be removed");
    }

    [TestMethod]
    public void WeakEventSource_ShouldInvokeRemainingHandlersAfterGarbageCollection()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        var permanentCallCount = 0;
        var lastValue = 0;

        // Permanent handler that we'll keep a reference to
        EventHandler<TestEventArgs> permanentHandler = (sender, e) =>
        {
            permanentCallCount++;
            lastValue = e.Value;
        };

        // Act - add permanent handler and a temporary one
        weakEvent.Subscribe(permanentHandler);

        // Create a scope for the temporary handler
        {
            var tempCallCount = 0;
            EventHandler<TestEventArgs>? temporaryHandler = (sender, e) => tempCallCount++;
            weakEvent.Subscribe(temporaryHandler);

            // Verify both work initially
            weakEvent.Raise(this, new TestEventArgs { Value = 42 });
            Assert.AreEqual(1, permanentCallCount, "Permanent handler should be called");
            Assert.AreEqual(1, tempCallCount, "Temporary handler should be called");

            // Clear reference to temporary handler
            temporaryHandler = null;
        }

        // Force garbage collection
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        // Reset counter
        permanentCallCount = 0;

        // Raise event again
        weakEvent.Raise(this, new TestEventArgs { Value = 100 });

        // Assert
        Assert.AreEqual(1, permanentCallCount, "Permanent handler should still be called after GC");
        Assert.AreEqual(100, lastValue, "Permanent handler should receive correct value");
    }

    // Helper for testing static handlers
    // Should be only used in one test.
    private static class StaticHandlerCounter
    {
        public static int Count { get; set; }
    }

    [TestMethod]
    public void WeakEventSource_ShouldSupportStaticHandlers()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        StaticHandlerCounter.Count = 0;

        // Static handler
        static void StaticHandler(object? sender, TestEventArgs args)
        {
            StaticHandlerCounter.Count++;
        }

        // Act
        weakEvent.Subscribe(StaticHandler);
        weakEvent.Raise(this, new TestEventArgs { Value = 42 });

        // Assert
        Assert.AreEqual(1, StaticHandlerCounter.Count, "Static handler should be called");

        // Force garbage collection (shouldn't affect static methods)
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        // Reset counter
        StaticHandlerCounter.Count = 0;

        // Raise again
        weakEvent.Raise(this, new TestEventArgs { Value = 100 });

        // Assert static handler still works
        Assert.AreEqual(1, StaticHandlerCounter.Count, "Static handler should still work after GC");
    }

    [TestMethod]
    public void WeakEventSource_ShouldHandleMultipleRaises()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        var callCount = 0;
        var lastValue = 0;

        EventHandler<TestEventArgs> handler = (sender, e) =>
        {
            callCount++;
            lastValue = e.Value;
        };

        // Act
        weakEvent.Subscribe(handler);

        // Raise multiple times
        for (var i = 0; i < 5; i++)
        {
            weakEvent.Raise(this, new TestEventArgs { Value = i });
        }

        // Assert
        Assert.AreEqual(5, callCount, "Handler should be called five times");
        Assert.AreEqual(4, lastValue, "Last value should be 4");
    }

    [TestMethod]
    public void WeakEventSource_ShouldSupportDynamicallyAddingAndRemovingHandlers()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        int callCount1 = 0, callCount2 = 0, callCount3 = 0;
        int lastValue1 = 0, lastValue2 = 0, lastValue3 = 0;

        EventHandler<TestEventArgs> handler1 = (sender, e) =>
        {
            callCount1++;
            lastValue1 = e.Value;
        };

        EventHandler<TestEventArgs> handler2 = (sender, e) =>
        {
            callCount2++;
            lastValue2 = e.Value;
        };

        EventHandler<TestEventArgs> handler3 = (sender, e) =>
        {
            callCount3++;
            lastValue3 = e.Value;
        };

        // Act - add first handler
        weakEvent.Subscribe(handler1);
        weakEvent.Raise(this, new TestEventArgs { Value = 1 });

        // Add second handler
        weakEvent.Subscribe(handler2);
        weakEvent.Raise(this, new TestEventArgs { Value = 2 });

        // Remove first handler
        weakEvent.Unsubscribe(handler1);
        weakEvent.Raise(this, new TestEventArgs { Value = 3 });

        // Add third handler
        weakEvent.Subscribe(handler3);
        weakEvent.Raise(this, new TestEventArgs { Value = 4 });

        // Assert
        Assert.AreEqual(2, callCount1, "First handler should be called twice");
        Assert.AreEqual(3, callCount2, "Second handler should be called three times");
        Assert.AreEqual(1, callCount3, "Third handler should be called once");

        Assert.AreEqual(2, lastValue1, "First handler should have last value 2");
        Assert.AreEqual(4, lastValue2, "Second handler should have last value 4");
        Assert.AreEqual(4, lastValue3, "Third handler should have last value 4");
    }

    [TestMethod]
    public void WeakEventSource_ShouldWorkWithAnonymousHandlers()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();
        var capturedValue = 0;

        EventHandler<TestEventArgs> handler = (sender, args) => capturedValue = args.Value;

        // Act
        weakEvent.Subscribe(handler);
        weakEvent.Raise(this, new TestEventArgs { Value = 42 });

        // Assert
        Assert.AreEqual(42, capturedValue, "Anonymous handler should be called");
    }

    [TestMethod]
    public void WeakEventSource_ShouldHandleRaisingWhenNoHandlersRegistered()
    {
        // Arrange
        var weakEvent = new WeakEventSource<TestEventArgs>();

        // Act & Assert - should not throw
        weakEvent.Raise(this, new TestEventArgs { Value = 42 });
    }
}
