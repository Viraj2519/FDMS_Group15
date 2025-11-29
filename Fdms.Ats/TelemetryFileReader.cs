/* 
 *  FILE          : TelemetryFileReader.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Viraj Solanki, jal shah , darsh patel
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the TelemetryFileReader class and encapsulates all 
    *    variables, constants, and logic for handling telemetry data file reading.
 */
using System;
using System.Collections.Generic;
using System.IO;
using Fdms.Shared;
using System.Data.SqlClient;

namespace Fdms.Ats
{
    //-------------------------------------------------------------
    // CLASS : TelemetryFileReader
    // PURPOSE :
    //   Provides functionality to read telemetry records from a file.
    //-------------------------------------------------------------
    public sealed class TelemetryFileReader
    {
        private readonly string _filePath;

        /*---------------------------------------------------------
         *  FUNCTION      : TelemetryFileReader
         *  DESCRIPTION   :
         *    initialize a TelemetryFileReader instance with
         *    provided file path.
         *  PARAMETERS    : 
         *      filePath - The path to the telemetry data file.
         *  RETURNS       : None
         *---------------------------------------------------------*/
        public TelemetryFileReader(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /*---------------------------------------------------------
         *  FUNCTION      : ReadTelemetryRecords
         *  DESCRIPTION   :
         *    Reads telemetry records from the specified file.
         *  PARAMETERS    : None
         *  RETURNS       : An enumerable of TelemetryRecord instances.
         *---------------------------------------------------------*/
        public IEnumerable<TelemetryRecord> ReadTelemetryRecords()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("Telemetry file not found.", _filePath);
            }

            using var reader = new StreamReader(_filePath);

            string? line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!TelemetryRecord.TryParseCsv(line, out var record, out string error))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[ATS] Skipping line {lineNumber}: {error}");
                    Console.ResetColor();
                    continue;
                }

                yield return record;
            }
        }
    }
}
