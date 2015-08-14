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

            GetStocks();
            GetOptions();
            GetFutures();
            GetOptionsOnFutures();
            GetSingleStockFutures();
            GetForex();
            GetIndices();
            GetExchangeTradedFunds();
            GetWarrants();
            
            //// The products below are listed on the site but don't have contract id information

            //// GetMetals();
            //// GetStructuredProducts(); // no contract id info on website
            //// GetBonds(); // not implemented - no contract id info on ib website
            //// GetFunds(); // not implemented - no contract id's, just prospectus info
            //// GetContractsForDifference(); // not implemented yet

            DateTime endTime = DateTime.Now;
            Logger.Instance.WriteInfo("Extraction completed. Endtime: {0}, Total time taken: {1:0.00} minutes",
                endTime, (endTime - startTime).TotalMinutes);
        }

        private static void GetContracts(List<string> urls, string contractType)
        {
            // get the list of exchanges
            var exchanges = GetExchangeList(urls, contractType);
            var exchangeFilename = string.Format("exchanges-{0}.txt", contractType);
            Util.InitFile(exchangeFilename, Exchange.GetHeader());
            Util.WriteToFile<Exchange>(exchangeFilename, exchanges);

            var filename = string.Format("contracts-{0}.txt", contractType);
            Util.InitFile(filename, Contract.GetHeader());
            
            // extract the contracts, one exchange at a time, appending to the contracts file            
            foreach (Exchange exchange in exchanges)
            {
                Logger.Instance.WriteInfo("Extracting exchange: {0}, type: {1}", exchange.Name, exchange.Category);

                // skip to next exchange if the category is blank
                if (!IsCategoryOk(exchange))
                    continue;

                // get the list of contracts for the exchange, then write to a file
                try
                {
                    var contracts = Contract.GetList(exchange);
                    Util.WriteToFile<Contract>(filename, contracts);
                    contracts.Clear();
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteError("Error: {0}", ex.Message);
                }

                Logger.Instance.WriteInfo("Web page requests for current exchange: {0}", Util.CurrentRequests);
                Util.CurrentRequests = 0; // reset
            }

            Logger.Instance.WriteInfo("Extracted {0} exchanges. Total web pages requested: {1}",
                exchanges.Count(), Util.TotalRequests);
        }

        private static void GetContractsForDifference()
        {
            // no conract id info on ib website
            throw new NotImplementedException();
        }

        private static void GetFunds()
        {
            // no contract id info on ib website
            throw new NotImplementedException();
        }

        private static void GetBonds()
        {
            // no contract id info on ib website
            throw new NotImplementedException();
        }

        private static void GetIndices()
        {
            var contractType = "INDICIES";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=ind");         // north american indicies
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_ind");  // european indicies
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_ind");    // asian options

            GetContracts(urls, contractType);
        }

        private static void GetMetals()
        {
            throw new NotImplementedException();
        }

        private static void GetForex()
        {
            var contractType = "FOREX";
            var url = "http://www.interactivebrokers.com/en/trading/exchanges.php?exch=ibfxpro&showcategories=FX&ib_entity=llc";

            var doc = Util.HtmlWebInstance.Load(url);

            var rows = doc.DocumentNode.Descendants("tr")
                        .Where(x => x.Descendants("td").Count() > 2
                                    && x.Descendants("td").ToList()[1].Descendants("a").Count() > 0
                                    && x.Descendants("td").ToList()[1].Descendants("a").ToList()[0]
                                            .Attributes["href"].Value.Contains("conid"));

            var contracts = new List<Contract>();
            foreach (var row in rows)
            {
                var contract = Contract.GetForexContract(row);
                contracts.Add(contract);
            }

            var filename = string.Format("contracts-{0}.txt", contractType);
            Util.InitFile(filename, Contract.GetHeader());
            Util.WriteToFile<Contract>(filename, contracts);
        }

        private static void GetSingleStockFutures()
        {
            var contractType = "SSFs";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=ssf");         // north american single stock futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_ssf");  // european single stock futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_ssf");    // asian single stock futures

            GetContracts(urls, contractType);
        }

        private static void GetStructuredProducts()
        {
            // no structured product contract id info on IB website

            throw new NotImplementedException();
        }

        private static void GetWarrants()
        {
            // no warrant contract info on IB website
            // Populate List in order to track Tickers that trade Warrants
            var contractType = "WARRANTS";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=war");        // north american warrants (no symbols)
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_war"); // european warrants (no symbols)
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_war");   // asian warrants (no symbols)

            GetContracts(urls, contractType);

            throw new NotImplementedException();
        }

        private static void GetExchangeTradedFunds()
        {
            var contractType = "ETFs";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=etf");         // north american exchange traded funds
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_etf");  // european exchange traded funds
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_etf");    // asian exchange traded funds

            GetContracts(urls, contractType);
        }

        private static void GetOptionsOnFutures()
        {
            var contractType = "FOPs";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=fop");         // north american options on futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_fop");  // european options on futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_fop");    // asian options on futures

            GetContracts(urls, contractType);
        }

        private static void GetFutures()
        {
            var contractType = "FUTURES";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=fut");         // north american futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_fut");  // european futures
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_fut");    // asian futures

            GetContracts(urls, contractType);
        }

        private static void GetOptions()
        {
            var contractType = "OPTIONS";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=opt");         // north american options
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_opt");  // european options
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_opt");    // asian options

            GetContracts(urls, contractType);
        }

        private static void GetStocks()
        {
            var contractType = "STOCK";
            var urls = new List<string>();
            urls.Add("http://www.interactivebrokers.com/en/p.php?f=products");                   // north american stocks
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=europe_stk");  // european stocks
            urls.Add("http://www.interactivebrokers.com/en/index.php?f=products&p=asia_stk");    // asian stocks

            GetContracts(urls, contractType);
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

        private static List<Exchange> GetExchangeList(List<string> urls, string contractType)
        {
            Logger.Instance.WriteInfo("Initializing exchanges.");

            // extract exchanges from urls
            var exchanges = new List<Exchange>();
            foreach(var url in urls)
                exchanges.AddRange(Exchange.GetList(url, contractType));

            return exchanges;
        }
    }
}
