// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Configuration;
using Xunit;

namespace OpenTelemetry.Trace.Tests;

public class BatchExportPartialActivityProcessorOptionsTest : IDisposable
{
    public BatchExportPartialActivityProcessorOptionsTest()
    {
        ClearEnvVars();
    }

    public void Dispose()
    {
        ClearEnvVars();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_Defaults()
    {
        var options = new BatchExportPartialActivityProcessorOptions();

        Assert.Equal(30000, options.ExporterTimeoutMilliseconds);
        Assert.Equal(512, options.MaxExportBatchSize);
        Assert.Equal(2048, options.MaxQueueSize);
        Assert.Equal(5000, options.ScheduledDelayMilliseconds);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_EnvironmentVariableOverride()
    {
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey, "1");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxExportBatchSizeEnvVarKey, "2");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxQueueSizeEnvVarKey, "3");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ScheduledDelayEnvVarKey, "4");

        var options = new BatchExportPartialActivityProcessorOptions();

        Assert.Equal(1, options.ExporterTimeoutMilliseconds);
        Assert.Equal(2, options.MaxExportBatchSize);
        Assert.Equal(3, options.MaxQueueSize);
        Assert.Equal(4, options.ScheduledDelayMilliseconds);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_UsingIConfiguration()
    {
        var values = new Dictionary<string, string?>()
        {
            [BatchExportPartialActivityProcessorOptions.MaxQueueSizeEnvVarKey] = "1",
            [BatchExportPartialActivityProcessorOptions.MaxExportBatchSizeEnvVarKey] = "2",
            [BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey] = "3",
            [BatchExportPartialActivityProcessorOptions.ScheduledDelayEnvVarKey] = "4",
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var options = new BatchExportPartialActivityProcessorOptions(configuration);

        Assert.Equal(1, options.MaxQueueSize);
        Assert.Equal(2, options.MaxExportBatchSize);
        Assert.Equal(3, options.ExporterTimeoutMilliseconds);
        Assert.Equal(4, options.ScheduledDelayMilliseconds);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_InvalidEnvironmentVariableOverride()
    {
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey, "invalid");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxExportBatchSizeEnvVarKey, "invalid");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxQueueSizeEnvVarKey, "invalid");
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ScheduledDelayEnvVarKey, "invalid");

        var options = new BatchExportPartialActivityProcessorOptions();

        Assert.Equal(30000, options.ExporterTimeoutMilliseconds);
        Assert.Equal(512, options.MaxExportBatchSize);
        Assert.Equal(2048, options.MaxQueueSize);
        Assert.Equal(5000, options.ScheduledDelayMilliseconds);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_SetterOverridesEnvironmentVariable()
    {
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey, "123");

        var options = new BatchExportPartialActivityProcessorOptions
        {
            ExporterTimeoutMilliseconds = 89000,
        };

        Assert.Equal(89000, options.ExporterTimeoutMilliseconds);
    }

    [Fact]
    public void BatchExportPartialProcessorOptions_EnvironmentVariableNames()
    {
        Assert.Equal("OTEL_BSP_EXPORT_TIMEOUT", BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey);
        Assert.Equal("OTEL_BSP_MAX_EXPORT_BATCH_SIZE", BatchExportPartialActivityProcessorOptions.MaxExportBatchSizeEnvVarKey);
        Assert.Equal("OTEL_BSP_MAX_QUEUE_SIZE", BatchExportPartialActivityProcessorOptions.MaxQueueSizeEnvVarKey);
        Assert.Equal("OTEL_BSP_SCHEDULE_DELAY", BatchExportPartialActivityProcessorOptions.ScheduledDelayEnvVarKey);
    }

    private static void ClearEnvVars()
    {
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ExporterTimeoutEnvVarKey, null);
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxExportBatchSizeEnvVarKey, null);
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.MaxQueueSizeEnvVarKey, null);
        Environment.SetEnvironmentVariable(BatchExportPartialActivityProcessorOptions.ScheduledDelayEnvVarKey, null);
    }
}
