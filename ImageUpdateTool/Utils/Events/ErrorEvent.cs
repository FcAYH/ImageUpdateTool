using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Utils.Events
{
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
    public class ErrorEventArgs : System.EventArgs
    {
        public string Message { get; set; }
        public string Title { get; set; } = "Error";

        public ErrorEventArgs() { }
        public ErrorEventArgs(string message)
        {
            Message = message;
        }
        public ErrorEventArgs(string message, string title)
        {
            Message = message;
            Title = title;
        }
    }
}
