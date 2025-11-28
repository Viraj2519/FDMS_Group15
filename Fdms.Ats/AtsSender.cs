/* 
 *  FILE          : AtsSender.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the AtsSender class and encapsulates all 
    *    variables, constants, and logic for handling ATS data transmission.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fdms.Shared;
using System.Data.SqlClient;

namespace Fdms.Ats
{
    //-------------------------------------------------------------
    // CLASS : AtsSender
    // PURPOSE : Handles the transmission of telemetry data to the ground terminal.
    //-------------------------------------------------------------
    public sealed class AtsSender : IDisposable
    {
        private readonly AtsConfig _config;
        private readonly TelemetryFileReader _fileReader;
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _groundEndpoint;

        private bool _disposed;

        //-------------------------------------------------------------
        // FUNCTION : AtsSender
        // PURPOSE :
        //   Initializes a new instance of the AtsSender class with the specified configuration.
        // PARAMETERS :
        //   config - The AtsConfig instance containing configuration settings.
        // RETURNS : None
        //-------------------------------------------------------------
        public AtsSender(AtsConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _fileReader = new TelemetryFileReader(_config.TelemetryFilePath);
            _udpClient = new UdpClient();

            var ipAddress = IPAddress.Parse(_config.GroundTerminalIp);
            _groundEndpoint = new IPEndPoint(ipAddress, _config.GroundTerminalPort);

            _udpClient.Connect(_groundEndpoint);
        }

        //-------------------------------------------------------------
        // FUNCTION : Run
        // PURPOSE :
        //   Starts the transmission of telemetry data to the ground terminal.
        // PARAMETERS : None
        // RETURNS : None
        //-------------------------------------------------------------
        public void Run()
        {
            Console.WriteLine("[ATS] Starting Aircraft Transmission System...");
            Console.WriteLine($"[ATS] Ground Terminal: {_groundEndpoint.Address}:{_groundEndpoint.Port}");
            Console.WriteLine($"[ATS] Tail Number: {_config.TailNumber}");
            Console.WriteLine($"[ATS] Telemetry File: {_config.TelemetryFilePath}");
            Console.WriteLine($"[ATS] Send Interval: {_config.SendIntervalSeconds:F2} seconds");
            Console.WriteLine();

            int sequenceNumber = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var record in _fileReader.ReadTelemetryRecords())
            {
                int checksum = ChecksumCalculator.CalculateChecksum(record);

                var packet = new TelemetryPacket(
                    _config.TailNumber,
                    sequenceNumber,
                    record,
                    checksum);

                string packetString = PacketSerializer.Serialize(packet);
                byte[] payload = Encoding.ASCII.GetBytes(packetString);

                try
                {
                    _udpClient.Send(payload, payload.Length);
                }
                catch (SocketException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ATS] Socket error while sending packet #{sequenceNumber}: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine($"[ATS] Sent packet #{sequenceNumber}: {packetString}");

                sequenceNumber++;

                double targetSeconds = sequenceNumber * _config.SendIntervalSeconds;
                double remaining = targetSeconds - stopwatch.Elapsed.TotalSeconds;

                if (remaining > 0)
                {
                    SleepForInterval(remaining);
                }
            }

            Console.WriteLine();
            Console.WriteLine("[ATS] End of telemetry file reached. Transmission complete.");
        }

        //-------------------------------------------------------------
        // FUNCTION : SleepForInterval
        // PURPOSE :
        //   Sleeps the current thread for the specified interval in seconds.
        // PARAMETERS :
        //   seconds - The interval to sleep in seconds.
        // RETURNS : None
        //-------------------------------------------------------------
        private static void SleepForInterval(double seconds)
        {
            if (seconds <= 0)
            {
                return;
            }

            int milliseconds = (int)Math.Round(seconds * 1000.0);
            if (milliseconds < 1)
            {
                milliseconds = 1;
            }

            Thread.Sleep(milliseconds);
        }

        //-------------------------------------------------------------
        // FUNCTION : Dispose
        // PURPOSE :
        //   Disposes of the AtsSender instance and releases resources.
        // PARAMETERS : None
        // RETURNS : None
        //-------------------------------------------------------------
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _udpClient.Dispose();
        }
    }
}