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
        private AppDomain pluginAppDomain1;
        private AppDomain pluginAppDomain2;
        private string pluginDirectory;
        public Form1()
        {
            InitializeComponent();
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            comboBoxDevices.Items.Add("Device1Plugin");
            comboBoxDevices.Items.Add("Device2Plugin");
            pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(pluginDirectory))
            {
                Directory.CreateDirectory(pluginDirectory);
            }
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

            if (selectedDevice == "Device1Plugin" && Device1 != null)
            {
                Device1.Stop();
                UnloadPluginAppDomain(ref pluginAppDomain1);
                Device1 = null;
            }
            else if (selectedDevice == "Device2Plugin" && Device2 != null)
            {
                Device2.Stop();
                UnloadPluginAppDomain(ref pluginAppDomain2);
                Device2 = null;
            }
        }
        private void LoadAndStartPlugin(string pluginName)
        {
            string pluginPath = Path.Combine(pluginDirectory, $"{pluginName}.dll");
            if (File.Exists(pluginPath))
            {
                if (pluginName == "Device2Plugin")
                {
                    pluginAppDomain2 = AppDomain.CreateDomain("PluginAppDomain2");
                    var loader = (PluginLoader)pluginAppDomain2.CreateInstanceAndUnwrap(
                        typeof(PluginLoader).Assembly.FullName,
                        typeof(PluginLoader).FullName);

                    Device2 = loader.LoadPlugin(pluginPath, $"{pluginName}Namespace.{pluginName}", this);
                    Device2.Start();
                }
                else if(pluginName == "Device1Plugin")
                {
                    pluginAppDomain1 = AppDomain.CreateDomain("PluginAppDomain1");
                    var loader = (PluginLoader)pluginAppDomain1.CreateInstanceAndUnwrap(
                        typeof(PluginLoader).Assembly.FullName,
                        typeof(PluginLoader).FullName);

                    Device1 = loader.LoadPlugin(pluginPath, $"{pluginName}Namespace.{pluginName}", this);
                    Device1.Start();
                }
            }
            else
            {
                MessageBox.Show($"Plugin DLL not found: {pluginPath}");
            }
        }
        public class PluginLoader : MarshalByRefObject
        {
            public IPlugin LoadPlugin(string assemblyPath, string typeName, IMainForm mainForm)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var type = assembly.GetType(typeName);
                return (IPlugin)Activator.CreateInstance(type, new object[] { mainForm });
            }
        }
        private void UnloadPluginAppDomain(ref AppDomain appDomain)
        {
            if (appDomain != null)
            {
                AppDomain.Unload(appDomain);
                appDomain = null;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();
            string relativeNewPluginPath = $@"..\..\..\{selectedDevice}Namespace\bin\Debug\{selectedDevice}.dll";
            string pluginPath = Path.Combine(pluginDirectory, $"{selectedDevice}.dll");
            // Stop Device B
            if (selectedDevice == "Device1Plugin" && Device1 != null)
            {
                Device1.Stop();
                UnloadPluginAppDomain(ref pluginAppDomain1);

                Device1 = null;
            }
            else if (selectedDevice == "Device2Plugin" && Device2 != null)
            {
                Device2.Stop();
                UnloadPluginAppDomain(ref pluginAppDomain2);
                Device2 = null;
            }
            // Get the base directory of the application
            //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //string absoluteNewPluginPath = Path.GetFullPath(Path.Combine(baseDirectory, relativeNewPluginPath));
            // Copy new DLL to plugin directory
            try
            {
                // Copy new DLL to plugin directory
                File.Copy(relativeNewPluginPath, pluginPath, true);
                // Load and start updated Device
                //LoadAndStartPlugin($"{selectedDevice}");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error updating plugin: {ex.Message}");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
