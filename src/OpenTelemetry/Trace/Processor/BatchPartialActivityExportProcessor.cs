// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace OpenTelemetry;

/// <summary>
/// Implements processor that batches <see cref="Activity"/> objects before calling exporter.
/// </summary>
public class BatchPartialActivityExportProcessor : BatchExportProcessor<Activity>
{
    internal SimpleLogRecordExportProcessor SimpleLogRecordExportProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchActivityExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporter']"/></param>
    /// <param name="simpleLogRecordExportProcessor"></param>
    /// <param name="maxQueueSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxQueueSize']"/></param>
    /// <param name="scheduledDelayMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='scheduledDelayMilliseconds']"/></param>
    /// <param name="exporterTimeoutMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporterTimeoutMilliseconds']"/></param>
    /// <param name="maxExportBatchSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxExportBatchSize']"/></param>
    public BatchPartialActivityExportProcessor(
        BaseExporter<Activity> exporter,
        SimpleLogRecordExportProcessor simpleLogRecordExportProcessor,
        int maxQueueSize = DefaultMaxQueueSize,
        int scheduledDelayMilliseconds = DefaultScheduledDelayMilliseconds,
        int exporterTimeoutMilliseconds = DefaultExporterTimeoutMilliseconds,
        int maxExportBatchSize = DefaultMaxExportBatchSize)
        : base(
            exporter,
            maxQueueSize,
            scheduledDelayMilliseconds,
            exporterTimeoutMilliseconds,
            maxExportBatchSize)
    {
        this.SimpleLogRecordExportProcessor = simpleLogRecordExportProcessor;
    }

    public override void OnStart(Activity data)
    {
        // TODO implement on start
        var logRecord = new LogRecord(
            scopeProvider: null,
            timestamp: DateTime.UtcNow,
            categoryName: "ExampleCategory",
            logLevel: LogLevel.Information,
            eventId: new EventId(1, "ExampleEvent"),
            formattedMessage: "This is a test log message.",
            state: null,
            exception: null,
            stateValues: new List<KeyValuePair<string, object?>>
            {
                new("Key1", "Value1"),
            }
        );

        this.SimpleLogRecordExportProcessor.Exporter.Export(new Batch<LogRecord>(logRecord));
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        // TODO implement on end
        this.OnExport(data);
    }

    protected override void OnExport(Activity data)
    {
        // TODO add heartbeat
        this.TryExport(data);
    }
}
