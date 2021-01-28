using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogToAzure
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            bool ok = ILogDLL.ILog.SaveLogToILog(
                "Comment DLL", 
                "Evorec", 
                version, 
                "INFO", 
                "Test Insertion Log", 
                "Tout s'est passé comme prévu"
            );
        }
    }
}
