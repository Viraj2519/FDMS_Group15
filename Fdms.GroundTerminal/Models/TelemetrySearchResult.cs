/* 
 *  FILE          : TelemetrySearchResult.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Viraj Solanki and Darsh Patel
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the TelemetrySearchResult class which encapsulates
    *    the search result for querying telemetry data.
 */
using System;
using System.Data.SqlClient;

namespace Fdms.GroundTerminal.Models
{
    //-------------------------------------------------------------
    // CLASS : TelemetrySearchResult
    // PURPOSE :
    //   Encapsulates the search result for querying telemetry data.
    //-------------------------------------------------------------
    public sealed class TelemetrySearchResult
    {
        public int TelemetryId { get; set; }
        public string TailNumber { get; set; } = string.Empty;
        public int PacketSequence { get; set; }
        public string TimestampRaw { get; set; } = string.Empty;

        public double Altitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Weight { get; set; }

        public double AccelX { get; set; }
        public double AccelY { get; set; }
        public double AccelZ { get; set; }

        public DateTime StoredAt { get; set; }
    }
}
