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
using System.Linq;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;
using System;

namespace IbContractExtractor
{
    public class Contract
    {
        public string ContractId { get; set; }
        public string Description { get; set; }
        public string IbSymbol { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public string ExchangeCode { get; set; }
        private static string url;
        private static bool complete;
        private static HtmlDocument doc;

        public Contract()
        { }

        public Contract(HtmlNode link, Exchange exchange)
        {
            ContractId = Regex.Match(link.ChildNodes[3].InnerHtml,"conid=(.*?)'").Groups[1].Value;
            Description = link.ChildNodes[3].InnerText.Replace("&AMP;","&");
            IbSymbol = link.ChildNodes[1].InnerText;
            Symbol = link.ChildNodes[5].InnerText;
            Currency = link.ChildNodes[7].InnerText;
            Category = exchange.Category;
            ExchangeCode = exchange.Code;
        }

        public static string GetHeader()
        {
            return "ContractId|Category|ExchangeCode|Description|IbSymbol|Symbol|Currency";
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", 
                ContractId, Category, ExchangeCode, Description, IbSymbol, Symbol, Currency);
        }

        public static List<Contract> GetList(Exchange exchange)
        {
            List<Contract> contracts = new List<Contract>();
            int index = 1;
            complete = false;

            while (!complete)
            {
                // get the links using the HtmlAgilityPack and Linq. IB splits 100 per page
                var links = GetLinks(exchange, index);

                if (links.Count() == 0)
                {
                    // we're done. set the flag and jump out.
                    complete = true;
                    continue;
                }
                else
                {
                    // parse out information about the exchange from the link.
                    foreach (var link in links)
                        contracts.Add(new Contract(link, exchange));

                    Logger.Instance.WriteInfo(" ... {0}", index);

                    index += 1;
                    if (exchange.Category.Equals("ETF"))
                    {
                        complete = true;
                        // Note, do not loop if ETF as all ETFs are shown on one page
                        continue;
                    }
                }
            }

            return contracts;
        }

        private static IEnumerable<HtmlNode> GetLinks(Exchange exchange, int index)
        {
            url = GetUrl(exchange, index);
            doc = Util.HtmlWebInstance.Load(url);

            // here's the magic. After examining the IB pages, the "juicy bit" is stored
            // on a table row. This Linq query grabs the rows that have a link containing
            // an IB contract id. We need the whole row to get the name, symbol, currency, etc.
            // This linq-ness is enabled by the HtmlAgilityPack. Damn sweet library! -- srlc
            var links = from e in doc.DocumentNode.Descendants("tr")
                        from d in e.Descendants("a")
                        where d.Attributes["href"].Value.Contains("conid")
                        select e;

            return links;
        }

        private static string GetUrl(Exchange exchange, int sequence)
        {
             if (exchange.Category.Equals("ETF"))
            {
                string mainUrl = "http://www.interactivebrokers.com/en/index.php?f=567&exch={0}";
                return string.Format(mainUrl, exchange.Code.ToLower());
            }
            else
            {
                string mainUrl = "https://www.interactivebrokers.com/en/index.php?f=2222&exch={0}&showcategories={1}&p=&cc=&limit=100&page={2}";
                return string.Format(mainUrl, exchange.Code.ToLower(), exchange.Category.ToUpper(), sequence);
            }
        }

        internal static Contract GetForexContract(HtmlNode row)
        {
            var cells = row.Descendants("td").ToList();

            var conId = Regex.Match(cells[1].InnerHtml, "^.*conid=(.*?)'.*$").Groups[1].Value;
            var currency = cells[3].InnerText;
            var description = cells[1].InnerText;
            var exchangeCode = "IDEALFX";
            var ibSymbol = cells[0].InnerText;
            var symbol = cells[2].InnerText;

            var contract = new Contract
            {
                Category = "FOREX",
                ContractId = conId,
                Currency = currency,
                Description = description,
                ExchangeCode = exchangeCode,
                IbSymbol = ibSymbol,
                Symbol = symbol
            };
            return contract;
        }

    }
}
