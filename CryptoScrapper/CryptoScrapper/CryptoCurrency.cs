using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;

public class CryptoCurrency
{
    public string Name { get; set; }
    public string Price { get; set; }
    public string Change { get; set; }

    public CryptoCurrency(string name, string price, string change)
    {
        Name = name;
        Price = price;
        Change = change;
    }
}