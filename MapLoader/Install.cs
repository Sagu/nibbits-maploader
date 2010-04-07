using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

namespace Nibbler
{
    public static class SelfInstall
    {
        public struct RegistryEntry
        {
            public RegistryKey Hive;
            public string Key;
            public string Name;
            public string Value;
            public Values ValueType;
            //Used to identify special test conditions beyond equality
            public enum Values
            {
                DEFAULT,
                INVOCATION_COMMAND
            }
        }

        /// <summary>
        /// Registry Key [Name, Value]
        /// </summary>
        private static List<RegistryEntry> _registryKeys = new List<RegistryEntry>()
        {
            new RegistryEntry() { Hive = Registry.ClassesRoot, Key = @"nibbits", Name = "@", Value = "URL:Nibbits Protocol Handler" },
            new RegistryEntry() { Hive = Registry.ClassesRoot, Key = @"nibbits", Name = "URL Protocol", Value = string.Empty },
            new RegistryEntry() { Hive = Registry.ClassesRoot, Key = @"nibbits\shell\open\command", Name = "@", ValueType = RegistryEntry.Values.INVOCATION_COMMAND }
        };
        /// <summary>
        /// Check if Nibbler is installed and the registry is sane
        /// </summary>
        /// <returns>True if installed, false if damaged or not installed</returns>
        public static bool IsInstalled()
        {
            try
            {
                foreach (RegistryEntry re in _registryKeys)
                {
                    if (re.Key == null)
                    {
                        //Registry entry doesn't exist
                        return false;
                    }
                    else
                    {
                        if (re.ValueType == RegistryEntry.Values.INVOCATION_COMMAND)
                        {
                            //Parse the invocation command and verify the executable exists
                            string _command = re.Hive.OpenSubKey(re.Key).GetValue(re.Name) as string;
                            string _file = null;
                            if (!string.IsNullOrEmpty(_command))
                            {
                                //Super dirty hacky way of stripping out command args
                                for (int i = _command.Length; i > 1; i--)
                                {
                                    if (System.IO.File.Exists(_command.Substring(0, i)))
                                    {
                                        _file = _command.Substring(0, i);
                                        break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(_file))
                            {
                                using (HashAlgorithm hashAlg = new SHA1Managed())
                                {
                                    byte[] sourceHash;
                                    byte[] targetHash;
                                    using (Stream file = new FileStream(_file, FileMode.Open, FileAccess.Read))
                                    {
                                        targetHash = hashAlg.ComputeHash(file);
                                    }
                                    using (Stream file = new FileStream(System.Reflection.Assembly.GetExecutingAssembly().Location, FileMode.Open, FileAccess.Read))
                                    {
                                        sourceHash = hashAlg.ComputeHash(file);
                                    }
                                    if (!targetHash.Equals(sourceHash))
                                    {
                                        //File being executed doesn't match installed version
                                        return false;
                                    }
                                }
                            }
                        }
                        if (re.Value != re.Hive.OpenSubKey(re.Key).GetValue(re.Name) as string)
                        {
                            //Registry is damaged
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Self-Install
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool Install()
        {
            //Check for admin
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                //Relaunch as admin
                var processInfo = new ProcessStartInfo() { Verb = "runas", FileName = System.Reflection.Assembly.GetExecutingAssembly().Location, UseShellExecute = true };
                try
                {
                    Process.Start(processInfo);
                }
                catch (Win32Exception e)
                {
                    //User cancelled
                }
                finally
                {
                    Environment.Exit(0);
                }

                //Should not be needed...odd
                return false;
            }
            else
            {
                try
                {
                    //Install
                    //Note to zeeg - you might want to create a simple install form here.
                    //#error Install location is stubbed
                    var _temp_location_identifier = string.Empty;

                    //Create directory and copy binary
                    System.IO.Directory.CreateDirectory(_temp_location_identifier);
                    System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, _temp_location_identifier + "/" + Environment.GetCommandLineArgs()[0]);

                    //Create registry entries
                    for (int index = 0; index < _registryKeys.Count; index++)
                    {
                        RegistryEntry re = _registryKeys[index];

                        //Prepare the invocation command
                        if (re.ValueType == RegistryEntry.Values.INVOCATION_COMMAND)
                            re.Value = _temp_location_identifier + @" ""%1""";
                        //Create the registry key and keypair;
                        re.Hive.CreateSubKey(re.Key);
                        re.Hive.OpenSubKey(re.Key).SetValue(re.Name, re.Value);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Installation failed", e);
                }
            }
        }

        /// <summary>
        /// Self-Uninstall
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool Uninstall()
        {
            //Check for admin
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                //Relaunch as admin
                var processInfo = new ProcessStartInfo() { Verb = "runas", FileName = System.Reflection.Assembly.GetExecutingAssembly().Location, UseShellExecute = true };
                try
                {
                    Process.Start(processInfo);
                }
                catch (Win32Exception e)
                {
                    //User cancelled
                }
                finally
                {
                    Environment.Exit(0);
                }

                //Should not be needed...odd
                return false;
            }
            else
            {
                try
                {
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Uninstallation failed", e);
                }
            }
        }
    }
}
