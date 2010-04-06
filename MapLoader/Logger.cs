using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nibbler
{
    internal class Logger
    {
        internal static string LogFile;
        private static StreamWriter _logWriter;

        private static StringBuilder PrepareException(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("===========================================================================");
            sb.AppendLine(String.Format("System Environment is {0} With {1} Logical Cores", Environment.OSVersion.Platform, Environment.ProcessorCount));
            sb.AppendLine(String.Format("Application Arguments were {0}", Environment.GetCommandLineArgs()));
            sb.AppendLine("---------------------------------------------------------------------------");
            sb.AppendLine(String.Format("Exception occurred at {0}", DateTime.Now));
            sb.AppendLine("---------------------------------------------------------------------------");
            sb.AppendLine(String.Format("Outer Exception: {0}",ex.Message));
            if (ex.TargetSite != null)
                sb.AppendLine(String.Format("Faulting Method: {0}",ex.TargetSite.Name));
            if (ex.InnerException != null)
                sb.AppendLine(String.Format("Inner Exception: {0}",ex.InnerException.Message));
            sb.AppendLine(String.Format("Stack Trace{0}{1}",Environment.NewLine,ex.StackTrace));
            sb.AppendLine("===========================================================================");
            sb.AppendLine();
            return sb;
        }

        #region Exception handler overloads
        /// <summary>
        /// Logs an exception to a file and optionally alerts the user
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="showMsgBox">Alert the user</param>
        internal static void LogException(Exception e, bool showMsgBox)
        {
            LogException_Real(e, showMsgBox);
        }

        /// <summary>
        /// Logs an exception to a file
        /// </summary>
        /// <param name="e">Exception</param>
        internal static void LogException(Exception e)
        {
            LogException_Real(e, false);
        }

        /// <summary>
        /// Logs an exception to a file
        /// </summary>
        /// <param name="message">Exception Message</param>
        internal static void LogException(string message)
        {
            var e = new Exception(message);
            LogException_Real(e, true);
        }

        /// <summary>
        /// Logs an exception to a file
        /// </summary>
        /// <param name="message">Exception Message</param>
        /// <param name="innerException">Inner Exception</param>
        internal static void LogException(string message, Exception innerException)
        {
            var e = new Exception(message,innerException);
            LogException_Real(e, true);
        }

        internal static void LogException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException_Real((Exception)e.ExceptionObject, true);
        }

        internal static void LogException(object sender, Exception e)
        {
            LogException_Real(e, true);
        }
        #endregion

        private static void LogException_Real(Exception ex, bool showMsgBox)
        {
            var sb = PrepareException(ex);
            if (string.IsNullOrEmpty(LogFile))
                LogFile = Environment.GetCommandLineArgs()[0] + ".log";
            if (!File.Exists(LogFile))
            {
                try
                {
                    _logWriter = File.CreateText(LogFile);
                }
                catch (UnauthorizedAccessException e)
                {
                    sb.Insert(0, PrepareException(e));
                    _logWriter = null;
                }
                catch (IOException e)
                {
                    sb.Insert(0, PrepareException(e));
                    _logWriter = null;
                }
            }
            else
            {
                try
                {
                    _logWriter = File.AppendText(LogFile);
                }
                catch (UnauthorizedAccessException e)
                {
                    sb.Insert(0, PrepareException(e));
                    _logWriter = null;
                }
                catch (IOException e)
                {
                    sb.Insert(0, PrepareException(e));
                    _logWriter = null;
                }
            }
            if (_logWriter != null)
            {
                string s = string.Format("An exception occurred and has been written to a logfile at {0}", LogFile + Environment.NewLine);
                if (showMsgBox)
                    MessageBox.Show(s + sb, "Error");
                _logWriter.Write(sb.ToString());
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter = null;
            }
            else
            {
                string s = string.Format("An exception occurred, additionally an error was encountered when writing to the logfile at {0}", LogFile + Environment.NewLine);
                if (showMsgBox)
                    MessageBox.Show(s + sb, "Error");
            }
        }
    }
}