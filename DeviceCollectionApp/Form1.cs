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
        private enum DeviceStatus
        {
            NotLoaded,
            Loading,
            Running,
            Stopping,
            Stopped,
            Error
        }

        private class DeviceInfo
        {
            public IPlugin Plugin { get; set; }
            public AppDomain AppDomain { get; set; }
            public DeviceStatus Status { get; set; }
            public string Name { get; set; }
        }

        private readonly Dictionary<string, DeviceInfo> devices;
        private string pluginDirectory;
        private bool isDisposed;

        public Form1()
        {
            InitializeComponent();
            devices = new Dictionary<string, DeviceInfo>
            {
                { "Device1Plugin", new DeviceInfo { Status = DeviceStatus.NotLoaded, Name = "Device1Plugin" } },
                { "Device2Plugin", new DeviceInfo { Status = DeviceStatus.NotLoaded, Name = "Device2Plugin" } }
            };
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            foreach (var device in devices.Keys)
            {
                comboBoxDevices.Items.Add(device);
            }
            
            pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(pluginDirectory))
            {
                Directory.CreateDirectory(pluginDirectory);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDevice))
            {
                MessageBox.Show("請選擇要啟動的設備!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var device = devices[selectedDevice];
            if (device.Status == DeviceStatus.Running)
            {
                MessageBox.Show("設備已經在運行中!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                LoadAndStartPlugin(selectedDevice);
            }
            catch (Exception ex)
            {
                devices[selectedDevice].Status = DeviceStatus.Error;
                LogError($"啟動設備 {selectedDevice} 時發生錯誤: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDevice))
            {
                MessageBox.Show("請選擇要停止的設備!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var device = devices[selectedDevice];
            if (device.Status != DeviceStatus.Running)
            {
                MessageBox.Show("設備未在運行中!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                device.Status = DeviceStatus.Stopping;
                StopDevice(selectedDevice);
                device.Status = DeviceStatus.Stopped;
                LogMessage($"設備 {selectedDevice} 已停止");
            }
            catch (Exception ex)
            {
                device.Status = DeviceStatus.Error;
                LogError($"停止設備 {selectedDevice} 時發生錯誤: {ex.Message}");
            }
        }

        private void LoadAndStartPlugin(string pluginName)
        {
            var device = devices[pluginName];
            device.Status = DeviceStatus.Loading;
            LogMessage($"正在加載設備 {pluginName}...");

            try
            {
                string pluginPath = Path.Combine(pluginDirectory, $"{pluginName}.dll");
                if (!File.Exists(pluginPath))
                {
                    throw new FileNotFoundException($"找不到插件文件: {pluginPath}");
                }

                var appDomain = AppDomain.CreateDomain($"PluginDomain_{pluginName}_{Guid.NewGuid()}");
                var loader = (PluginLoader)appDomain.CreateInstanceAndUnwrap(
                    typeof(PluginLoader).Assembly.FullName,
                    typeof(PluginLoader).FullName);

                var plugin = loader.LoadPlugin(pluginPath, $"{pluginName}Namespace.{pluginName}", this);
                
                device.AppDomain = appDomain;
                device.Plugin = plugin;
                device.Status = DeviceStatus.Running;
                
                plugin.Start();
                LogMessage($"設備 {pluginName} 已成功啟動");
            }
            catch
            {
                device.Status = DeviceStatus.Error;
                throw;
            }
        }

        private void StopDevice(string pluginName)
        {
            var device = devices[pluginName];
            try
            {
                device.Plugin?.Stop();
                UnloadPluginAppDomain(device.AppDomain);
                device.Plugin = null;
                device.AppDomain = null;
            }
            catch (Exception)
            {
                device.Status = DeviceStatus.Error;
                throw;
            }
        }

        private void UnloadPluginAppDomain(AppDomain appDomain)
        {
            if (appDomain != null)
            {
                try
                {
                    AppDomain.Unload(appDomain);
                }
                catch (Exception ex)
                {
                    LogError($"卸載 AppDomain 時發生錯誤: {ex.Message}");
                    throw;
                }
            }
        }

        private void LogMessage(string message)
        {
            AppendTextToRichTextBox($"[INFO] {message}");
        }

        private void LogError(string message)
        {
            AppendTextToRichTextBox($"[ERROR] {message}");
            MessageBox.Show(message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void AppendTextToRichTextBox(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText($"{text}{Environment.NewLine}");
                    richTextBox1.ScrollToCaret();
                });
            }
            else
            {
                richTextBox1.AppendText($"{text}{Environment.NewLine}");
                richTextBox1.ScrollToCaret();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //雙開更新後還有點bug
            string selectedDevice = comboBoxDevices.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDevice))
            {
                MessageBox.Show("請選擇要更新的設備!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string relativeNewPluginPath = $@"..\..\..\{selectedDevice}Namespace\bin\Debug\{selectedDevice}.dll";
            string pluginPath = Path.Combine(pluginDirectory, $"{selectedDevice}.dll");

            try
            {
                var device = devices[selectedDevice];
                if (device.Status == DeviceStatus.Running)
                {
                    StopDevice(selectedDevice);
                }

                File.Copy(relativeNewPluginPath, pluginPath, true);
                LogMessage($"設備 {selectedDevice} 更新成功");
            }
            catch (Exception ex)
            {
                LogError($"更新設備時發生錯誤: {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
    }
}
