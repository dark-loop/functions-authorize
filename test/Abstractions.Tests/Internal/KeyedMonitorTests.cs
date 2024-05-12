// <copyright file="KeyedMonitorTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using AsyncKeyedLock;

namespace Abstractions.Tests.Internal
{
    [TestClass]
    public class KeyedMonitorTests
    {
        private bool _flag;
        private List<string>? _executedTasks;
        private List<string>? _unmonitored;
        private List<string>? _monitored;
        private List<string>? _winner;
        private AsyncKeyedLocker<string>? _asyncKeyedLocker;

        [TestInitialize]
        public void Initialize()
        {
            _flag = false;
            _executedTasks = new List<string>();
            _unmonitored = new List<string>();
            _monitored = new List<string>();
            _winner = new List<string>();
            _asyncKeyedLocker = new(o =>
            {
                o.PoolSize = 20;
                o.PoolInitialFill = 1;
            });
        }

        [TestMethod("KeyedMonitor: should allow for other threads to unblock after first exit")]
        public async Task KeyedMonitorShouldAllowSilentExitAfterFirstExit()
        {
            // Arrange
            var task1 = () => MonitoredLogicAsync("task1", 200);
            var task2 = () => MonitoredLogicAsync("task2", 0);
            var task3 = () => MonitoredLogicAsync("task3", 0);
            var task4 = () => MonitoredLogicAsync("task4", 20);
            var task5 = () => MonitoredLogicAsync("task5", 0);

            // Act
            _ = Task.Run(() => task1());

            // waiting 100ms to ensure task1 enters first;
            await Task.Delay(50);

            _ = Task.Run(() => task2());
            _ = Task.Run(() => task3());
            _ = Task.Run(() => task4());

            await Task.Delay(180);

            _ = Task.Run(() => task5());

            // awaiting for all tasks to complete
            await Task.Delay(500);

            // Assert
            Assert.AreEqual(5, _executedTasks!.Count);
            Assert.AreEqual(1, _unmonitored!.Count);
            Assert.AreEqual(4, _monitored!.Count);
            Assert.AreEqual(1, _winner!.Count);
            Assert.IsTrue(_flag);
        }

        private async Task MonitoredLogicAsync(string name, int millisecondsToBlock)
        {
            _executedTasks!.Add(name);
            
            if (_flag)
            {
                _unmonitored!.Add(name);
                return;
            }

            using (await _asyncKeyedLocker.LockAsync("x"))
            {
                await Task.Delay(millisecondsToBlock);
                _monitored!.Add(name);

                if (_flag)
                {
                    return;
                }

                _flag = true;
                _winner!.Add(name);
            }
        }
    }
}
