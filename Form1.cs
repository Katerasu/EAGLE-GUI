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
using System.Speech.Synthesis;
using System.Management;
using System.Security.AccessControl;


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

        public string[] cmds, names;
        public AguilaRemoteControl()
        {
            InitializeComponent();

            WriteConsole($"========== Aguila Remote Control Version 1.0 ==========" + Environment.NewLine);

            LoadFeatures();

            abort_btn.Enabled = false;
            abort_btn.BackColor = Color.Gray;

            sendInput_btn.Enabled = false;
            sendInput_btn.BackColor = Color.Gray;

            inputBox.Enabled = false;
            inputBox.BackColor = Color.Gray;
        }

        /////////////////// Features ///////////////////
        private void LoadFeatures()
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
        private void comboBoxFeatures_SelectedIndexChanged(object sender, EventArgs e)
        {
            run_config_box.Text = cmds[Array.IndexOf(names, comboBoxFeatures.Text)];
        }

        /////////////////// Write Text To Rich Text Box ///////////////////
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


        // Define a structure to hold word-color mappings
        private struct WordColorMapping
        {
            public string Word;
            public Color TextColor;
            public Color BackColor;
        }

        // Load the color configuration from the XML file
        private WordColorMapping[] LoadColorConfiguration(string colorConfigPath)
        {
            XDocument xmlDoc = XDocument.Load(colorConfigPath);
            var mappings = xmlDoc.Descendants("word")
                                 .Select(f => new WordColorMapping
                                 {
                                     Word = f.Attribute("word").Value.ToLower(),
                                     TextColor = ColorTranslator.FromHtml(f.Attribute("color").Value),
                                     BackColor = ColorTranslator.FromHtml(f.Attribute("backColor").Value)
                                 })
                                 .ToArray();
            return mappings;
        }

        void WriteLine(string message_child)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string messageToWrite = timestamp + ": " + message_child + "\n";
            // Set color base on string

            // Load the color configuration
            string colorConfigPath = "WriteLineColor.config";
            var colorMappings = LoadColorConfiguration(colorConfigPath);

            // Find the color mapping based on the message content
            var mapping = colorMappings.FirstOrDefault(m => messageToWrite.ToLower().Contains(m.Word));
            Color textColor = mapping.TextColor != default ? mapping.TextColor : Color.Black;
            Color backColor = mapping.BackColor != default ? mapping.BackColor : rtb_result.BackColor;

            // Check if an invoke is required for the control
            if (rtb_result.InvokeRequired)
            {
                // Use Invoke to handle the update on the UI thread
                rtb_result.Invoke(new Action(() =>
                {
                    AppendLineToConsole(messageToWrite, textColor, backColor);
                }));
            }
            else
            {
                AppendLineToConsole(messageToWrite, textColor, backColor);
            }

            // Log to file
            Task.Run(() => LogToFile(messageToWrite.Replace("\n", "")));
        }

        private void WriteConsole(string message)
        {
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

        /////////////////// Cells Check Box ///////////////////
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

        private void SetCheckBoxesCheckedState(IEnumerable<CheckBox> checkBoxes, bool checkedState)
        {
            foreach (var checkBox in checkBoxes)
            {
                if (checkBox.Enabled)
                {
                    checkBox.Checked = checkedState;
                }
            }
        }

        private void cb_col_A_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_1, cb_2, cb_3, cb_4, cb_5, cb_6 }, cb_col_A.Checked);
        }

        private void cb_col_B_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_7, cb_8, cb_9, cb_10, cb_11, cb_12 }, cb_col_B.Checked);
        }

        private void cb_col_C_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_13, cb_14, cb_15, cb_16, cb_17, cb_18 }, cb_col_C.Checked);
        }

        private void cb_col_D_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_19, cb_20, cb_21, cb_22, cb_23, cb_24 }, cb_col_D.Checked);
        }

        private void cb_col_E_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_25, cb_26, cb_27, cb_28, cb_29, cb_30 }, cb_col_E.Checked);
        }

        private void cb_col_F_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_31, cb_32, cb_33, cb_34, cb_35, cb_36 }, cb_col_F.Checked);
        }

        private void cb_select_all_CheckedChanged(object sender, EventArgs e)
        {
            SetCheckBoxesCheckedState(new[] { cb_col_A, cb_col_B, cb_col_C, cb_col_D, cb_col_E, cb_col_F }, cb_select_all.Checked);
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
                        cb.Checked = isOnline;
                        cb.BackColor = Color.Gray;
                        WriteLine("Cell " + Site[Int32.Parse(cellNum)] + " is OFFLINE, disabling cell");
                    }
                    else
                    {
                        cb.Enabled = isOnline;
                        cb.BackColor = gb_cell_selection.BackColor;
                    }

                }
            });
        }

        private async void ScanCells_btn_Click(object sender, EventArgs e)
        {
            List<string> cellsIp = new List<string>();
            WriteLine("Scanning cells status...");
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

        /////////////////// Button Clicked ///////////////////



        private Dictionary<string, Color> orgColor = new Dictionary<string, Color>
        {
            { "execute_btn", Color.SpringGreen },
            { "abort_btn", Color.Coral },
            { "sendInput_btn", Color.SkyBlue },
        };

        private Process process;
        private StreamWriter streamWriter;
        private void execute_btn_Click(object sender, EventArgs e)
        {
            // Disable the button for prevent multi click
            execute_btn.Enabled = false;
            execute_btn.BackColor = Color.Gray;
            // Enable abort button
            abort_btn.Enabled = true;
            abort_btn.BackColor = Color.Coral;

            string selectedCells = string.Join(",", CheckCheckBoxes());
            string command = run_config_box.Text + " " + selectedCells;
            
            // Create a new process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + $@"{command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };


            // Start the process using the Process class
            process = new Process { StartInfo = processStartInfo };
            

            // Subscribe to the OutputDataReceived event
            process.OutputDataReceived += (sender1, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    // This will be called in real-time when the process writes to its standard output
                    WriteConsole(args.Data); // Replace with your method to handle the output

                    // Check if the process is waiting for input
                    string messageLower = args.Data.ToLower();
                    if (messageLower.Contains("waiting for input"))
                    {
                        // Enable the sendInput button on the UI thread
                        this.Invoke(new Action(() =>
                        {
                            sendInput_btn.Enabled = true;
                            sendInput_btn.BackColor = Color.SkyBlue;
                            inputBox.Enabled = true;
                            inputBox.BackColor = rtb_result.BackColor;
                        }));
                    }
                }
            };

            // Set EnableRaisingEvents to true to allow the Exited event to be raised
            process.EnableRaisingEvents = true;

            

            process.Exited += (sender1, args) =>
            {
                // Re-enable the button on the UI thread
                this.Invoke(new Action(() =>
                {
                    // Enable execute button
                    execute_btn.Enabled = true;
                    execute_btn.BackColor = Color.SpringGreen;
                    // Disable abort button
                    abort_btn.Enabled = false;
                    abort_btn.BackColor = Color.Gray;
                    process.Close();
                    process = null;
                }));
                //execute_btn.Enabled = true;
                //process.Dispose();
            };

            try
            {
                // Start the process
                process.Start();
                streamWriter = process.StandardInput;
                // Begin asynchronous read of the standard output stream
                process.BeginOutputReadLine();
                // Begin asynchronous read of the standard error stream (if needed)
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during process execution
                WriteConsole($"An error occurred: {ex.Message}");
                // Enable execute button
                execute_btn.Enabled = true;
                execute_btn.BackColor = Color.SpringGreen;
                // Disable abort button
                abort_btn.Enabled = false;
                abort_btn.BackColor = Color.Gray;
            }
        }

        private void KillProcessAndChildren(int pid)
        {
            // Create an instance of the ManagementObjectSearcher class to find all processes
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                // Kill the child process
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                // Kill the main process after the children have been killed
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private void abort_btn_Click(object sender, EventArgs e)
        {
            // Close process
            // If the process is still running after the wait, force it to stop
            KillProcessAndChildren(process.Id);

            // Restart GUI
            execute_btn.Enabled = true;
            execute_btn.BackColor = orgColor["execute_btn"];

            sendInput_btn.Enabled = false;
            sendInput_btn.BackColor = Color.Gray;

            inputBox.Enabled = false;
            inputBox.BackColor = Color.Gray;

        }

        private void sendInput_btn_Click(object sender, EventArgs e)
        {
            // Check if the process is running and if inputBox has text
            if (process != null && !process.HasExited && !string.IsNullOrWhiteSpace(inputBox.Text))
            {
                string input = inputBox.Text;

                //// Write the input from inputBox to the process's standard input
                //using (StreamWriter sw = process.StandardInput)
                //{
                //    sw.WriteLine(input);
                //}

                // Convert the input string to bytes and write to the process's standard input
                streamWriter.WriteLine(input);

                // Optionally, clear the inputBox after sending the input
                inputBox.Clear();
                // Disable sendInput btn after it's been clicked
                sendInput_btn.Enabled = false;
                sendInput_btn.BackColor = Color.Gray;
                inputBox.Enabled = false;
                inputBox.BackColor = Color.Gray;

            }
            else
            {
                // Handle the case where the process is not running or inputBox is empty
                WriteConsole("Failed to send input, process is not running or input is empty.");
            }
        }

        private void clear_console_Click(object sender, EventArgs e)
        {
            rtb_result.Clear();
        }


    }
}
