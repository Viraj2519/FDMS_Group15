/* 
 *  FILE          : MainForm.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya; Darsh Patel; jal Shah; Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file defines the MainForm class which serves as the main GUI
 *    for the FDMS Ground Terminal application.
 */
using Fdms.GroundTerminal.Models;
using Fdms.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fdms.GroundTerminal
{
    //-------------------------------------------------------------
    // CLASS : MainForm
    // PURPOSE :
    //   Serves as the main GUI for the FDMS Ground Terminal application.
    //-------------------------------------------------------------
    public sealed class MainForm : Form
    {
        private readonly GroundTerminalController _controller;

        private Label _lblMode;

        private Label _lblStatus;

        private TabControl _tabControl;
        private TabPage _tabRealTime;
        private TabPage _tabSearch;

        private DataGridView _dgvRealTime;
        private BindingList<RealTimeRow> _realTimeRows;

        private TextBox _txtTailFilter;
        private CheckBox _chkFrom;
        private DateTimePicker _dtFrom;
        private CheckBox _chkTo;
        private DateTimePicker _dtTo;
        private Button _btnSearch;
        private Button _btnClear;
        private Button _btnExport;
        private DataGridView _dgvSearchResults;
        private BindingList<TelemetrySearchResult> _searchResults;

        /*---------------------------------------------------------
        *  FUNCTION      : MainForm
        *  DESCRIPTION   :
        *    Initializes a new instance of the MainForm class with
        *    the specified controller.
        *  PARAMETERS    :
        *      controller - The GroundTerminalController instance.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public MainForm(GroundTerminalController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            Text = "FDMS Ground Terminal - Group 15";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 650);

            InitializeComponents();

            _controller.RealTimeTelemetryReceived += ControllerOnRealTimeTelemetryReceived;
            _controller.InvalidPacketCountChanged += ControllerOnInvalidPacketCountChanged;

            UpdateStatusLabel();

            _controller.Start();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : InitializeComponents
        *  DESCRIPTION   :
        *    Initializes and configures all GUI components.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void InitializeComponents()
        {
            _lblMode = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };

            _lblStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Padding = new Padding(8, 0, 0, 0)
            };

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            _tabControl.SelectedIndexChanged += (_, __) => UpdateModeLabel();

            _tabRealTime = new TabPage("Real-Time");
            _tabSearch = new TabPage("Search");

            _tabControl.TabPages.Add(_tabRealTime);
            _tabControl.TabPages.Add(_tabSearch);

            InitializeRealTimeTab();
            InitializeSearchTab();

            Controls.Add(_tabControl);
            Controls.Add(_lblStatus);
            Controls.Add(_lblMode);

            UpdateModeLabel();

            FormClosing += MainForm_FormClosing;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : InitializeRealTimeTab
        *  DESCRIPTION   :
        *    Initializes and configures the Real-Time tab components.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void InitializeRealTimeTab()
        {
            _realTimeRows = new BindingList<RealTimeRow>();

            _dgvRealTime = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                DataSource = _realTimeRows
            };

            _tabRealTime.Controls.Add(_dgvRealTime);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : InitializeSearchTab
        *  DESCRIPTION   :
        *    Initializes and configures the Search tab components.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void InitializeSearchTab()
        {
            _searchResults = new BindingList<TelemetrySearchResult>();

            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90
            };

            int marginLeft = 10;
            int marginTop = 10;
            int spacingX = 10;

            var lblTail = new Label
            {
                Text = "Tail Number:",
                Left = marginLeft,
                Top = marginTop + 8,
                AutoSize = true
            };

            _txtTailFilter = new TextBox
            {
                Left = lblTail.Right + spacingX,
                Top = marginTop,
                Width = 100
            };

            _chkFrom = new CheckBox
            {
                Text = "From:",
                Left = _txtTailFilter.Right + spacingX,
                Top = marginTop + 8,
                AutoSize = true
            };

            _dtFrom = new DateTimePicker
            {
                Left = _chkFrom.Right + spacingX,
                Top = marginTop,
                Width = 160,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss"
            };

            _chkTo = new CheckBox
            {
                Text = "To:",
                Left = _dtFrom.Right + spacingX,
                Top = marginTop + 8,
                AutoSize = true
            };

            _dtTo = new DateTimePicker
            {
                Left = _chkTo.Right + spacingX,
                Top = marginTop,
                Width = 160,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss"
            };

            _btnSearch = new Button
            {
                Text = "Search",
                Left = marginLeft,
                Top = marginTop + 35,
                Width = 100
            };
            _btnSearch.Click += BtnSearch_Click;

            _btnClear = new Button
            {
                Text = "Clear",
                Left = _btnSearch.Right + spacingX,
                Top = marginTop + 35,
                Width = 100
            };
            _btnClear.Click += BtnClear_Click;

            _btnExport = new Button
            {
                Text = "Export...",
                Left = _btnClear.Right + spacingX,
                Top = marginTop + 35,
                Width = 100
            };
            _btnExport.Click += BtnExport_Click;

            panelTop.Controls.Add(lblTail);
            panelTop.Controls.Add(_txtTailFilter);
            panelTop.Controls.Add(_chkFrom);
            panelTop.Controls.Add(_dtFrom);
            panelTop.Controls.Add(_chkTo);
            panelTop.Controls.Add(_dtTo);
            panelTop.Controls.Add(_btnSearch);
            panelTop.Controls.Add(_btnClear);
            panelTop.Controls.Add(_btnExport);

            _dgvSearchResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                DataSource = _searchResults
            };

            _tabSearch.Controls.Add(_dgvSearchResults);
            _tabSearch.Controls.Add(panelTop);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : UpdateModeLabel
        *  DESCRIPTION   :
        *    Updates the mode label based on the selected tab.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void UpdateModeLabel()
        {
            if (_tabControl.SelectedTab == _tabRealTime)
            {
                _lblMode.Text = "Mode: REAL-TIME (live telemetry display)";
                _lblMode.ForeColor = Color.DarkGreen;
            }
            else
            {
                _lblMode.Text = "Mode: SEARCH RESULTS (database query display)";
                _lblMode.ForeColor = Color.DarkBlue;
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : UpdateStatusLabel
        *  DESCRIPTION   :
        *    Updates the status label with current port, database name, and invalid packet count.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void UpdateStatusLabel()
        {
            int port = _controller.ListenPort;
            string dbName = _controller.DatabaseName;
            int invalid = _controller.InvalidPacketCount;

            _lblStatus.Text = $"Port: {port} | Database: {dbName} | Invalid packets: {invalid}";
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ControllerOnInvalidPacketCountChanged
        *  DESCRIPTION   :
        *    Handles the event when the invalid packet count changes.
        *  PARAMETERS    :
        *    sender - The source of the event.
        *    e - Event arguments.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void ControllerOnInvalidPacketCountChanged(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ControllerOnInvalidPacketCountChanged(sender, e)));
                return;
            }

            UpdateStatusLabel();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ControllerOnRealTimeTelemetryReceived
        *  DESCRIPTION   :
        *    Handles the event when real-time telemetry is received.
        *  PARAMETERS    :
        *    sender - The source of the event.
        *    e - Real-time telemetry event arguments.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void ControllerOnRealTimeTelemetryReceived(object? sender, RealTimeTelemetryEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ControllerOnRealTimeTelemetryReceived(sender, e)));
                return;
            }

            var row = new RealTimeRow
            {
                TailNumber = e.TailNumber,
                SequenceNumber = e.SequenceNumber,
                TimestampRaw = e.TimestampRaw,
                Altitude = e.Altitude,
                Pitch = e.Pitch,
                Bank = e.Bank,
                Weight = e.Weight,
                AccelX = e.AccelX,
                AccelY = e.AccelY,
                AccelZ = e.AccelZ,
                Checksum = e.Checksum
            };

            _realTimeRows.Insert(0, row);

            const int maxRows = 500;
            while (_realTimeRows.Count > maxRows)
            {
                _realTimeRows.RemoveAt(_realTimeRows.Count - 1);
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : BtnSearch_Click
        *  DESCRIPTION   :
        *    Handles the Search button click event to perform telemetry search.
        *  PARAMETERS    :
        *      sender - The source of the event.
        *      e - Event arguments.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                var criteria = new TelemetrySearchCriteria
                {
                    TailNumber = string.IsNullOrWhiteSpace(_txtTailFilter.Text)
                        ? null
                        : _txtTailFilter.Text.Trim(),
                    FromTimestamp = _chkFrom.Checked ? _dtFrom.Value : (DateTime?)null,
                    ToTimestamp = _chkTo.Checked ? _dtTo.Value : (DateTime?)null
                };

                var results = _controller.SearchTelemetry(criteria);

                _searchResults.Clear();
                foreach (var r in results)
                {
                    _searchResults.Add(r);
                }

                _tabControl.SelectedTab = _tabSearch;
                UpdateModeLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while searching telemetry:\n\n" + ex.Message,
                    "FDMS Ground Terminal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : BtnClear_Click
        *  DESCRIPTION   :
        *    Handles the Clear button click event to reset search criteria and results.
        *  PARAMETERS    :
        *      sender - The source of the event.
        *      e - Event arguments.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _txtTailFilter.Clear();
            _chkFrom.Checked = false;
            _chkTo.Checked = false;

            _searchResults.Clear();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_searchResults.Count == 0)
                {
                    MessageBox.Show(
                        "There are no search results to export.",
                        "FDMS Ground Terminal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using var dialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = "fdms_search_results.txt"
                };

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var criteria = new TelemetrySearchCriteria
                    {
                        TailNumber = string.IsNullOrWhiteSpace(_txtTailFilter.Text)
                            ? null
                            : _txtTailFilter.Text.Trim(),
                        FromTimestamp = _chkFrom.Checked ? _dtFrom.Value : (DateTime?)null,
                        ToTimestamp = _chkTo.Checked ? _dtTo.Value : (DateTime?)null
                    };

                    _controller.ExportSearchResultsToAscii(criteria, dialog.FileName);

                    MessageBox.Show(
                        "Search results exported successfully.",
                        "FDMS Ground Terminal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while exporting results:\n\n" + ex.Message,
                    "FDMS Ground Terminal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : MainForm_FormClosing
        *  DESCRIPTION   :
        *    Handles the FormClosing event to stop the controller.
        *  PARAMETERS    :
        *      sender - The source of the event.
        *      e - Event arguments.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _controller.Stop();
        }


        //-------------------------------------------------------------
        // CLASS : RealTimeRow
        // PURPOSE :
        //   Represents a row in the real-time telemetry data grid.
        //-------------------------------------------------------------
        private sealed class RealTimeRow
        {
            public string TailNumber { get; set; } = string.Empty;
            public int SequenceNumber { get; set; }
            public string TimestampRaw { get; set; } = string.Empty;

            public double Altitude { get; set; }
            public double Pitch { get; set; }
            public double Bank { get; set; }

            public double Weight { get; set; }
            public double AccelX { get; set; }
            public double AccelY { get; set; }
            public double AccelZ { get; set; }

            public int Checksum { get; set; }
        }
    }
}