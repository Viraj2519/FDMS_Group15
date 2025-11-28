using System;

namespace Fdms.Ats
{
    internal static class Program
    {

        private static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("FDMS - Aircraft Transmission System (ATS) - Group 15");
                Console.WriteLine("---------------------------------------------------");

                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: Fdms.Ats.exe <TailNumber> [TelemetryFilePath]");
                    Console.WriteLine();
                    Console.WriteLine("Examples:");
                    Console.WriteLine("  Fdms.Ats.exe C-FGAX");
                    Console.WriteLine("  Fdms.Ats.exe C-GEFC");
                    Console.WriteLine("  Fdms.Ats.exe C-QWWT");
                    Console.WriteLine();
                    Console.WriteLine("[ATS] No tail number supplied on the command line.");
                    Console.WriteLine("[ATS] Defaulting to the configuration in AtsConfig (e.g., C-FGAX / C-FGAX.txt).");
                    Console.WriteLine();
                }

                var config = AtsConfig.FromArgsOrDefaults(args);

                Console.WriteLine($"[ATS] Ground Terminal: {config.GroundHost}:{config.GroundPort}");
                Console.WriteLine($"[ATS] Tail Number:     {config.TailNumber}");
                Console.WriteLine($"[ATS] Telemetry File:  {config.TelemetryFilePath}");
                Console.WriteLine();

                using var sender = new AtsSender(config);
                sender.Run();

                Console.WriteLine("[ATS] Press any key to exit...");
                Console.ReadKey();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ATS] Fatal error: " + ex.Message);
                Console.ResetColor();
                return 1;
            }
        }
    }
}