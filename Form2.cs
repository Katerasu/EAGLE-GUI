using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AguilaRemoteControl
{
    public partial class Form2 : Form
    {
        private Dictionary<int, string> Site = new Dictionary<int, string>
        {
            {1, "A101"},    {2, "A201"},    {3, "A301"},    {4, "A401"},    {5, "A501"},    {6, "A601"},
            {7, "B101"},    {8, "B201"},    {9, "B301"},    {10, "B401"},   {11, "B501"},   {12, "B601"},
            {13, "C101"},   {14, "C201"},   {15, "C301"},   {16, "C401"},   {17, "C501"},   {18, "C601"},
            {19, "D101"},   {20, "D201"},   {21, "D301"},   {22, "D401"},   {23, "D501"},   {24, "D601"},
            {25, "E101"},   {26, "E201"},   {27, "E301"},   {28, "E401"},   {29, "E501"},   {30, "E601"},
            {31, "F101"},   {32, "F201"},   {33, "F301"},   {34, "F401"},   {35, "F501"},   {36, "F601"}
        };


        public Form2()
        {
            InitializeComponent();
        }
        //Ping cell online
        //private bool PingHost(string nameOrAddress)
        //{
        //    bool pingable = false;
        //    Ping pinger = new Ping();
        //    try
        //    {
        //        PingReply reply = pinger.Send(nameOrAddress);
        //        pingable = reply.Status == IPStatus.Success;
        //        MessageBox.Show("Ping to " + nameOrAddress + pingable);
        //    }
        //    catch (PingException ex)
        //    {
        //        // Write log with the exception message
        //        MessageBox.Show("Ping to " + nameOrAddress + " failed: " + ex.Message);

        //        // Discard PingExceptions and return false;
        //    }
        //    return pingable;
        //}
        private void button_transfer_Click(object sender, EventArgs e)
        {
            List<int> selectedCells = new List<int>();
            for (int i = 1; i <= 36; i++)
            {
                CheckBox cb = this.Controls.Find("cb_" + i, true)[0] as CheckBox;
                if (cb != null && cb.Checked)
                {
                    selectedCells.Add(i);
                }
            }

           
            foreach (int cellNumber in selectedCells)
            {
                string cellIp = "10.250.0." + cellNumber;
                string sstCell = Site[cellNumber];
                //string destination = @"\\" + cellIp + @"\C$\STHI\TestProgram\" + tp;
                
                bool pingable = false;
                Ping pinger = new Ping();

                PingReply reply = pinger.Send(cellIp);
                pingable = reply.Status == IPStatus.Success;
                MessageBox.Show("Ping to " + sstCell + pingable);
            }
            
        }
    }
}
