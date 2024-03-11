// <copyright file="Class1.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Tests
{
    public static class LoggerUtils
    {
        public static ILogger<T> CreateLogger<T>(LogLevel level = LogLevel.None, Action<LogLevel, EventId, object, Exception?, Delegate>? onLog = null)
        {
            var mock = new Mock<ILogger<T>>();

            mock
                .Setup(l => l.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception?, Delegate>((lvl, id, state, exception, formatter) =>
                {
                    if (lvl < level) return;

                    Console.WriteLine($"[{lvl}]: {formatter.DynamicInvoke(state, exception)}");
                    onLog?.Invoke(lvl, id, state, exception, formatter);
                });

            return mock.Object;
        }
    }
}
