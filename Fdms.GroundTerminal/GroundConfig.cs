/* 
 *  FILE          : GroundConfig.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya; Darsh Patel; Jal Shah; Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file defines the GroundConfig class which encapsulates
 *    the configuration settings for the Ground Terminal application.
 */

using System;
using System.Data.SqlClient;

namespace Fdms.GroundTerminal
{
    //-------------------------------------------------------------
    // CLASS : GroundConfig
    // PURPOSE :
    //   Encapsulates the configuration settings for the Ground Terminal application.
    //-------------------------------------------------------------
    public sealed class GroundConfig
    {

        public int ListenPort { get; }


        public string ConnectionString { get; }

        /*---------------------------------------------------------
        *  FUNCTION      : GroundConfig
        *  DESCRIPTION   :
        *    initialize a GroundConfig instance with
        *    provided configuration values.
        *  PARAMETERS    :
        *      listenPort - The port number the ground terminal listens on.
        *      connectionString - The SQL Server connection string.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private GroundConfig(int listenPort, string connectionString)
        {
            if (listenPort < 1 || listenPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(listenPort), "Listen port must be between 1 and 65535.");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A valid SQL Server connection string must be provided.", nameof(connectionString));

            ListenPort = listenPort;
            ConnectionString = connectionString;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : FromArgsOrDefaults
        *  DESCRIPTION   :
        *    Creates a GroundConfig instance from command-line arguments
        *    or default values if arguments are not provided.
        *  PARAMETERS    :
        *      args - Command-line arguments.
        *  RETURNS       : A GroundConfig instance.
        *---------------------------------------------------------*/
        public static GroundConfig FromArgsOrDefaults(string[] args)
        {

            int listenPort = 5000;

            if (args != null && args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
            {
                if (!int.TryParse(args[0], out listenPort) || listenPort < 1 || listenPort > 65535)
                {
                    throw new ArgumentException(
                        $"Invalid listen port '{args[0]}'. Please provide an integer between 1 and 65535.");
                }
            }

            string connectionString;

            if (args != null && args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
            {

                connectionString = args[1];
            }
            else
            {
                connectionString =
                    "Server=AYUSH;Database=FDMS;Trusted_Connection=True;TrustServerCertificate=True;";
            }

            return new GroundConfig(listenPort, connectionString);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ToString
        *  DESCRIPTION   :
        *    Provides a string representation of the GroundConfig instance.
        *  PARAMETERS    : None
        *  RETURNS       : A string representing the GroundConfig instance.
        *---------------------------------------------------------*/
        public override string ToString()
        {
            return $"ListenPort={ListenPort}, ConnectionString={ConnectionString}";
        }
    }
}