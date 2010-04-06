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
                if (Registry.ClassesRoot.OpenSubKey("nibbits") == null)
                {
                    //Not installed, check for admin
                    if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        //Relaunch as admin
                        var processInfo = new ProcessStartInfo() { Verb = "runas", FileName = System.Reflection.Assembly.GetExecutingAssembly().Location, UseShellExecute = true};
                        try
                        {
                            Process.Start(processInfo);
                        }
                        catch (Win32Exception e)
                        {
                            //User cancelled, exit.
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        //Install
//Note to zeeg - you might want to create a simple install form here.
#error Install location is stubbed
                        var _temp_location_identifier = string.Empty;
                        System.IO.Directory.CreateDirectory(_temp_location_identifier);
                        System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, _temp_location_identifier + "/" + Environment.GetCommandLineArgs()[0]);
                    }
                }
                else
                {
                    Application.Run(new LoaderConfig());
                }
            }
            else
            {

                string link = args[0];

                //I recommend relocating "nibbits" somewhere, and adding some logic to register the helper.
                link = link.Replace(link.Substring(0, "nibbler".Length), "http");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LoaderForm(link));
            }
        }
    }
}
