using CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device2PluginNamespace
{
    public class Device2Plugin : MarshalByRefObject, IPlugin
    {
        private bool isRunning;
        private IMainForm mainForm;

        public Device2Plugin(IMainForm form)
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
                string message = "Device2 collecting data...測試完成2!";
                mainForm.AppendTextToRichTextBox(message);
                Task.Delay(1000).Wait();
            }
        }
    }
}
