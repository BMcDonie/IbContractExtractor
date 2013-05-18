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

using System.Collections.Generic;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System;

namespace IbContractExtractor
{
    public class Util
    {
        private static HtmlWeb web;
        private static int totalRequests = 0;
        public static int TotalRequests { get { return totalRequests; } }
        public static int CurrentRequests { get; set; }

        public static HtmlWeb HtmlWebInstance
        {
            get
            {
                if (web == null)
                {
                    web = new HtmlWeb();
                    web.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)";
                    web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);
                }

                return web;
            }
        }

        private static bool OnPreRequest(HttpWebRequest request)
        {
            // set the Accept header. IB is cranky about this. Get 400 Bad Request errors without it
            request.Accept = "text/html, application/xhtml+xml, */*";
            totalRequests++; // keep track of how many pages have been requested
            CurrentRequests++;

            return true;
        }

        /// <summary>
        /// Writes a list to a file. Assumes object T has overriden ToString().
        /// </summary>
        public static void WriteToFile<T>(string filename, List<T> list, bool append = false)
        {
            using (StreamWriter writer = new StreamWriter(filename, append))
            {
                list.ForEach(i => writer.WriteLine(i));
            }
        }

        internal static void InitFile(string filename, string header)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false))
                {
                    writer.WriteLine(header);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteError(ex.Message);
            }
        }
    }
}
