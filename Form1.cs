using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace AguilaRemoteControl
{
    public partial class AguilaRemoteControl : Form
    {
        private Dictionary<int, string> Site = new Dictionary<int, string>
        {
            {1, "A101"},    {7, "B101"},    {13, "C101"},    {19, "D101"},    {25, "E101"},    {31, "F101"},
            {2, "A201"},    {8, "B201"},    {14, "C201"},    {20, "D201"},    {26, "E201"},    {32, "F201"},
            {3, "A301"},    {9, "B301"},    {15, "C301"},    {21, "D301"},    {27, "E301"},    {33, "F301"},
            {4, "A401"},    {10, "B401"},    {16, "C401"},    {22, "D401"},    {28, "E401"},    {34, "F401"},
            {5, "A501"},    {11, "B501"},    {17, "C501"},    {23, "D501"},    {29, "E501"},    {35, "F501"},
            {6, "A601"},    {12, "B601"},    {18, "C601"},    {24, "D601"},    {30, "E601"},    {36, "F601"}
        };

        public AguilaRemoteControl()
        {
            InitializeComponent();

            WriteConsole($"========== Aguila Remote Control Version 1.0 ==========" + Environment.NewLine);
            //LogToLogfileAsync("Info", "========== Aguila Remote Control Version 1.0 ==========");

        }
        //public async Task RunPowerShellScript(string scriptPath, List<string> list, string source, string destination)
        //{
        //    // Join the list into a comma-separated string
        //    string listAsString = string.Join(",", list);

        //    using (Runspace runspace = RunspaceFactory.CreateRunspace())
        //    {
        //        runspace.Open();

        //        // Create a PowerShell instance
        //        using (Pipeline ps = runspace.CreatePipeline())
        //        {
        //            Command scriptCommand = new Command(scriptPath);
        //            scriptCommand.Parameters.Add("source", source);
        //            scriptCommand.Parameters.Add("destination", destination);
        //            scriptCommand.Parameters.Add("list", listAsString);
        //            ps.Commands.Add(scriptCommand);

        //            // Execute the script and handle the results
        //            try
        //            {
        //                // Run the PowerShell script asynchronously
        //                Collection<PSObject> results = await Task.Run(() => ps.Invoke());

        //                // Check for errors
        //                if (ps.Error.Count > 0)
        //                {
        //                    while (!ps.Error.EndOfPipeline)
        //                    {
        //                        PSObject error = ps.Error.Read() as PSObject;
        //                        if (error != null)
        //                        {
        //                            // Log the error
        //                            WriteConsole($"Error: {error}", Color.Red);
        //                        }
        //                    }
        //                    // 

        //                }
        //                else
        //                {
        //                    foreach (PSObject result in results)
        //                    {
        //                        // Process the result object
        //                        WriteConsole(result.ToString(), Color.Black);
        //                    }
        //                }
        //            }

        //            catch (Exception ex)
        //            {
        //                WriteConsole($"General Exception: {ex.Message}", Color.Red);
        //            }

        //        }

        //        runspace.Close();
        //    }


        //}

        // Function PingHost - ping cell online




        // Function WriteConsole - Log messages to a RichTextBox with color
        //private async Task<(List<string> cellIPOnline, List<string> sstCellOnline)> CheckCellsOnlineAsync()
        //{
        //    List<int> selectedCells = new List<int>();
        //    for (int i = 1; i <= 36; i++)
        //    {
        //        CheckBox cb = this.Controls.Find("cb_" + i, true)[0] as CheckBox;
        //        if (cb != null && cb.Checked)
        //        {
        //            selectedCells.Add(i);
        //        }

        //    }

        //    if (selectedCells.Count == 0)
        //    {
        //        MessageBox.Show("Please select cell", @"", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return (new List<string>(), new List<string>());
        //    }

        //    List<string> cellIPOnline = new List<string>();
        //    List<string> sstCellOnline = new List<string>();

        //    foreach (int cellNumber in selectedCells)
        //    {
        //        string cellIp = "10.250.0." + cellNumber;
        //        string sstCell = Site[cellNumber];
        //        bool isOnline = false;
        //        using (Ping pinger = new Ping())
        //        {
        //            try
        //            {
        //                PingReply reply = await pinger.SendPingAsync(cellIp);
        //                isOnline = reply.Status == IPStatus.Success;
        //            }
        //            catch (PingException)
        //            {
        //                // Handle any exceptions here
        //                isOnline = false;
        //            }
        //        }

        //        if (isOnline)
        //        {
        //            cellIPOnline.Add(cellIp);
        //            sstCellOnline.Add(sstCell);
        //        }

        //        // Construct the log message based on the ping result
        //        string iplog = $"Cell {sstCell} is {(isOnline ? "ONLINE" : "OFFLINE")}";
        //        Color logColor = isOnline ? Color.DarkGreen : Color.Red;

        //        WriteConsole(iplog);
        //        //await LogToLogfileAsync(sstCell, iplog);


        //    }

        //    return (cellIPOnline, sstCellOnline);
        //}

        private void AppendLineToConsole(string text, Color textColor, Color backColor)
        {
            //Set the color for the appended text
            rtb_result.SelectionStart = rtb_result.TextLength;
            rtb_result.SelectionLength = 0;
            rtb_result.SelectionColor = textColor;
            rtb_result.SelectionBackColor = backColor;
            // Append the log message to the RichTextBox
            rtb_result.AppendText(text);
            // Deselect the text
            rtb_result.SelectionColor = rtb_result.ForeColor;
            rtb_result.SelectionBackColor = rtb_result.BackColor;
            // Optionally, scroll to the bottom of the RichTextBox
            rtb_result.ScrollToCaret();
        }

        void WriteLine(string message_child)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string messageToWrite = timestamp + ": " + message_child + "\n";
            // Set color base on string

            // Add items to feature combo box
            string colorConfigPath = "WriteLineColor.config";
            // Load the XML document
            XDocument xmlDoc = XDocument.Load(colorConfigPath);
            // Query the document to retrieve the feature elements
            var features = xmlDoc.Descendants("words")
                                 .Select(f => new
                                 {
                                     word = f.Attribute("word").Value,
                                     color = f.Attribute("color").Value,
                                     backColor = f.Attribute("backColor").Value
                                 })
                                 .ToList();
            // Convert the results to arrays
            string[] words, colors, backColors;
            words = features.Select(f => f.word).ToArray();
            colors = features.Select(f => f.color).ToArray();
            backColors = features.Select(f => f.backColor).ToArray();

            // Assign color
            string message_child_lower = message_child.ToLower();
            Color color = Color.DarkGray;
            Color backColor = rtb_result.BackColor;
            foreach (var word in words)
            {
                if (message_child_lower.Contains(word))
                {
                    color = Color.colors[Array.IndexOf(words, word)];
                }
            }

            //Color color = Color.DarkGray;
            //Color backColor = rtb_result.BackColor;
            //string message_child_lower = message_child.ToLower();
            //if (message_child_lower.Contains("done") ||
            //    message_child_lower.Contains("success") ||
            //    message_child_lower.Contains("complete"))
            //{
            //    color = Color.White;
            //    backColor = Color.DarkGreen;
            //}
            //if (message_child_lower.Contains("fail") ||
            //    message_child_lower.Contains("exception") ||
            //    message_child_lower.Contains("error"))
            //{
            //    color = Color.White;
            //    backColor = Color.Red;
            //}

            // Check if an invoke is required for the control
            if (rtb_result.InvokeRequired)
            {
                // Use Invoke to handle the update on the UI thread
                rtb_result.Invoke(new Action(() =>
                {
                    AppendLineToConsole(messageToWrite, color, backColor);
                }));
            }
            else
            {
                AppendLineToConsole(messageToWrite, color, backColor);
            }

            // Log to file
            Task.Run(() => LogToFile(messageToWrite.Replace("\n", "")));
        }

        private void WriteConsole(string message)
        {
            
            //string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //// Check if the method is running on the UI thread
            //if (rtb_result.InvokeRequired)
            //{
            //    // If not, use Invoke to handle the cross-thread operation
            //    rtb_result.Invoke(new Action<string, Color>(WriteConsole), $"{timestamp} {message}", color);
            //}
            //else
            //{
            //    // Set the color for the appended text
            //    rtb_result.SelectionColor = color;
            //    // Append the log message to the RichTextBox with a newline
            //    rtb_result.AppendText(timestamp + " " + message + Environment.NewLine);
            //    // Reset the color back to the default
            //    rtb_result.SelectionColor = rtb_result.ForeColor;
            //    // Optionally, scroll to the bottom of the RichTextBox
            //    rtb_result.ScrollToCaret();
            //}
            // Check if the message contains multiple lines
            if (message.Contains("\n"))
            {
                
                // Split the message into lines
                string[] lines = message.Split(new[] { "\r\n" }, StringSplitOptions.None);

                // Prepend the timestamp to each line and write to the console
                foreach (string line in lines)
                {
                    // If the line is not empty, prepend the timestamp
                    if (!string.IsNullOrWhiteSpace(line) && line.Length > 0)
                    {
                        WriteLine(line);
                    }
                }
            }
            else
            {
                // If the message is a single line, prepend the timestamp and write to the console
                if (!string.IsNullOrWhiteSpace(message) && message.Length > 0)
                {
                    WriteLine(message);
                }
            }
        }

        private async Task LogToFile(string message)
        {
            string logfile = @"D:\Aguila_Setup\EAGLE\eagle_log.txt";
            //logfile = @"C:\temp\EAGLE\AguilaRemoteControl\bin\Debug\log.txt";

            // Ensure the directory exists
            string logDirectory = Path.GetDirectoryName(logfile);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Asynchronously append the log entry to the log file
            using (StreamWriter streamWriter = new StreamWriter(logfile, append: true))
            {
                await streamWriter.WriteLineAsync(message);
            }
        }

        public List<string> CheckCheckBoxes()
        {
            List<string> selectedIps = new List<string>();
            for (int i = 1; i <= 36; i++)
            {
                CheckBox cb = this.Controls.Find("cb_" + i, true)[0] as CheckBox;
                if (cb != null && cb.Checked)
                {
                    string ip = $"10.250.0.{i}";
                    selectedIps.Add(ip);
                }

            }
            return selectedIps;
        }


        /////////////////// Button Clicked ///////////////////
        private void execute_btn_Click(object sender, EventArgs e)
        {
            // Disable the button for prevent multi click
            execute_btn.Enabled = false;

            string selectedCells = string.Join(",", CheckCheckBoxes());
            string command = run_config_box.Text + " " + selectedCells;
            
            // Create a new process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + $@"{command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Start the process using the Process class
            Process process = new Process { StartInfo = processStartInfo };

            // Subscribe to the OutputDataReceived event
            process.OutputDataReceived += (sender1, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    // This will be called in real-time when the process writes to its standard output
                    WriteConsole(args.Data); // Replace with your method to handle the output
                }
            };

            // Set EnableRaisingEvents to true to allow the Exited event to be raised
            process.EnableRaisingEvents = true;

            process.Exited += (sender1, args) =>
            {
                // Re-enable the button on the UI thread
                this.Invoke(new Action(() =>
                {
                    execute_btn.Enabled = true;
                    process.Close();
                }));
                //execute_btn.Enabled = true;
                //process.Dispose();
            };

            try
            {
                // Start the process
                process.Start();
                // Begin asynchronous read of the standard output stream
                process.BeginOutputReadLine();
                // Begin asynchronous read of the standard error stream (if needed)
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during process execution
                WriteConsole($"An error occurred: {ex.Message}");
                execute_btn.Enabled = true;
            }
        }

        // Ensure the RichTextBox is updated on the UI thread
        /*
            if (rtb_result.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    WriteConsole(iplog,logColor);
                }));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    WriteConsole(iplog,logColor);
                }));
            }
            */


        private void SetCheckBoxesCheckedState(IEnumerable<CheckBox> checkBoxes, bool checkedState)
        {
            foreach (var checkBox in checkBoxes)
            {
                checkBox.Checked = checkedState;
            }
        }
        private void cb_col_A_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_A.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_1, cb_2, cb_3, cb_4, cb_5, cb_6 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_1, cb_2, cb_3, cb_4, cb_5, cb_6 }, false);
            }

        }
        private void cb_col_B_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_B.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_7, cb_8, cb_9, cb_10, cb_11, cb_12 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_7, cb_8, cb_9, cb_10, cb_11, cb_12 }, false);
            }
        }
        private void cb_col_C_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_C.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_13, cb_14, cb_15, cb_16, cb_17, cb_18 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_13, cb_14, cb_15, cb_16, cb_17, cb_18 }, false);
            }
        }
        private void cb_col_D_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_D.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_19, cb_20, cb_21, cb_22, cb_23, cb_24 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_19, cb_20, cb_21, cb_22, cb_23, cb_24 }, false);
            }
        }
        private void cb_col_E_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_E.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_25, cb_26, cb_27, cb_28, cb_29, cb_30 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_25, cb_26, cb_27, cb_28, cb_29, cb_30 }, false);
            }
        }
        private void cb_col_F_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_col_F.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_31, cb_32, cb_33, cb_34, cb_35, cb_36 }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_31, cb_32, cb_33, cb_34, cb_35, cb_36 }, false);
            }
        }
        private void cb_select_all_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_select_all.Checked)
            {
                SetCheckBoxesCheckedState(new[] { cb_col_A, cb_col_B, cb_col_C, cb_col_D, cb_col_E, cb_col_F }, true);
            }
            else
            {
                SetCheckBoxesCheckedState(new[] { cb_col_A, cb_col_B, cb_col_C, cb_col_D, cb_col_E, cb_col_F }, false);
            }
        }

        public string[] cmds, names;
        private void AguilaRemoteControl_Load(object sender, EventArgs e)
        {
            // Add items to feature combo box
            string featureConfigPath = "FeatureConfig.config";
            // Load the XML document
            XDocument xmlDoc = XDocument.Load(featureConfigPath);
            // Query the document to retrieve the feature elements
            var features = xmlDoc.Descendants("feature")
                                 .Select(f => new
                                 {
                                     Name = f.Attribute("name").Value,
                                     Cmd = f.Attribute("cmd").Value
                                 })
                                 .ToList();
            // Convert the results to arrays
            names = features.Select(f => f.Name).ToArray();
            cmds = features.Select(f => f.Cmd).ToArray();
            // Add items from config file to feature combo box
            for (int i = 0; i < names.Length; i++) { comboBoxFeatures.Items.Add(names[i].ToString()); }
        }

        //private async void ScanCells_btn_Click(object sender, EventArgs e)
        //{
        //    List<string> onlineCells = new List<string>();
        //    List<string> cellsIp = new List<string>();

        //    // Get all cells IP into string list
        //    for (int i = 1; i <= 36; i++)
        //    {
        //        string ip = $"10.250.0.{i}";
        //        cellsIp.Add(ip);
        //    }

        //    foreach (string cellIp in cellsIp)
        //    {
        //        bool isOnline = false;
        //        string cellNum = cellIp.Split('.').ToList().Last();
        //        using (Ping pinger = new Ping())
        //        {
        //            try
        //            {
        //                PingReply reply = await pinger.SendPingAsync(cellIp);
        //                isOnline = reply.Status == IPStatus.Success;
        //            }
        //            catch (PingException)
        //            {
        //                // Handle any exceptions here
        //                isOnline = false;
        //            }
        //        }

        //        if (!isOnline)
        //        {
        //            CheckBox cb = this.Controls.Find("cb_" + cellNum, true)[0] as CheckBox;
        //            cb.Enabled = false;
        //            Console.WriteLine(cellNum + " is OFFLINE");
        //        }


        //    }
        //}

        private async void ScanCells_btn_Click(object sender, EventArgs e)
        {
            List<string> cellsIp = new List<string>();
            WriteLine("Scanning cells ONLINE/OFFLINE...");
            ScanCells_btn.Enabled = false;

            // Get all cells IP into string list
            for (int i = 1; i <= 36; i++)
            {
                string ip = $"10.250.0.{i}";
                cellsIp.Add(ip);
            }

            // Create a list to hold all the tasks
            List<Task> tasks = new List<Task>();

            foreach (string cellIp in cellsIp)
            {
                // Create a task for each cell IP
                tasks.Add(CheckCellOnline(cellIp));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            WriteLine("Scanning cells completed.");
            ScanCells_btn.Enabled = true;
        }

        private async Task CheckCellOnline(string cellIp)
        {
            bool isOnline = false;
            string cellNum = cellIp.Split('.').Last();
            using (Ping pinger = new Ping())
            {
                try
                {
                    PingReply reply = await pinger.SendPingAsync(cellIp);
                    isOnline = reply.Status == IPStatus.Success;
                }
                catch (PingException)
                {
                    // Handle any exceptions here
                    isOnline = false;
                }
            }

            // Since we're updating the UI, we need to make sure it's done on the UI thread
            this.Invoke((MethodInvoker)delegate
            {
                CheckBox cb = this.Controls.Find("cb_" + cellNum, true).FirstOrDefault() as CheckBox;
                if (cb != null)
                {
                    if (!isOnline)
                    {
                        cb.Enabled = isOnline;
                        WriteLine("Cell " + Site[Int32.Parse(cellNum)] + " is OFFLINE, disabling cell");
                    }
                    
                }
            });
        }

        private void comboBoxFeatures_SelectedIndexChanged(object sender, EventArgs e)
        {
            run_config_box.Text = cmds[Array.IndexOf(names, comboBoxFeatures.Text)];
        }
    }
}
