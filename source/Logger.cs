// This file is part of the "IBContractExtractor".
// Copyright (C) 2010 Shane Cusson (shane.cusson@vaultic.com)
// For conditions of distribution and use, see copyright notice in COPYING.txt

// IBContractExtractor is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// IBContractExtractor is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IbContractExtractor
{
    public class Logger : IDisposable
    {
        private StreamWriter writer;
        private string logMessage;
        private static Logger instance;

        public Logger(string filename)
        {
            InitLogger(filename);
        }

        private void InitLogger(string filename)
        {
            string logFile = string.Format(filename, DateTime.Now.ToString("yyyyMMdd"));

            writer = new StreamWriter(logFile, false);
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger("log.txt");
                }

                return instance;
            }
        }

        public void WriteHeader(string message, params object[] messageParams)
        {
            // write out 80 equals as a header, the message, then another
            Write(LogType.Header, new String('=', 80));
            Write(LogType.Header, message, messageParams);
        }

        public void WriteFooter(string message, params object[] messageParams)
        {
            // write out 80 dashes as a footer, the message, then another
            Write(LogType.Footer, new String('-', 80));
            Write(LogType.Footer, message, messageParams);
        }

        public void WriteInfo(string message, params object[] messageParams)
        {
            Write(LogType.Info, message, messageParams);
        }

        public void WriteError(string message, params object[] messageParams)
        {
            Write(LogType.Error, message, messageParams);
        }

        /// <summary>
        /// Writes an error message to the log. Sets the "ExecptionSource" parameter
        /// of the log then overrides it.
        /// </summary>
        /// <param name="source">The object throwing the exception.</param>
        /// <param name="message">The error message to parse and display.</param>
        /// <param name="messageParams">The individual message parameters.</param>
        public void WriteException(string source, string message, params object[] messageParams)
        {
            Write(LogType.Exception, string.Format("{0}: {1}",
                source, string.Format(message, messageParams)));
        }

        string formattedMessage;
        private void Write(LogType logType, string message, params object[] messageParams)
        {
            // This is to work around any situations where the message is expecting params
            // but didnt get them.
            if (messageParams.Count() > 0)
                formattedMessage = string.Format(message, messageParams);
            else
                formattedMessage = message;

            logMessage = string.Format("[{0}] {1}: {2}", DateTime.Now.ToString("yyyyMMdd HH:mm.ss"),
                logType, formattedMessage);

            Console.WriteLine(logMessage);

            writer.WriteLine(logMessage);
            writer.Flush();
        }

        public void Dispose()
        {
            writer.Close();
        }
    }

    public enum LogType
    {
        Header,
        Footer,
        Info,
        Error,
        Warning,
        Exception
    }
}
