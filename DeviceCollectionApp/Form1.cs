using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonInterfaces;

namespace DeviceCollectionApp
{
    public partial class Form1 : Form, IMainForm
    {
        private IPlugin Device1;
        private IPlugin Device2;
        public Form1()
        {
            InitializeComponent();
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            comboBoxDevices.Items.Add("Device1Plugin");
            comboBoxDevices.Items.Add("Device2Plugin");
        }
        public void AppendTextToRichTextBox(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText(text + Environment.NewLine);
                });
            }
            else
            {
                richTextBox1.AppendText(text + Environment.NewLine);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();
            if (selectedDevice != null)
            {
                LoadAndStartPlugin(selectedDevice);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();

            if (selectedDevice == "Device1Plugin")
            {
                Device1.Stop();
            }
            else if (selectedDevice == "Device2Plugin")
            {
                Device2.Stop();
            }
        }
        private void LoadAndStartPlugin(string pluginName)
        {
            string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{pluginName}.dll");
            if (File.Exists(pluginPath))
            {
                Assembly assembly = Assembly.LoadFrom(pluginPath);
                Type pluginType = assembly.GetType($"{pluginName}Namespace.{pluginName}");
                if (pluginType != null && typeof(IPlugin).IsAssignableFrom(pluginType) && pluginName == "Device1Plugin")
                {
                    Device1 = (IPlugin)Activator.CreateInstance(pluginType, this);
                    Device1.Start();
                }
                else if(pluginType != null && typeof(IPlugin).IsAssignableFrom(pluginType) && pluginName == "Device2Plugin")
                {
                    Device2 = (IPlugin)Activator.CreateInstance(pluginType, this);
                    Device2.Start();
                }
                else
                {
                    Console.WriteLine("ERROR");
                }
            }
        }
    }
}
