﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Logging;

using Moq;

namespace Microsoft.Testing.Platform.UnitTests;

[TestClass]
public sealed class LoggerTests
{
    private static readonly Func<string, Exception?, string> Formatter =
        (state, exception) =>
            string.Format(CultureInfo.InvariantCulture, "{0}{1}", state, exception is not null ? $" -- {exception}" : string.Empty);

    private const string Message = "Test";
    private readonly Exception _exception = new("TestException");
    private readonly Mock<ILogger> _mockLogger = new();

    public LoggerTests()
    {
        _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>(), Formatter));
        _mockLogger.Setup(x => x.LogAsync(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>(), Formatter));
    }

    private Logger<string> CreateLogger(LogLevel logLevel)
    {
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns<LogLevel>(currentLogLevel => currentLogLevel >= logLevel);

        Logger logger = new([_mockLogger.Object], logLevel);

        Mock<ILoggerFactory> mockLoggerFactory = new();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger);
        return new Logger<string>(mockLoggerFactory.Object);
    }

    [DynamicData(nameof(LogTestHelpers.GetLogLevelCombinations), typeof(LogTestHelpers))]
    [TestMethod]
    public void Logger_CheckEnabled(LogLevel defaultLogLevel, LogLevel currentLogLevel)
    {
        Logger<string> logger = CreateLogger(defaultLogLevel);
        Assert.AreEqual(logger.IsEnabled(currentLogLevel), LogTestHelpers.IsLogEnabled(defaultLogLevel, currentLogLevel));
    }

    [DynamicData(nameof(LogTestHelpers.GetLogLevelCombinations), typeof(LogTestHelpers))]
    [TestMethod]
    public void Logger_Log_FormattedStringIsCorrect(LogLevel defaultLogLevel, LogLevel currentLogLevel)
    {
        Logger<string> logger = CreateLogger(defaultLogLevel);

        logger.Log(currentLogLevel, Message, _exception, Formatter);
        _mockLogger.Verify(
            x => x.Log(currentLogLevel, Message, _exception, Formatter),
            LogTestHelpers.GetExpectedLogCallTimes(defaultLogLevel, currentLogLevel));
    }

    [DynamicData(nameof(LogTestHelpers.GetLogLevelCombinations), typeof(LogTestHelpers))]
    [TestMethod]
    public async ValueTask Logger_LogAsync_FormattedStringIsCorrect(LogLevel defaultLogLevel, LogLevel currentLogLevel)
    {
        Logger<string> logger = CreateLogger(defaultLogLevel);

        await logger.LogAsync(currentLogLevel, Message, _exception, Formatter);
        _mockLogger.Verify(
            x => x.LogAsync(currentLogLevel, Message, _exception, Formatter),
            LogTestHelpers.GetExpectedLogCallTimes(defaultLogLevel, currentLogLevel));
    }
}
