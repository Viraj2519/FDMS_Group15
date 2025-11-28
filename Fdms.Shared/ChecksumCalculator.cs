using System;

namespace Fdms.Shared
{

    public static class ChecksumCalculator
    {

        public static int CalculateChecksum(TelemetryRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return CalculateChecksum(record.Altitude, record.Pitch, record.Bank);
        }


        public static int CalculateChecksum(double altitude, double pitch, double bank)
        {
            double average = (altitude + pitch + bank) / 3.0;

            int checksum = (int)Math.Round(average, MidpointRounding.AwayFromZero);

            return checksum;
        }
    }
}
