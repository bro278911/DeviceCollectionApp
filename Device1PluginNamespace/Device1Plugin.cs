using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;

namespace Device1PluginNamespace
{
    public class Device1Plugin : MarshalByRefObject,IPlugin
    {
        private bool isRunning;
        private IMainForm mainForm;

        public Device1Plugin(IMainForm form)
        {
            mainForm = form;
        }

        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                Task.Run(() => CollectData());
            }
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void CollectData()
        {
            while (isRunning)
            {
                string message = "Device1 collecting data...測試完成!";
                mainForm.AppendTextToRichTextBox(message);
                Task.Delay(1000).Wait();
            }
        }
    }
}
