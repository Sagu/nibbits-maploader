using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Nibbler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += Logger.LogException;

            if (args.Length == 0)
            {
                //Check if Nibbler is installed
                if (!SelfInstall.IsInstalled())
                    SelfInstall.Install();
                else
                {
                    Application.Run(new LoaderConfig());
                }
            }
            else
            {
                switch (args[0].ToLower())
                {
                    case "-uninstall":
                    case "/uninstall":
                    case "nibbler://uninstall":
                        //Confirmation dialog will be needed to avoid a malicious uninstall
                        SelfInstall.Uninstall();
                        break;
                    case "-config":
                    case "/config":
                    case "nibbler://config":
                        Application.Run(new LoaderConfig());
                        break;
                    default:
                        {
                            string link = args[0];

                            //I recommend relocating "nibbits" somewhere, and adding some logic to register the helper.
                            link = link.Replace(link.Substring(0, "nibbler".Length), "http");

                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new LoaderForm(link));
                            break;
                        }
                }
            }
        }
    }
}
