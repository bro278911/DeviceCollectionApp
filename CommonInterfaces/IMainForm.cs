using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface IMainForm
    {
        void AppendTextToRichTextBox(string text);
    }
    public interface IPlugin
    {
        void Start();
        void Stop();
    }
}
