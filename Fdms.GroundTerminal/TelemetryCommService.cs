/* 
 *  FILE          : TelemetryCommService.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Viraj Solanki and Darsh Patel
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file defines the TelemetryCommService class which handles
 *    receiving telemetry packets over UDP, validating them, and raising events.
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fdms.Shared;
using System.Data.SqlClient;

namespace Fdms.GroundTerminal
{
    //-------------------------------------------------------------
    // CLASS : TelemetryCommService
    // PURPOSE :
    //   Handles receiving telemetry packets over UDP, validating them,
    //   and raising events.
    //-------------------------------------------------------------
    public sealed class TelemetryCommService : IDisposable
    {
        private readonly int _listenPort;
        private readonly UdpClient _udpClient;
        private CancellationTokenSource? _cts;
        private Task? _receiveTask;
        private bool _disposed;

        public event EventHandler<ValidPacketReceivedEventArgs>? ValidPacketReceived;
        public event EventHandler<InvalidPacketReceivedEventArgs>? InvalidPacketReceived;

        /*---------------------------------------------------------
        *  FUNCTION      : TelemetryCommService
        *  DESCRIPTION   :
        *    Initializes a new instance of the TelemetryCommService class
        *    to listen for telemetry packets on the specified UDP port.
        *  PARAMETERS    : 
        *      listenPort - The UDP port to listen on for incoming telemetry packets.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public TelemetryCommService(int listenPort)
        {
            _listenPort = listenPort;
            _udpClient = new UdpClient(_listenPort);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : Start
        *  DESCRIPTION   :
        *    Starts the telemetry communication service to begin
        *    receiving telemetry packets.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Start()
        {
            if (_cts != null)
                throw new InvalidOperationException("TelemetryCommService is already started.");

            _cts = new CancellationTokenSource();
            _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            Console.WriteLine($"[GTS] TelemetryCommService listening on port {_listenPort} (UDP).");
        }

        /*---------------------------------------------------------
        *  FUNCTION      : Stop
        *  DESCRIPTION   :
        *    Stops the telemetry communication service.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Stop()
        {
            if (_cts == null)
                return;

            _cts.Cancel();
            try
            {
                _receiveTask?.Wait();
            }
            catch (AggregateException)
            {
            }

            _cts.Dispose();
            _cts = null;
            _receiveTask = null;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ReceiveLoopAsync
        *  DESCRIPTION   :
        *    Asynchronous loop to receive telemetry packets over UDP,
        *    validate them, and raise events.
        *  PARAMETERS    :
        *      cancellationToken - Token to monitor for cancellation requests.
        *  RETURNS       : Task
        *---------------------------------------------------------*/
        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result;

                try
                {
                    result = await _udpClient.ReceiveAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[GTS] Socket error in ReceiveLoop: {ex.Message}");
                    Console.ResetColor();
                    continue;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[GTS] Unexpected error in ReceiveLoop: {ex.Message}");
                    Console.ResetColor();
                    continue;
                }

                string rawPacket = Encoding.ASCII.GetString(result.Buffer);

                if (!PacketSerializer.TryDeserialize(rawPacket, out var packet, out string parseError))
                {
                    OnInvalidPacketReceived(new InvalidPacketReceivedEventArgs(
                        rawPacket,
                        null,
                        null,
                        "ParseError",
                        parseError,
                        null,
                        null));
                    continue;
                }

                int expectedChecksum = ChecksumCalculator.CalculateChecksum(packet.Telemetry);
                if (expectedChecksum != packet.Checksum)
                {
                    string reason = "ChecksumMismatch";
                    string details = $"Expected={expectedChecksum}, Received={packet.Checksum}";

                    OnInvalidPacketReceived(new InvalidPacketReceivedEventArgs(
                        rawPacket,
                        packet.TailNumber,
                        packet.SequenceNumber,
                        reason,
                        details,
                        expectedChecksum,
                        packet.Checksum));
                    continue;
                }

                OnValidPacketReceived(new ValidPacketReceivedEventArgs(rawPacket, packet));
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : OnValidPacketReceived
        *  DESCRIPTION   :
        *    Raises the ValidPacketReceived event.
        *  PARAMETERS    :
        *      e - The ValidPacketReceivedEventArgs instance.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void OnValidPacketReceived(ValidPacketReceivedEventArgs e)
        {
            ValidPacketReceived?.Invoke(this, e);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : OnInvalidPacketReceived
        *  DESCRIPTION   :
        *    Raises the InvalidPacketReceived event.
        *  PARAMETERS    :
        *      e - The InvalidPacketReceivedEventArgs instance.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void OnInvalidPacketReceived(InvalidPacketReceivedEventArgs e)
        {
            InvalidPacketReceived?.Invoke(this, e);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : Dispose
        *  DESCRIPTION   :
        *    Disposes the TelemetryCommService instance and releases resources.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();
            _udpClient.Dispose();
        }
    }

    //-------------------------------------------------------------
    // CLASS : ValidPacketReceivedEventArgs
    // PURPOSE :
    //   Provides data for the ValidPacketReceived event.
    //-------------------------------------------------------------
    public sealed class ValidPacketReceivedEventArgs : EventArgs
    {
        public string RawPacket { get; }
        public TelemetryPacket Packet { get; }

        /*---------------------------------------------------------
        *  FUNCTION      : ValidPacketReceivedEventArgs
        *  DESCRIPTION   :
        *    Initializes a new instance of the ValidPacketReceivedEventArgs class.
        *  PARAMETERS    :
        *      rawPacket - The raw packet string.
        *      packet - The parsed TelemetryPacket.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public ValidPacketReceivedEventArgs(string rawPacket, TelemetryPacket packet)
        {
            RawPacket = rawPacket ?? throw new ArgumentNullException(nameof(rawPacket));
            Packet = packet ?? throw new ArgumentNullException(nameof(packet));
        }
    }

    //-------------------------------------------------------------
    // CLASS : InvalidPacketReceivedEventArgs
    // PURPOSE :
    //   Provides data for the InvalidPacketReceived event.
    //-------------------------------------------------------------
    public sealed class InvalidPacketReceivedEventArgs : EventArgs
    {
        public string RawPacket { get; }
        public string? TailNumber { get; }
        public int? SequenceNumber { get; }

        public string ReasonCode { get; }
        public string Details { get; }

        public int? ExpectedChecksum { get; }
        public int? ActualChecksum { get; }

        /*---------------------------------------------------------
        *  FUNCTION      : InvalidPacketReceivedEventArgs
        *  DESCRIPTION   :
        *    Initializes a new instance of the InvalidPacketReceivedEventArgs class.
        *  PARAMETERS    :
        *      rawPacket - The raw packet string.
        *      tailNumber - The tail number if available.
        *      sequenceNumber - The sequence number if available.
        *      reasonCode - The reason code for invalidity.
        *      details - Additional details about the invalidity.
        *      expectedChecksum - The expected checksum if applicable.
        *      actualChecksum - The actual checksum if applicable.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public InvalidPacketReceivedEventArgs(string rawPacket, string? tailNumber, int? sequenceNumber, string reasonCode, string details, int? expectedChecksum, int? actualChecksum)
        {
            RawPacket = rawPacket ?? throw new ArgumentNullException(nameof(rawPacket));
            TailNumber = tailNumber;
            SequenceNumber = sequenceNumber;
            ReasonCode = reasonCode ?? "Unknown";
            Details = details ?? string.Empty;
            ExpectedChecksum = expectedChecksum;
            ActualChecksum = actualChecksum;
        }
    }
}
