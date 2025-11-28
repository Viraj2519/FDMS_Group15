/* 
 *  FILE          : Program.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya; Darsh Patel; Jal Shah ; Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file contains the main entry point for the FDMS Ground Terminal application.
 */
using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Fdms.GroundTerminal
{
    //-------------------------------------------------------------
    // CLASS : Program
    // PURPOSE :
    //   Contains the entry point for the FDMS Ground Terminal application.
    //-------------------------------------------------------------
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GroundConfig config;
            try
            {
                config = GroundConfig.FromArgsOrDefaults(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to create Ground Terminal configuration:\n\n" + ex.Message,
                    "FDMS Ground Terminal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            using var controller = new GroundTerminalController(config);
            Application.Run(new MainForm(controller));
        }
    }
}
