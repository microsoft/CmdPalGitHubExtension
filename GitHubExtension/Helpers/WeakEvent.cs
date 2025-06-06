// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using System.Reflection;

namespace GitHubExtension.Helpers;

#pragma warning disable SA1649 // File name should match first type name
public class WeakEventSource<TEventArgs>
        where TEventArgs : EventArgs
{
    private delegate void OpenEventHandler(object? target, object? sender, TEventArgs e);

    private struct StrongHandler
    {
        private readonly object? _target;
        private readonly OpenEventHandler _openHandler;

        public StrongHandler(object? target, OpenEventHandler openHandler)
        {
            _target = target;
            _openHandler = openHandler;
        }

        public void Invoke(object? sender, TEventArgs e)
        {
            _openHandler(_target, sender, e);
        }
    }

    private sealed class WeakDelegate
    {
        private readonly WeakReference? _weakTarget;
        private readonly MethodInfo _method;
        private readonly OpenEventHandler _openHandler;

        public WeakDelegate(
            Delegate handler,
            OpenEventHandler openHandler)
        {
            _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
            _method = handler.GetMethodInfo();
            _openHandler = openHandler;
        }

        public bool IsAlive => _weakTarget?.IsAlive ?? true;

        public StrongHandler? TryGetStrongHandler()
        {
            object? target = null;
            if (_weakTarget != null)
            {
                target = _weakTarget.Target;
                if (target == null)
                {
                    return null;
                }
            }

            return new StrongHandler(target, _openHandler);
        }

        public bool IsMatch(Delegate handler)
        {
            return ReferenceEquals(handler.Target, _weakTarget?.Target)
                    && handler.GetMethodInfo().Equals(_method);
        }
    }

    private readonly object _delegatesLock = new();
    private readonly List<WeakDelegate> _delegates = new();

    private static OpenEventHandler CreateOpenHandler(MethodInfo method)
    {
        var target = Expression.Parameter(typeof(object), "target");
        var sender = Expression.Parameter(typeof(object), "sender");
        var e = Expression.Parameter(typeof(TEventArgs), "e");

        if (method.IsStatic)
        {
            var expr = Expression.Lambda<OpenEventHandler>(Expression.Call(method, sender, e), target, sender, e);
            return expr.Compile();
        }
        else
        {
            var expr = Expression.Lambda<OpenEventHandler>(Expression.Call(Expression.Convert(target, method.DeclaringType!), method, sender, e), target, sender, e);
            return expr.Compile();
        }
    }

    public void Raise(object? sender, TEventArgs args)
    {
        var validDelegates = new List<WeakDelegate>();
        lock (_delegatesLock)
        {
            _delegates.RemoveAll(d =>
            {
                if (d.IsAlive)
                {
                    validDelegates.Add(d);
                    return false;
                }

                return true;
            });
        }

        foreach (var d in validDelegates)
        {
            var strongHandler = d.TryGetStrongHandler();
            if (strongHandler != null)
            {
                strongHandler.Value.Invoke(sender, args);
            }
        }
    }

    public void Subscribe(EventHandler<TEventArgs>? handler)
    {
        if (handler == null)
        {
            return;
        }

        lock (_delegatesLock)
        {
            _delegates.Add(new WeakDelegate(handler, CreateOpenHandler(handler.GetMethodInfo())));
        }
    }

    public void Unsubscribe(EventHandler<TEventArgs>? handler)
    {
        if (handler == null)
        {
            return;
        }

        lock (_delegatesLock)
        {
            _delegates.RemoveAll(d =>
            {
                if (d.IsMatch(handler))
                {
                    return true;
                }

                return !d.IsAlive;
            });
        }
    }
}
