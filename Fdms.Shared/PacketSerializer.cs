using System;
using System.Globalization;

namespace Fdms.Shared
{

    public static class PacketSerializer
    {
        private const char FieldSeparator = '|';

        public static string Serialize(TelemetryPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            string body = packet.Telemetry.ToCsvString();

            return string.Join(FieldSeparator,
                packet.TailNumber,
                packet.SequenceNumber.ToString(CultureInfo.InvariantCulture),
                body,
                packet.Checksum.ToString(CultureInfo.InvariantCulture));
        }


        public static bool TryDeserialize(
            string rawPacket,
            out TelemetryPacket packet,
            out string errorReason)
        {
            packet = null;
            errorReason = string.Empty;

            if (string.IsNullOrWhiteSpace(rawPacket))
            {
                errorReason = "Packet string is empty.";
                return false;
            }

            string[] parts = rawPacket.Split(FieldSeparator);
            if (parts.Length != 4)
            {
                errorReason = $"Expected 4 '|' separated fields (Tail|Seq|Body|Checksum), got {parts.Length}.";
                return false;
            }

            string tailNumber = parts[0].Trim();
            string sequenceText = parts[1].Trim();
            string bodyCsv = parts[2];
            string checksumText = parts[3].Trim();

            if (string.IsNullOrWhiteSpace(tailNumber))
            {
                errorReason = "Tail number is missing or empty.";
                return false;
            }

            if (!int.TryParse(sequenceText, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int sequenceNumber) ||
                sequenceNumber < 0)
            {
                errorReason = $"Invalid sequence number: \"{sequenceText}\".";
                return false;
            }

            if (!TelemetryRecord.TryParseCsv(bodyCsv, out TelemetryRecord telemetry, out string telemetryError))
            {
                errorReason = $"Failed to parse telemetry body: {telemetryError}";
                return false;
            }

            if (!int.TryParse(checksumText, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int checksum))
            {
                errorReason = $"Invalid checksum value: \"{checksumText}\".";
                return false;
            }

            packet = new TelemetryPacket(tailNumber, sequenceNumber, telemetry, checksum);
            return true;
        }
    }
}
