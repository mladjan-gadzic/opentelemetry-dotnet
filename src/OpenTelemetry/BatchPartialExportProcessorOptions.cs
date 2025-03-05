// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

namespace OpenTelemetry;

/// <summary>
/// Contains batch export processor options.
/// </summary>
/// <typeparam name="T">The type of telemetry object to be exported.</typeparam>
public class BatchPartialExportProcessorOptions<T>
    where T : class
{
    /// <summary>
    /// Gets or sets the maximum queue size. The queue drops the data if the maximum size is reached. The default value is 2048.
    /// </summary>
    public int MaxQueueSize { get; set; } = BatchPartialExportProcessor<T>.DefaultMaxQueueSize;

    /// <summary>
    /// Gets or sets the delay interval (in milliseconds) between two consecutive exports. The default value is 5000.
    /// </summary>
    public int ScheduledDelayMilliseconds { get; set; } = BatchPartialExportProcessor<T>.DefaultScheduledDelayMilliseconds;

    /// <summary>
    /// Gets or sets the timeout (in milliseconds) after which the export is cancelled. The default value is 30000.
    /// </summary>
    public int ExporterTimeoutMilliseconds { get; set; } = BatchPartialExportProcessor<T>.DefaultExporterTimeoutMilliseconds;

    /// <summary>
    /// Gets or sets the maximum batch size of every export. It must be smaller or equal to MaxQueueLength. The default value is 512.
    /// </summary>
    public int MaxExportBatchSize { get; set; } = BatchPartialExportProcessor<T>.DefaultMaxExportBatchSize;
}
