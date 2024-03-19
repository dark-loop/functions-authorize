// <copyright file="AsyncKeyedMonitor.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization.Internal
{
    /// <summary>
    /// Provides a way to monitor a key and block other threads from entering the same key.
    /// </summary>
    internal static class KeyedMonitor
    {
        private static readonly ConcurrentDictionary<object, KeyedLock> __locks = new();
        private static readonly Timer __cleanupTimer = new(_ => OnCleanup(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        static KeyedMonitor()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => __cleanupTimer.Dispose();
        }

        /// <summary>
        /// Enters the monitor for the specified key.
        /// </summary>
        /// <remarks>
        /// Try to use very unique keys per block of code you want to protect.
        /// If the same ey might be used in different parts of the code, create a string and prefix with another identifier.
        /// </remarks>
        /// <param name="key">The key to monitor.</param>
        /// <param name="unblockOnFirstExit">
        /// A value indicating the protected logic shouldn't need the lock any longer.
        /// The lock should not be monitored and should be cleaned-up internally.
        /// </param>
        public static async Task EnterAsync(object key, bool unblockOnFirstExit = false)
        {
            var @lock = __locks.GetOrAdd(key, _ => new KeyedLock(unblockOnFirstExit));

            if (!@lock.Terminated)
            {
                await @lock.LockAsync();
            }
        }

        /// <summary>
        /// Exits the monitor for the specified key.
        /// </summary>
        /// <param name="key">The key blocked on.</param>
        public static void Exit(object key)
        {
            if (__locks.TryGetValue(key, out var @lock))
            {
                @lock.Unlock();
            }
        }

        private static void OnCleanup()
        {
            var keysToDelete = __locks.Where(x => x.Value.Terminated).ToList();
            
            foreach (var key in keysToDelete)
            {
                __locks.TryRemove(key.Key, out _);
            }
        }

        private class KeyedLock
        {
            private readonly SemaphoreSlim _monitor;
            private readonly bool _disposeOnFirstExit;
            private bool _terminated;

            public KeyedLock(bool disposeOnFirstExit)
            {
                _monitor = new SemaphoreSlim(1);
                _disposeOnFirstExit = disposeOnFirstExit;
            }

            public bool Terminated => _terminated;

            public async Task LockAsync()
            {
                await _monitor.WaitAsync();
            }

            public void Unlock()
            {
                if (_terminated)
                {
                    return;
                }

                if (_disposeOnFirstExit)
                {
                    while (_monitor.CurrentCount == 0)
                    {
                        _monitor.Release();
                        Task.Delay(1).Wait();
                    }

                    _terminated = true;
                    _monitor.Dispose();
                }
                else
                {
                    if (_monitor.CurrentCount == 0)
                    {
                        _monitor.Release();
                    }
                }
            }
        }
    }
}
