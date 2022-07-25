using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using static System.Net.Mime.MediaTypeNames;

namespace ImportComprovante
{
   

    class Program : ServiceBase
    {
        private void SetStartup(string AppName, bool enable)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue(AppName) == null)
                {
                    startupKey.Close();
                    startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                    // Add startup reg key
                    startupKey.SetValue(AppName, System.Reflection.Assembly.GetExecutingAssembly().Location);
                    startupKey.Close();
                }
            }
            else
            {
                // remove startup
                startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                startupKey.DeleteValue(AppName, false);
                startupKey.Close();
            }
        }
        static void Main(string[] args)
        {
            ServiceInstaller serviceInstaller1 = new ServiceInstaller();
            if (System.Environment.UserInteractive)
            {
                serviceInstaller1.ServiceName = "Retorno de dados bancários";
                serviceInstaller1.Description = "Processamento de comprovantes de retorno";


                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":

                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new Program());
            }
        }

        protected override void OnStart(string[] args)
        {
            LeituraTxt.Ler();   
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
