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

using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace IbContractExtractor
{
    public class Exchange
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Link { get; set; }
        private static HtmlDocument doc;

        public Exchange(HtmlNode source)
        {
            Link = string.Format("{0}{1}", "http://www.interactivebrokers.com/en/", source.Attributes["href"].Value);
            Name = source.InnerText;
            Category = Regex.Match(Link, "showcategories=(.*)&").Groups[1].Value;
            Code = Regex.Match(Link, "exch=(.*?)&").Groups[1].Value.ToUpper();
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}", Category, Code, Name, Link);
        }

        public static List<Exchange> GetList(string source, string contractType)
        {
            // grab all the links from the source, using HtmlAgilityPack and Linq
            doc = Util.HtmlWebInstance.Load(source);

            var links = GetLinks();

            // parse out information about the exchange from the link.
            List<Exchange> exchanges = new List<Exchange>();
            foreach (var link in links)
                exchanges.Add(new Exchange(link));

            return exchanges;
        }

        private static IEnumerable<HtmlNode> GetLinks()
        {
            // use Linq on the document to pull out all the html links with "trading" in the name
            var links = doc.DocumentNode.Descendants("a")
                            .Where(e => e.Attributes.Contains("href") && e.Attributes["href"].Value.StartsWith("trading"));

            return links;
        }

        internal static string GetHeader()
        {
            return "Category|ExchangeCode|ExchangeName|ExchangeLink";
        }
    }
}