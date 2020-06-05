﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.OpcUa.Edge.Publisher.Services {
    using Microsoft.Azure.IIoT.OpcUa.Edge.Publisher.Models;
    using Microsoft.Azure.IIoT.OpcUa.Publisher.Models;
    using Microsoft.Azure.IIoT.OpcUa.Protocol;
    using Microsoft.Azure.IIoT.OpcUa.Protocol.Models;
    using Microsoft.Azure.IIoT.OpcUa.Core;
    using Microsoft.Azure.IIoT.Module;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using Prometheus;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Globalization;

    /// <summary>
    /// Writer group processing engine
    /// </summary>
    public class WriterGroupProcessingEngine : IWriterGroupProcessingEngine,
        IDisposable {

        /// <inheritdoc/>
        public string WriterGroupId { get; set; }

        /// <inheritdoc/>
        public uint? MaxNetworkMessageSize {
            get => _maxEncodedMessageSize ?? 256 * 1024;
            set {
                if (!value.HasValue || value.Value > 0) {
                    _maxEncodedMessageSize = value;
                }
            }
        }

        /// <inheritdoc/>
        public TimeSpan? PublishingInterval {
            get => _publishingInterval;
            set {
                if (_publishingInterval != value) {
                    _engine?.SetBatchTriggerInterval(value);
                    _publishingInterval = value;
                }
            }
        }

        /// <inheritdoc/>
        public int? BatchSize {
            get => _batchSize ?? 1;
            set {
                if (!value.HasValue || value.Value > 0) {
                    _batchSize = value;
                }
            }
        }

        /// <inheritdoc/>
        public TimeSpan? DiagnosticsInterval {
            get => _diagnosticsInterval;
            set {
                if (_diagnosticsInterval != value) {
                    if (value == null) {
                        _diagnosticsOutputTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    else {
                        _diagnosticsOutputTimer.Change(value.Value, value.Value);
                    }
                    _diagnosticsInterval = value;
                }
            }
        }

        /// <inheritdoc/>
        public string MessageSchema { get; set; }

        /// <inheritdoc/>
        public string HeaderLayoutUri { get; set; }

        /// <inheritdoc/>
        public NetworkMessageContentMask? NetworkMessageContentMask { get; set; }

        /// <inheritdoc/>
        public uint? GroupVersion { get; set; }

        /// <inheritdoc/>
        public DataSetOrderingType? DataSetOrdering { get; set; }

        /// <inheritdoc/>
        public double? SamplingOffset { get; set; }

        /// <inheritdoc/>
        public List<double> PublishingOffset { get; set; }

        /// <inheritdoc/>
        public TimeSpan? KeepAliveTime { get; set; }

        /// <inheritdoc/>
        public byte? Priority { get; set; }

        /// <summary>
        /// Publisher id
        /// </summary>
        internal string PublisherId => _events.DeviceId + "_" + _events.ModuleId;

        /// <summary>
        /// Create writer group processor
        /// </summary>
        /// <param name="encoders"></param>
        /// <param name="events"></param>
        /// <param name="subscriptions"></param>
        /// <param name="logger"></param>
        public WriterGroupProcessingEngine(IEventEmitter events, ISubscriptionManager subscriptions,
            IEnumerable<INetworkMessageEncoder> encoders, ILogger logger) {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
            _encoders = encoders?.ToDictionary(e => e.MessageScheme, e => e) ??
                throw new ArgumentNullException(nameof(encoders));

            if (_encoders.Count == 0) {
                // Add at least one encoder
                _encoders.Add(MessageSchemaTypes.NetworkMessageUadp, new UadpNetworkMessageEncoder());
            }

            _engine = new DataFlowEngine(this);
            _writers = new ConcurrentDictionary<string, DataSetWriterSubscription>();
            _diagnosticsOutputTimer = new Timer(DiagnosticsOutputTimer_Elapsed);
        }

        /// <inheritdoc/>
        public void AddWriters(IEnumerable<DataSetWriterModel> dataSetWriters) {

            // TODO capture tasks

            foreach (var writer in dataSetWriters) {
                _writers.AddOrUpdate(writer.DataSetWriterId, writerId => {
                    var subscription = new DataSetWriterSubscription(this, writer);
                    subscription.OpenAsync().ContinueWith(_ => subscription.ActivateAsync());
                    return subscription;
                }, (writerId, subscription) => {
                    subscription.DeactivateAsync().ContinueWith(_ => subscription.Dispose());
                    subscription = new DataSetWriterSubscription(this, writer);
                    subscription.OpenAsync().ContinueWith(_ => subscription.ActivateAsync());
                    return subscription;
                });
            }
        }

        /// <inheritdoc/>
        public void RemoveWriters(IEnumerable<string> dataSetWriters) {

            // TODO capture tasks

            foreach (var writer in dataSetWriters) {
                if (_writers.TryRemove(writer, out var subscription)) {
                    // TODO: Add cleanup
                    subscription.DeactivateAsync().ContinueWith(_ => subscription.Dispose());
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            var subscriptions = _writers.Values.ToList();
            _writers.Clear();
            try {
                // Stop
                _engine.Dispose();
                Task.WhenAll(subscriptions.Select(sc => sc.DeactivateAsync())).Wait();
            }
            catch {
                // Nothing...
            }
            finally {
                subscriptions.ForEach(sc => sc.Dispose());
                _diagnosticsOutputTimer.Dispose();
            }
        }

        /// <summary>
        /// Message flow engine
        /// </summary>
        private sealed class DataFlowEngine {

            /// <summary>
            /// Input messages
            /// </summary>
            public int SourceMessageCount => _inputBlock.OutputCount;

            /// <summary>
            /// Encoding input
            /// </summary>
            public int EncodingInputCount => _encodingBlock.InputCount;

            /// <summary>
            /// Encoding output
            /// </summary>
            public int EncodingOutputCount => _encodingBlock.OutputCount;

            /// <summary>
            /// Sent pending
            /// </summary>
            public int SentPendingCount => _sinkBlock.InputCount;

            /// <summary>
            /// Sent messages
            /// </summary>
            public long SentCompleteCount => _sentCompleteCount;

            /// <summary>
            /// Create engine
            /// </summary>
            /// <param name="outer"></param>
            internal DataFlowEngine(WriterGroupProcessingEngine outer) {
                _outer = outer ?? throw new ArgumentNullException(nameof(outer));
                _logger = _outer._logger?.ForContext<WriterGroupProcessingEngine>() ??
                    throw new ArgumentNullException(nameof(_logger));
                _batchTriggerIntervalTimer = new Timer(BatchTriggerIntervalTimer_Elapsed);
                _cts = new CancellationTokenSource();

                // Input
                _inputBlock = new BatchBlock<DataSetMessageModel>(
                    _outer.BatchSize.Value, new GroupingDataflowBlockOptions {
                        CancellationToken = _cts.Token,
                        EnsureOrdered = true
                    });

                // Encoder
                _encodingBlock = new TransformManyBlock<DataSetMessageModel[], NetworkMessageModel>(
                    _outer.EncodeMessages,
                    new ExecutionDataflowBlockOptions {
                        SingleProducerConstrained = true,
                        EnsureOrdered = true,
                        CancellationToken = _cts.Token
                    });

                // Writer
                _sinkBlock = new ActionBlock<NetworkMessageModel>(
                    SendAsync,
                    new ExecutionDataflowBlockOptions {
                        MaxDegreeOfParallelism = 1,
                        EnsureOrdered = true,
                        CancellationToken = _cts.Token
                    });

                // Link it all up
                _inputBlock.LinkTo(_encodingBlock, new DataflowLinkOptions {
                    PropagateCompletion = true
                });
                _encodingBlock.LinkTo(_sinkBlock, new DataflowLinkOptions {
                    PropagateCompletion = true
                });
            }

            /// <inheritdoc/>
            public void SetBatchTriggerInterval(TimeSpan? value) {
                if (value == null) {
                    _batchTriggerIntervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                else {
                    _batchTriggerIntervalTimer.Change(value.Value, value.Value);
                }
            }

            /// <summary>
            /// process message
            /// </summary>
            /// <param name="message"></param>
            public void Enqueue(DataSetMessageModel message) {
                _inputBlock.Post(message);
            }

            /// <inheritdoc/>
            public void Dispose() {
                try {
                    // Stop
                    _inputBlock.Complete();
                    _cts.Cancel();
                }
                catch {
                    // Nothing...
                }
                finally {
                    _batchTriggerIntervalTimer.Dispose();
                    _cts.Dispose();
                }
            }

            /// <summary>
            /// Batch trigger interval
            /// </summary>
            /// <param name="state"></param>
            private void BatchTriggerIntervalTimer_Elapsed(object state) {
                _inputBlock.TriggerBatch();
            }

            /// <summary>
            /// Send messages
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            private async Task SendAsync(NetworkMessageModel message) {
                if (message == null) {
                    return;
                }
                try {
                    using (kSendingDuration.NewTimer()) {
                        await _outer._events.SendEventAsync(message.Body,
                            message.ContentType, message.MessageSchema, message.ContentEncoding);
                    }
                    Interlocked.Increment(ref _sentCompleteCount);
                    kMessagesSent.WithLabels(_iotHubMessageSinkGuid, _iotHubMessageSinkStartTime).Inc();
                }
                catch (Exception ex) {
                    _logger.Error(ex, "Error while sending messages to IoT Hub.");
                    // we do not set the block into a faulted state.
                }
            }

            private readonly WriterGroupProcessingEngine _outer;
            private readonly ILogger _logger;
            private readonly Timer _batchTriggerIntervalTimer;
            private readonly BatchBlock<DataSetMessageModel> _inputBlock;
            private readonly CancellationTokenSource _cts;
            private readonly TransformManyBlock<DataSetMessageModel[], NetworkMessageModel> _encodingBlock;
            private readonly ActionBlock<NetworkMessageModel> _sinkBlock;
            private readonly string _iotHubMessageSinkGuid = Guid.NewGuid().ToString();
            private long _sentCompleteCount;

            private readonly string _iotHubMessageSinkStartTime =
                DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
            private static readonly Histogram kSendingDuration = Metrics.CreateHistogram(
                "iiot_edge_publisher_messages_duration", "Histogram of message sending durations");
            private static readonly Gauge kMessagesSent = Metrics.CreateGauge(
                "iiot_edge_publisher_messages", "Number of messages sent to IotHub",
                    new GaugeConfiguration {
                        LabelNames = new[] { "runid", "timestamp_utc" }
                    });
        }

        /// <summary>
        /// A dataset writer
        /// </summary>
        private sealed class DataSetWriterSubscription : IDisposable {

            /// <summary>
            /// Active subscription
            /// </summary>
            public ISubscription Subscription { get; set; }

            /// <summary>
            /// Create subscription from template
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="dataSetWriter"></param>
            public DataSetWriterSubscription(WriterGroupProcessingEngine outer,
                DataSetWriterModel dataSetWriter) {

                _outer = outer ??
                    throw new ArgumentNullException(nameof(outer));
                _logger = _outer._logger?.ForContext<DataSetWriterSubscription>() ??
                    throw new ArgumentNullException(nameof(_logger));
                _dataSetWriter = dataSetWriter.Clone() ??
                    throw new ArgumentNullException(nameof(dataSetWriter));
                _subscriptionInfo = _dataSetWriter.ToSubscriptionModel();

                if (dataSetWriter.KeyFrameInterval.HasValue &&
                   dataSetWriter.KeyFrameInterval.Value > TimeSpan.Zero) {
                    _keyframeTimer = new System.Timers.Timer(
                        dataSetWriter.KeyFrameInterval.Value.TotalMilliseconds);
                    _keyframeTimer.Elapsed += KeyframeTimerElapsedAsync;
                }
                else {
                    _keyFrameCount = dataSetWriter.KeyFrameCount;
                }

                if (dataSetWriter.DataSetMetaDataSendInterval.HasValue &&
                    dataSetWriter.DataSetMetaDataSendInterval.Value > TimeSpan.Zero) {
                    _metaData = dataSetWriter.DataSet?.DataSetMetaData ??
                        throw new ArgumentNullException(nameof(dataSetWriter.DataSet));

                    _metadataTimer = new System.Timers.Timer(
                        dataSetWriter.DataSetMetaDataSendInterval.Value.TotalMilliseconds);
                    _metadataTimer.Elapsed += MetadataTimerElapsed;
                }
            }

            /// <summary>
            /// Open subscription
            /// </summary>
            /// <returns></returns>
            public async Task OpenAsync() {
                if (Subscription != null) {
                    _logger.Warning("Subscription already exists");
                    return;
                }

                var sc = await _outer._subscriptions.GetOrCreateSubscriptionAsync(
                    _subscriptionInfo);
                sc.OnSubscriptionChange += OnSubscriptionChangedAsync;
                await sc.ApplyAsync(_subscriptionInfo.MonitoredItems,
                    _subscriptionInfo.Configuration, false);
                Subscription = sc;
            }

            /// <summary>
            /// activate a subscription
            /// </summary>
            /// <returns></returns>
            public async Task ActivateAsync() {
                if (Subscription == null) {
                    _logger.Warning("Subscription not registered");
                    return;
                }

                await Subscription.ApplyAsync(_subscriptionInfo.MonitoredItems,
                    _subscriptionInfo.Configuration, true);

                if (_keyframeTimer != null) {
                    _keyframeTimer.Start();
                }

                if (_metadataTimer != null) {
                    _metadataTimer.Start();
                }
            }

            /// <summary>
            /// deactivate a subscription
            /// </summary>
            /// <returns></returns>
            public async Task DeactivateAsync() {

                if (Subscription == null) {
                    _logger.Warning("Subscription not registered");
                    return;
                }

                await Subscription.ApplyAsync(_subscriptionInfo.MonitoredItems,
                    _subscriptionInfo.Configuration, false);

                if (_keyframeTimer != null) {
                    _keyframeTimer.Stop();
                }

                if (_metadataTimer != null) {
                    _metadataTimer.Stop();
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                if (Subscription != null) {
                    Subscription.OnSubscriptionChange -= OnSubscriptionChangedAsync;
                    Subscription.ApplyAsync(null, _subscriptionInfo.Configuration, false);
                    Subscription.Dispose();
                }
                _keyframeTimer?.Dispose();
                _metadataTimer?.Dispose();
                Subscription = null;
            }

            /// <summary>
            /// Fire when keyframe timer elapsed to send keyframe message
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private async void KeyframeTimerElapsedAsync(object sender, System.Timers.ElapsedEventArgs e) {
                try {
                    _keyframeTimer.Enabled = false;

                    _logger.Debug("Insert keyframe message...");
                    var sequenceNumber = (uint)Interlocked.Increment(ref _currentSequenceNumber);
                    var snapshot = await Subscription.GetSnapshotAsync();
                    if (snapshot != null) {
                        _outer.ProcessDataSetWriterNotification(_dataSetWriter, sequenceNumber, snapshot);
                    }
                }
                catch (Exception ex) {
                    _logger.Information(ex, "Failed to send keyframe.");
                }
                finally {
                    _keyframeTimer.Enabled = true;
                }
            }

            /// <summary>
            /// Fired when metadata time elapsed
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void MetadataTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
                // Send(_metaData)
            }

            /// <summary>
            /// Handle subscription change messages
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="notification"></param>
            private async void OnSubscriptionChangedAsync(
                object sender, SubscriptionNotificationModel notification) {
                var sequenceNumber = (uint)Interlocked.Increment(ref _currentSequenceNumber);
                if (_keyFrameCount.HasValue && _keyFrameCount.Value != 0 &&
                    (sequenceNumber % _keyFrameCount.Value) == 0) {
                    var snapshot = await Try.Async(() => Subscription.GetSnapshotAsync());
                    if (snapshot != null) {
                        notification = snapshot;
                    }
                }
                _outer.ProcessDataSetWriterNotification(_dataSetWriter, sequenceNumber, notification);
            }

            private readonly System.Timers.Timer _keyframeTimer;
            private readonly System.Timers.Timer _metadataTimer;
            private readonly DataSetMetaDataModel _metaData;
            private readonly uint? _keyFrameCount;
            private long _currentSequenceNumber;
            private readonly WriterGroupProcessingEngine _outer;
            private readonly DataSetWriterModel _dataSetWriter;
            private readonly SubscriptionModel _subscriptionInfo;
            private readonly ILogger _logger;
        }

        /// <summary>
        /// handle subscription change messages
        /// </summary>
        /// <param name="dataSetWriter"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="notification"></param>
        private void ProcessDataSetWriterNotification(DataSetWriterModel dataSetWriter,
            uint sequenceNumber, SubscriptionNotificationModel notification) {
            try {
                var notifications = notification.Notifications.ToList();
                var message = new DataSetMessageModel {
                    // TODO: Filter changes on the monitored items contained in the template
                    Notifications = notifications,
                    ServiceMessageContext = notification.ServiceMessageContext,
                    SubscriptionId = notification.SubscriptionId,
                    SequenceNumber = sequenceNumber,
                    ContentMask = (uint?)NetworkMessageContentMask,
                    PublisherId = PublisherId,
                    ApplicationUri = notification.ApplicationUri,
                    EndpointUrl = notification.EndpointUrl,
                    TimeStamp = notification.Timestamp,
                    Writer = dataSetWriter
                };
                Interlocked.Add(ref _valueChangesCount, notifications.Count);
                Interlocked.Increment(ref _dataChangesCount);
                _engine.Enqueue(message);
            }
            catch (Exception ex) {
                _logger.Debug(ex, "Failed to produce message");
            }
        }

        /// <summary>
        /// Encode dataset messages using the configured encoder
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> EncodeMessages(IEnumerable<DataSetMessageModel> input) {
            // Select encoder
            if (MessageSchema == null || !_encoders.TryGetValue(MessageSchema, out var encoder)) {
                // Use first encoder
                encoder = _encoders.First().Value;
            }
            if (BatchSize.Value == 1) {
                return encoder.Encode(input, (int)MaxNetworkMessageSize.Value);
            }
            return encoder.EncodeBatch(input, (int)MaxNetworkMessageSize.Value);
        }

        /// <summary>
        /// Diagnostics timer
        /// </summary>
        /// <param name="state"></param>
        private void DiagnosticsOutputTimer_Elapsed(object state) {
            var totalDuration = (DateTime.UtcNow - _diagnosticStart).TotalSeconds;
            var numberOfConnectionRetries = _writers.Values
                .Where(sc => sc.Subscription != null)
                .Select(sc => sc.Subscription)
                .Sum(sc => sc.NumberOfConnectionRetries);

            if (_dataChangesCount > 0 || _valueChangesCount > 0 || _engine.SentCompleteCount > 0) {
                var diagInfo = new StringBuilder();
                diagInfo.AppendLine();
                diagInfo.AppendLine("   DIAGNOSTICS INFORMATION for         : {deviceId}; {moduleId}");
                diagInfo.AppendLine("   # Ingestion duration                : {duration,14:dd\\:hh\\:mm\\:ss} (dd:hh:mm:ss)");
                diagInfo.AppendLine("   # Ingress DataChanges (from OPC)    : {dataChangesCount,14:0}({dataChangesAverage:0.##}/s)");
                diagInfo.AppendLine("   # Ingress ValueChanges (from OPC)   : {valueChangesCount,14:0}({valueChangesAverage:0.##}/s)");

                diagInfo.AppendLine("   # Ingress BatchBlock buffer size    : {batchDataSetMessageBlockOutputCount,14:0}");
                diagInfo.AppendLine("   # Encoding Block input/output size  : {encodingBlockInputCount,14:0} | {encodingBlockOutputCount:0}");
             // diagInfo.AppendLine("   # Encoder Notifications processed   : {notificationsProcessedCount,14:0}");
             // diagInfo.AppendLine("   # Encoder Notifications dropped     : {notificationsDroppedCount,14:0}");
             // diagInfo.AppendLine("   # Encoder IoT Messages processed    : {messagesProcessedCount,14:0}");
             // diagInfo.AppendLine("   # Encoder avg Notifications/Message : {notificationsPerMessage,14:0}");
             // diagInfo.AppendLine("   # Encoder avg IoT Message body size : {messageSizeAverage,14:0}");
                diagInfo.AppendLine("   # Outgress Batch Block buffer size  : {batchNetworkMessageBlockOutputCount,14:0}");
                diagInfo.AppendLine("   # Outgress input buffer count       : {sinkBlockInputCount,14:0}");
                diagInfo.AppendLine("   # Outgress IoT message count        : {messageSinkSentMessagesCount,14:0}({sentMessagesAverage:0.##}/s)");
                diagInfo.AppendLine("   # Connection retries                : {connectionRetries,14:0}");

                var dataChangesAverage = _dataChangesCount > 0 && totalDuration > 0 ?
                    _dataChangesCount / totalDuration : 0;
                var valueChangesAverage = _valueChangesCount > 0 && totalDuration > 0 ?
                    _valueChangesCount / totalDuration : 0;
                var sentMessagesAverage = _engine.SentCompleteCount > 0 && totalDuration > 0 ?
                    _engine.SentCompleteCount / totalDuration : 0;

                _logger.Information(diagInfo.ToString(),
                    _events.DeviceId, _events.ModuleId,
                    TimeSpan.FromSeconds(totalDuration),
                    _dataChangesCount, dataChangesAverage,
                    _valueChangesCount, valueChangesAverage,
                    _engine?.SourceMessageCount,
                    _engine?.EncodingInputCount, _engine?.EncodingOutputCount,
                  // _messageEncoder.NotificationsProcessedCount,
                  // _messageEncoder.NotificationsDroppedCount,
                  // _messageEncoder.MessagesProcessedCount,
                  // _messageEncoder.AvgNotificationsPerMessage,
                  // _messageEncoder.AvgMessageSize,
                    _engine?.SentPendingCount,
                    _engine.SentCompleteCount, sentMessagesAverage,
                    numberOfConnectionRetries);
            }

            kDataChangesCount.WithLabels(PublisherId, WriterGroupId)
                .Set(_dataChangesCount);
            kDataChangesPerSecond.WithLabels(PublisherId, WriterGroupId)
                .Set(_valueChangesCount / totalDuration);
            kValueChangesCount.WithLabels(PublisherId, WriterGroupId)
                .Set(_valueChangesCount);
            kValueChangesPerSecond.WithLabels(PublisherId, WriterGroupId)
                .Set(_valueChangesCount / totalDuration);
        //    kNotificationsProcessedCount.WithLabels(PublisherId, WriterGroupId)
        //        .Set(_messageEncoder.NotificationsProcessedCount);
        //    kNotificationsDroppedCount.WithLabels(PublisherId, WriterGroupId)
        //        .Set(_messageEncoder.NotificationsDroppedCount);
        //    kMessagesProcessedCount.WithLabels(PublisherId, WriterGroupId)
        //        .Set(_messageEncoder.MessagesProcessedCount);
        //    kNotificationsPerMessageAvg.WithLabels(PublisherId, WriterGroupId)
        //        .Set(_messageEncoder.AvgNotificationsPerMessage);
        //    kMesageSizeAvg.WithLabels(PublisherId, WriterGroupId)
        //        .Set(_messageEncoder.AvgMessageSize);
            kIoTHubQueueBuffer.WithLabels(PublisherId, WriterGroupId)
                .Set((long)_engine?.SentPendingCount);
            kSentMessagesCount.WithLabels(PublisherId, WriterGroupId)
                .Set(_engine.SentCompleteCount);
            kNumberOfConnectionRetries.WithLabels(PublisherId, WriterGroupId)
                .Set(numberOfConnectionRetries);
        }

        // Services
        private readonly Dictionary<string, INetworkMessageEncoder> _encoders;
        private readonly ISubscriptionManager _subscriptions;
        private readonly IEventEmitter _events;
        private readonly ILogger _logger;

        // State
        private readonly ConcurrentDictionary<string, DataSetWriterSubscription> _writers;
        private uint? _maxEncodedMessageSize;
        private int? _batchSize;
        private TimeSpan? _publishingInterval;
        private readonly DataFlowEngine _engine;

        // Diagnostics
        private readonly DateTime _diagnosticStart = DateTime.UtcNow;
        private TimeSpan? _diagnosticsInterval;
        private readonly Timer _diagnosticsOutputTimer;
        private long _valueChangesCount;
        private long _dataChangesCount;

        private static readonly GaugeConfiguration kGaugeConfig = new GaugeConfiguration {
            LabelNames = new[] { "publisherid", "triggerid" }
        };

        private static readonly Gauge kValueChangesCount = Metrics.CreateGauge(
            "iiot_edge_publisher_value_changes", "Opc ValuesChanges delivered for processing", kGaugeConfig);
        private static readonly Gauge kValueChangesPerSecond = Metrics.CreateGauge(
            "iiot_edge_publisher_value_changes_per_second", "Opc ValuesChanges/second delivered for processing", kGaugeConfig);
        private static readonly Gauge kDataChangesCount = Metrics.CreateGauge(
            "iiot_edge_publisher_data_changes", "Opc DataChanges delivered for processing", kGaugeConfig);
        private static readonly Gauge kDataChangesPerSecond = Metrics.CreateGauge(
            "iiot_edge_publisher_data_changes_per_second", "Opc DataChanges/second delivered for processing", kGaugeConfig);
        private static readonly Gauge kIoTHubQueueBuffer = Metrics.CreateGauge(
            "iiot_edge_publisher_iothub_queue_size", "IoT messages queued sending", kGaugeConfig);
        private static readonly Gauge kSentMessagesCount = Metrics.CreateGauge(
            "iiot_edge_publisher_sent_iot_messages", "IoT messages sent to hub", kGaugeConfig);
        private static readonly Gauge kNumberOfConnectionRetries = Metrics.CreateGauge(
            "iiot_edge_publisher_connection_retries", "OPC UA connect retries", kGaugeConfig);

        private static readonly Gauge kNotificationsProcessedCount = Metrics.CreateGauge(
            "iiot_edge_publisher_encoded_notifications", "publisher engine encoded opc notifications count", kGaugeConfig);
        private static readonly Gauge kNotificationsDroppedCount = Metrics.CreateGauge(
            "iiot_edge_publisher_dropped_notifications", "publisher engine dropped opc notifications count", kGaugeConfig);
        private static readonly Gauge kMessagesProcessedCount = Metrics.CreateGauge(
            "iiot_edge_publisher_processed_messages", "publisher engine processed iot messages count", kGaugeConfig);
        private static readonly Gauge kNotificationsPerMessageAvg = Metrics.CreateGauge(
            "iiot_edge_publisher_notifications_per_message_average",
            "publisher engine opc notifications per iot message average", kGaugeConfig);
        private static readonly Gauge kMesageSizeAvg = Metrics.CreateGauge(
            "iiot_edge_publisher_encoded_message_size_average",
            "publisher engine iot message encoded body size average", kGaugeConfig);
    }
}
