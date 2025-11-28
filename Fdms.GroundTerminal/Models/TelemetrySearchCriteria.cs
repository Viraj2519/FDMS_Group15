/* 
 *  FILE          : TelemetrySearchCriteria.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Viraj Solanki and Darsh Patel
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the TelemetrySearchCriteria class which encapsulates
    *    the search criteria for querying telemetry data.
 */
using System;
using System.Data.SqlClient;


namespace Fdms.GroundTerminal.Models
{
    //-------------------------------------------------------------
    // CLASS : TelemetrySearchCriteria
    // PURPOSE :
    //   Encapsulates the search criteria for querying telemetry data.
    //-------------------------------------------------------------
    public sealed class TelemetrySearchCriteria
    {
        public string? TailNumber { get; set; }
        public DateTime? FromTimestamp { get; set; }
        public DateTime? ToTimestamp { get; set; }
    }
}
