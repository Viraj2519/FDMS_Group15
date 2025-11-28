/* 
 *  FILE          : AtsConfig.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the AtsConfig class and encapsulates all 
    *    variables, constants, and logic for handling ATS configuration.
 */
using System;
using System.IO;

namespace Fdms.Ats
{
    //-------------------------------------------------------------
    // CLASS : AtsConfig
    // PURPOSE :
    //   Encapsulates configuration settings for the ATS application.
    //-------------------------------------------------------------
    public sealed class AtsConfig
    {
        public string GroundHost { get; }
        public int GroundPort { get; }
        public string TailNumber { get; }
        public string TelemetryFilePath { get; }
        public TimeSpan SendInterval { get; }

        public string GroundTerminalIp => GroundHost;
        public int GroundTerminalPort => GroundPort;
        public double SendIntervalSeconds => SendInterval.TotalSeconds;

        /*---------------------------------------------------------
        *  FUNCTION      : AtsConfig
        *  DESCRIPTION   :
        *    initialize an AtsConfig instance with
        *    provided configuration values.
        *  PARAMETERS    : 
        *      groundHost - The hostname or IP address of the ground terminal.
        *      groundPort - The port number of the ground terminal.
        *      tailNumber - The aircraft tail number.
        *      telemetryFilePath - The file path for telemetry data.
        *      sendInterval - The interval at which data is sent.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private AtsConfig(string groundHost, int groundPort, string tailNumber, string telemetryFilePath, TimeSpan sendInterval)
        {
            GroundHost = groundHost;
            GroundPort = groundPort;
            TailNumber = tailNumber;
            TelemetryFilePath = telemetryFilePath;
            SendInterval = sendInterval;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : FromArgsOrDefaults
        *  DESCRIPTION   :
        *    Creates an AtsConfig instance from command-line arguments
        *    or default values if arguments are not provided.
        *  PARAMETERS    :
        *      args - Command-line arguments.
        *  RETURNS       : An AtsConfig instance.
        *---------------------------------------------------------*/
        public static AtsConfig FromArgsOrDefaults(string[] args)
        {
            string groundHost = "127.0.0.1";
            int groundPort = 5000;
            string tailNumber = "C-FGAX";
            double sendIntervalSeconds = 1.0;

            if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
            {
                tailNumber = args[0].Trim().ToUpperInvariant();
            }

            string telemetryFileName = $"{tailNumber}.txt";

            if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
            {
                telemetryFileName = args[1];
            }

            string baseDir = AppContext.BaseDirectory;
            string telemetryFilePath = Path.IsPathRooted(telemetryFileName)
                ? telemetryFileName
                : Path.Combine(baseDir, telemetryFileName);

            return new AtsConfig(groundHost, groundPort, tailNumber, telemetryFilePath, TimeSpan.FromSeconds(sendIntervalSeconds));
        }
    }
}