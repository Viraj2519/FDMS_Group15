using System;

namespace Fdms.Shared
{

    public sealed class TelemetryPacket
    {

        public string TailNumber { get; }

        public int SequenceNumber { get; }

        public TelemetryRecord Telemetry { get; }

        public int Checksum { get; }

        public TelemetryPacket(string tailNumber, int sequenceNumber, TelemetryRecord telemetry, int checksum)
        {
            if (string.IsNullOrWhiteSpace(tailNumber))
            {
                throw new ArgumentException("Tail number cannot be null or empty.", nameof(tailNumber));
            }

            if (sequenceNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequenceNumber),
                    "Sequence number must be non-negative.");
            }

            Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

            TailNumber = tailNumber.Trim();
            SequenceNumber = sequenceNumber;
            Checksum = checksum;
        }

        public override string ToString()
        {
            return $"{TailNumber} #{SequenceNumber} | Alt={Telemetry.Altitude}, Pitch={Telemetry.Pitch}, Bank={Telemetry.Bank}, Checksum={Checksum}";
        }
    }
}
