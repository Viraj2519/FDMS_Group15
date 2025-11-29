/* 
 *  FILE          : TelemetryDataService.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Viraj Solanki and Darsh Patel
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file defines the TelemetryDataService class which handles
 *    database operations related to telemetry data.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Fdms.Shared;
using Fdms.GroundTerminal.Models;
namespace Fdms.GroundTerminal
{
    //-------------------------------------------------------------
    // CLASS : TelemetryDataService
    // PURPOSE :
    //   Handles database operations related to telemetry data.
    //-------------------------------------------------------------
    public sealed class TelemetryDataService
    {
        private readonly string _connectionString;

        /*---------------------------------------------------------
         *  FUNCTION      : TelemetryDataService
         *  DESCRIPTION   :
         *    Initializes a new instance of the TelemetryDataService class
         *    with the specified database connection string.
         *  PARAMETERS    :
         *      connectionString - The database connection string.
         *  RETURNS       : None
         *---------------------------------------------------------*/
        public TelemetryDataService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string must be provided.", nameof(connectionString));

            _connectionString = connectionString;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : SaveValidPacket
        *  DESCRIPTION   :
        *    Saves a valid telemetry packet to the database.
        *  PARAMETERS    :
        *      packet - The TelemetryPacket instance to save.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void SaveValidPacket(TelemetryPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                UpsertAircraft(conn, tx, packet.TailNumber);

                InsertTelemetryGForce(conn, tx, packet);

                InsertAttitudeParameters(conn, tx, packet);

                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : UpsertAircraft
        *  DESCRIPTION   :
        *    Inserts a new aircraft record if it does not already exist.
        *  PARAMETERS    :
        *      conn - The SQL connection.
        *      tx - The SQL transaction.
        *      tailNumber - The tail number of the aircraft.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private static void UpsertAircraft(SqlConnection conn, SqlTransaction tx, string tailNumber)
        {
            if (string.IsNullOrWhiteSpace(tailNumber))
                throw new ArgumentException("Tail number cannot be empty.", nameof(tailNumber));

            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Aircraft WHERE tail_number = @tail_number)
            BEGIN
            INSERT INTO Aircraft (tail_number, model, airline)
            VALUES (@tail_number, 'Simulated Trainer', 'FDMS Test Fleet');
            END;";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20).Value = tailNumber;
            cmd.ExecuteNonQuery();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : InsertTelemetryGForce
        *  DESCRIPTION   :
        *    Inserts a telemetry g-force record into the database.
        *  PARAMETERS    :
        *      conn - The SQL connection.
        *      tx - The SQL transaction.
        *      packet - The TelemetryPacket instance containing the data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private static void InsertTelemetryGForce(SqlConnection conn, SqlTransaction tx, TelemetryPacket packet)
        {

            var t = packet.Telemetry;

            const string sql = @"
            INSERT INTO Telemetry_GForce
            (
            tail_number,
            packet_sequence,
            timestamp_raw,
            accel_x,
            accel_y,
            accel_z,
            weight,
            altitude,
            stored_at
            )   
            VALUES
            (
            @tail_number,
            @packet_sequence,
            @timestamp_raw,
            @accel_x,
            @accel_y,
            @accel_z,
            @weight,
            @altitude,
            @stored_at
            );";

            using var cmd = new SqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20).Value = packet.TailNumber;
            cmd.Parameters.Add("@packet_sequence", SqlDbType.Int).Value = packet.SequenceNumber;
            cmd.Parameters.Add("@timestamp_raw", SqlDbType.NVarChar, 64)
               .Value = t.TimestampRaw ?? string.Empty;

            cmd.Parameters.Add("@accel_x", SqlDbType.Float).Value = t.AccelX;
            cmd.Parameters.Add("@accel_y", SqlDbType.Float).Value = t.AccelY;
            cmd.Parameters.Add("@accel_z", SqlDbType.Float).Value = t.AccelZ;
            cmd.Parameters.Add("@weight", SqlDbType.Float).Value = t.Weight;
            cmd.Parameters.Add("@altitude", SqlDbType.Float).Value = t.Altitude;

            // REQ-DB-040: local system time at storage
            cmd.Parameters.Add("@stored_at", SqlDbType.DateTime).Value = DateTime.Now;

            cmd.ExecuteNonQuery();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : InsertAttitudeParameters
        *  DESCRIPTION   :
        *    Inserts an attitude parameters record into the database.
        *  PARAMETERS    :
        *      conn - The SQL connection.
        *      tx - The SQL transaction.
        *      packet - The TelemetryPacket instance containing the data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private static void InsertAttitudeParameters(SqlConnection conn, SqlTransaction tx, TelemetryPacket packet)
        {
            var t = packet.Telemetry;

            const string sql = @"
            INSERT INTO Attitude_Parameters
            (
            tail_number,
            packet_sequence,
            pitch,
            bank,
            stored_at
            )
            VALUES
            (
            @tail_number,
            @packet_sequence,
            @pitch,
            @bank,
            @stored_at
            );";

            using var cmd = new SqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20).Value = packet.TailNumber;
            cmd.Parameters.Add("@packet_sequence", SqlDbType.Int).Value = packet.SequenceNumber;
            cmd.Parameters.Add("@pitch", SqlDbType.Float).Value = t.Pitch;
            cmd.Parameters.Add("@bank", SqlDbType.Float).Value = t.Bank;
            cmd.Parameters.Add("@stored_at", SqlDbType.DateTime).Value = DateTime.Now;

            cmd.ExecuteNonQuery();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : SaveInvalidPacket
        *  DESCRIPTION   :
        *    Saves an invalid telemetry packet to the database.
        *  PARAMETERS    :
        *      invalid - The InvalidPacketReceivedEventArgs instance containing the data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void SaveInvalidPacket(InvalidPacketReceivedEventArgs invalid)
        {
            if (invalid == null) throw new ArgumentNullException(nameof(invalid));

            const string sql = @"
INSERT INTO Error_Packets
(
    tail_number,
    packet_sequence,
    raw_packet,
    checksum,
    error_reason,
    stored_at
)
VALUES
(
    @tail_number,
    @packet_sequence,
    @raw_packet,
    @checksum,
    @error_reason,
    @stored_at
);";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);

            if (string.IsNullOrWhiteSpace(invalid.TailNumber))
                cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20).Value = invalid.TailNumber;

            if (invalid.SequenceNumber.HasValue)
                cmd.Parameters.Add("@packet_sequence", SqlDbType.Int).Value = invalid.SequenceNumber.Value;
            else
                cmd.Parameters.Add("@packet_sequence", SqlDbType.Int).Value = DBNull.Value;

            cmd.Parameters.Add("@raw_packet", SqlDbType.VarChar, -1)
               .Value = invalid.RawPacket ?? string.Empty;

            if (invalid.ActualChecksum.HasValue)
                cmd.Parameters.Add("@checksum", SqlDbType.Int).Value = invalid.ActualChecksum.Value;
            else
                cmd.Parameters.Add("@checksum", SqlDbType.Int).Value = DBNull.Value;

            var reason = $"{invalid.ReasonCode}: {invalid.Details}";
            cmd.Parameters.Add("@error_reason", SqlDbType.VarChar, 200)
               .Value = (object)reason ?? DBNull.Value;

            cmd.Parameters.Add("@stored_at", SqlDbType.DateTime).Value = DateTime.Now;

            cmd.ExecuteNonQuery();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : SearchTelemetry
        *  DESCRIPTION   :
        *    Searches telemetry data based on the provided criteria.
        *  PARAMETERS    :
        *      criteria - The TelemetrySearchCriteria instance containing search parameters.
        *  RETURNS       : A list of TelemetrySearchResult instances matching the criteria.
        *---------------------------------------------------------*/
        public IList<TelemetrySearchResult> SearchTelemetry(TelemetrySearchCriteria criteria)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            var results = new List<TelemetrySearchResult>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;

            var sql = @"
SELECT
    g.tail_number,
    g.packet_sequence,
    g.timestamp_raw,
    g.altitude,
    a.pitch,
    a.bank,
    g.weight,
    g.accel_x,
    g.accel_y,
    g.accel_z
FROM Telemetry_GForce g
INNER JOIN Attitude_Parameters a
    ON g.tail_number = a.tail_number
   AND g.packet_sequence = a.packet_sequence
WHERE 1 = 1";

            if (!string.IsNullOrWhiteSpace(criteria.TailNumber))
            {
                sql += " AND g.tail_number = @tail_number";
                cmd.Parameters.Add("@tail_number", SqlDbType.VarChar, 20)
                   .Value = criteria.TailNumber.Trim();
            }


            sql += " ORDER BY g.stored_at ASC;";

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var r = new TelemetrySearchResult
                {
                    TailNumber = reader.GetString(reader.GetOrdinal("tail_number")),
                    PacketSequence = reader.GetInt32(reader.GetOrdinal("packet_sequence")),
                    TimestampRaw = reader.GetString(reader.GetOrdinal("timestamp_raw")),
                    Altitude = reader.GetDouble(reader.GetOrdinal("altitude")),
                    Pitch = reader.GetDouble(reader.GetOrdinal("pitch")),
                    Bank = reader.GetDouble(reader.GetOrdinal("bank")),
                    Weight = reader.GetDouble(reader.GetOrdinal("weight")),
                    AccelX = reader.GetDouble(reader.GetOrdinal("accel_x")),
                    AccelY = reader.GetDouble(reader.GetOrdinal("accel_y")),
                    AccelZ = reader.GetDouble(reader.GetOrdinal("accel_z"))
                };

                results.Add(r);
            }

            return results;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ExportResultsToAscii
        *  DESCRIPTION   :
        *    Exports telemetry search results to an ASCII file.
        *  PARAMETERS    :
        *      results - The collection of TelemetrySearchResult instances to export.
        *      filePath - The file path to save the ASCII file.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void ExportResultsToAscii(IEnumerable<TelemetrySearchResult> results, string filePath)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));

            using var writer = new StreamWriter(filePath);

            writer.WriteLine(
                "TailNumber,Sequence,TimestampRaw,Altitude,Pitch,Bank,Weight,AccelX,AccelY,AccelZ");

            foreach (var r in results)
            {
                var line = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3:F6},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6}",
                    r.TailNumber,
                    r.PacketSequence,
                    r.TimestampRaw,
                    r.Altitude,
                    r.Pitch,
                    r.Bank,
                    r.Weight,
                    r.AccelX,
                    r.AccelY,
                    r.AccelZ);

                writer.WriteLine(line);
            }
        }
    }
}