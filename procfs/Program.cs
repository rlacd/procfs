using DokanNet;
using DokanNet.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procfs
{
    class Program
    {
        static void Main(string[] args)
        {
            Dokan.Mount(new ProcFS(), "p:\\", DokanOptions.RemovableDrive, 4, new NullLogger());
        }
    }

    class NullLogger : ILogger
    {
        public void Debug(string message, params object[] args)
        {
            
        }

        public void Error(string message, params object[] args)
        {
            
        }

        public void Fatal(string message, params object[] args)
        {
            
        }

        public void Info(string message, params object[] args)
        {
            
        }

        public void Warn(string message, params object[] args)
        {
            
        }
    }
}
