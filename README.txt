IbContractExtractor 

Scrapes contract id's from Interactive Broker's website.  

The program crawls all the links found at:
    http://www.interactivebrokers.com/en/p.php?f=products
		
The program is written in C# and relies on the HtmlAgilityPack for most/all of 
the heavy lifting (http://htmlagilitypack.codeplex.com/). The output file is about 12MB, 
with around 216,000 contracts. There are about 224 exchanges, 2450 pages to download.

The author is Shane Castle (shane.castle@vaultic.com).
	  
Installation
----------------------------

	Installing IbContractExtractor is just a matter of unzipping the download 
	file to wherever you want to install it.
	
Running IBController
----------------------------

	Start IBController via the command line. 
	
	Example:
	c:\unzip path\>IbExtract.exe

	1. Writes out a list of exchanges to:
      ..\output\exchanges.txt

	2.  Writes a list of contract id's and details to:
     ..\output\contracts.txt

	
Building IBController
-----------------------------

	The project is built with VisualStudio 2010. 
	It should compile with command line tools, but I haven't tested that.
	
	A free version of VisualStudio 2010 Express is available here:
	   http://www.microsoft.com/express/Downloads/

	Just open the .sln file and compile as normal.

Notes
-----------------------------
	Moved to GitHub from SourceForge in May 2013.
