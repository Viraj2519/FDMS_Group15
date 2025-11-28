using System;
using System.Globalization;

namespace Fdms.Shared
{

    public sealed class TelemetryRecord
    {

        public string TimestampRaw { get; }

        public double AccelX { get; }
        public double AccelY { get; }
        public double AccelZ { get; }

        public double Weight { get; }
        public double Altitude { get; }

        public double Pitch { get; }
        public double Bank { get; }

        public TelemetryRecord(
            string timestampRaw,
            double accelX,
            double accelY,
            double accelZ,
            double weight,
            double altitude,
            double pitch,
            double bank)
        {
            if (string.IsNullOrWhiteSpace(timestampRaw))
            {
                throw new ArgumentException("Timestamp cannot be null or empty.", nameof(timestampRaw));
            }

            TimestampRaw = timestampRaw.Trim();
            AccelX = accelX;
            AccelY = accelY;
            AccelZ = accelZ;
            Weight = weight;
            Altitude = altitude;
            Pitch = pitch;
            Bank = bank;
        }


        public static bool TryParseCsv(string csvLine, out TelemetryRecord record, out string errorReason)
        {
            record = null;
            errorReason = string.Empty;

            if (string.IsNullOrWhiteSpace(csvLine))
            {
                errorReason = "Telemetry line is empty.";
                return false;
            }

            string[] parts = csvLine.Split(',');
            if (parts.Length < 8)
            {
                errorReason = $"Expected at least 8 comma-separated values, got {parts.Length}.";
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            string timestampRaw = parts[0];

            if (!TryParseDouble(parts[1], out double accelX, out errorReason) ||
                !TryParseDouble(parts[2], out double accelY, out errorReason) ||
                !TryParseDouble(parts[3], out double accelZ, out errorReason) ||
                !TryParseDouble(parts[4], out double weight, out errorReason) ||
                !TryParseDouble(parts[5], out double altitude, out errorReason) ||
                !TryParseDouble(parts[6], out double pitch, out errorReason) ||
                !TryParseDouble(parts[7], out double bank, out errorReason))
            {
                return false;
            }

            record = new TelemetryRecord(
                timestampRaw,
                accelX,
                accelY,
                accelZ,
                weight,
                altitude,
                pitch,
                bank);

            return true;
        }

        public string ToCsvString()
        {

            var culture = CultureInfo.InvariantCulture;

            return string.Join(",",
                TimestampRaw,
                AccelX.ToString(culture),
                AccelY.ToString(culture),
                AccelZ.ToString(culture),
                Weight.ToString(culture),
                Altitude.ToString(culture),
                Pitch.ToString(culture),
                Bank.ToString(culture));
        }

        private static bool TryParseDouble(string text, out double value, out string errorReason)
        {
            errorReason = string.Empty;
            if (!double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out value))
            {
                errorReason = $"Failed to parse \"{text}\" as a floating-point value.";
                return false;
            }

            return true;
        }
    }
}
