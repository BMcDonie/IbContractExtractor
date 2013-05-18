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
using System.IO;
using System.Linq;
using System.Reflection;

namespace IbContractExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ExtractContracts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}\n\nFull Exception:\n{1}", ex.Message, ex);
            }
        }

        private static void ExtractContracts()
        {
            // init general stuff
            DateTime startTime = DateTime.Now;
            Logger.Instance.WriteInfo("Start time: {0}", startTime);
            Util.InitFile("contracts.txt", Contract.GetHeader());

            // get the list of exchanges
            var exchanges = GetExchangeList();
            Util.WriteToFile<Exchange>("exchanges.txt", exchanges);

            // extract the contracts, one exchange at a time, appending to the contracts file
            var contracts = new List<Contract>();
            foreach (Exchange exchange in exchanges)
            {
                Logger.Instance.WriteInfo("Extracting exchange: {0}, type: {1}", exchange.Name, exchange.Category);

                // skip to next exchange if the category is blank
                if (!IsCategoryOk(exchange))
                    continue;

                // get the list of contracts for the exchange, then write to a file
                try
                {
                    contracts.AddRange(Contract.GetList(exchange));
                    Util.WriteToFile<Contract>("contracts.txt", contracts, true);
                    contracts.Clear();
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteError("Error: {0}", ex.Message);
                }

                Logger.Instance.WriteInfo("Web page requests for current exchange: {0}", Util.CurrentRequests);
                Util.CurrentRequests = 0; // reset
            }

            Logger.Instance.WriteInfo("Exctracted {0} exchanges. Total web pages requested: {1}",
                exchanges.Count(), Util.TotalRequests);

            DateTime endTime = DateTime.Now;
            Logger.Instance.WriteInfo("Extraction completed. Endtime: {0}, Total time taken: {1:0.00} minutes",
                endTime, (endTime - startTime).TotalMinutes);
        }

        private static bool IsCategoryOk(Exchange exchange)
        {
            if (exchange.Category == "")
            {
                Logger.Instance.WriteException("Blank category for exchange, no contracts extracted.",
                    exchange.Name);
                return false; // skip to the next exchange
            }
            else
                return true;
        }

        private static List<Exchange> GetExchangeList()
        {
            Logger.Instance.WriteInfo("Initializing exchange lists.");

            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/p.php?f=products");                   // north american stocks
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_stk");  // european stocks
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_stk");    // asian stocks

            // extract exchanges from urls
            var exchanges = new List<Exchange>();
            foreach(var url in urls)
                exchanges.AddRange(Exchange.GetList(url));

            return exchanges;
        }
    }
}
