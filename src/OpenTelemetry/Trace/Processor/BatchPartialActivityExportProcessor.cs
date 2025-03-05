// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Diagnostics;
using OpenTelemetry.Logs;

namespace OpenTelemetry;

/// <summary>
/// Implements processor that batches <see cref="Activity"/> objects before calling exporter.
/// </summary>
public class BatchPartialActivityExportProcessor : BatchPartialExportProcessor<Activity>
{
    private readonly BaseExportProcessor<LogRecord> baseExportProcessor;
    private readonly ConcurrentDictionary<ActivitySpanId, Activity> activeActivities;
    private readonly ConcurrentQueue<KeyValuePair<ActivitySpanId, Activity>> endedActivities;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchActivityExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporter']"/></param>
    /// <param name="baseExportProcessor"><inheritdoc cref="BaseExportProcessor{LogRecord}" path="/param[@name='baseExportProcessor']"/></param>
    /// <param name="maxQueueSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxQueueSize']"/></param>
    /// <param name="scheduledDelayMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='scheduledDelayMilliseconds']"/></param>
    /// <param name="exporterTimeoutMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporterTimeoutMilliseconds']"/></param>
    /// <param name="maxExportBatchSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxExportBatchSize']"/></param>
    public BatchPartialActivityExportProcessor(
        BaseExporter<Activity> exporter,
        BaseExportProcessor<LogRecord> baseExportProcessor,
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
        this.baseExportProcessor = baseExportProcessor;
        this.activeActivities = new ConcurrentDictionary<ActivitySpanId, Activity>();
        this.endedActivities = new ConcurrentQueue<KeyValuePair<ActivitySpanId, Activity>>();
    }

    public override void OnStart(Activity data)
    {
        var logRecord = GetLogRecord(data, this.GetHeartbeatLogRecordAttributes());
        this.baseExportProcessor.Exporter.Export(new Batch<LogRecord>(logRecord));

        this.activeActivities[data.SpanId] = data;
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        this.TryExport(data);

        var logRecordAttributes = new List<KeyValuePair<string, object?>>
        {
            new("partial.event", "stop"),
        };
        var logRecord = GetLogRecord(data, logRecordAttributes);
        this.baseExportProcessor.Exporter.Export(new Batch<LogRecord>(logRecord));

        this.endedActivities.Enqueue(new KeyValuePair<ActivitySpanId, Activity>(data.SpanId, data));
    }

    protected override void Heartbeat()
    {
        // remove ended activities from active activities
        while (this.endedActivities.TryDequeue(out var activity))
        {
            this.activeActivities.TryRemove(activity.Key, out _);
        }

        foreach (var keyValuePair in this.activeActivities)
        {
            LogRecord logRecord = GetLogRecord(keyValuePair.Value, this.GetHeartbeatLogRecordAttributes());
            this.baseExportProcessor.Exporter.Export(new Batch<LogRecord>(logRecord));
        }
    }

    private List<KeyValuePair<string, object?>> GetHeartbeatLogRecordAttributes() =>
    [
        new("partial.event", "heartbeat"),
        new("partial.frequency", this.ScheduledDelayMilliseconds + "ms")
    ];

    protected override void OnExport(Activity data) => this.TryExport(data);

    private static List<KeyValuePair<string, object?>> GetLogRecordAttributes() =>
    [
        new("telemetry.logs.cluster", "partial"),
        new("telemetry.logs.project", "span"),
    ];

    private static LogRecord GetLogRecord(
        Activity data,
        List<KeyValuePair<string, object?>> logRecordAttributesToBeAdded)
    {
        var logRecord = new LogRecord
        {
            Timestamp = DateTime.UtcNow,
            TraceId = data.TraceId,
            SpanId = data.SpanId,
            TraceFlags = ActivityTraceFlags.None,
            Severity = LogRecordSeverity.Info,
            SeverityText = "Info",
            Body = "Here goes serialized proto span",
        };
        var logRecordAttributes = GetLogRecordAttributes();
        logRecordAttributes.AddRange(logRecordAttributesToBeAdded);
        logRecord.Attributes = logRecordAttributes;
        return logRecord;
    }
}
