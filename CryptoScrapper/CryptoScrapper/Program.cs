using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;


class Program
{
    static async Task Main(string[] args)
    {
        
        // Realiza una llamada inicial a getCryptoData
        await getCryptoData(false);

        //Instanciamos un objeto de System.Timers para que ejecute el mismo proceso cada 1 minuto
        // Configura el temporizador para ejecutar FCryptoData cada 60 segundos
        var timer = new System.Timers.Timer();
        timer.Interval = 10000;
        timer.Elapsed += async (sender, e) =>
        {
            await getCryptoData(true);
        };
        timer.Start();

        Console.ReadLine();
    }

    static async Task getCryptoData(bool NewCall)
    {
        var url = "https://finance.yahoo.com/crypto/?offset=0&count=200";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);
        //En html está guardado el codigo completo tras la petición

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var cryptoTable = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='W(100%)']");

        var cryptoRows = cryptoTable.Descendants("tr");

        bool firstChangeFound = false;

        List<CryptoCurrency> cryptoCurrencies = new List<CryptoCurrency>();

        foreach (var row in cryptoRows)
        {
            var cells = row.Descendants("td");

            string cryptoName = "";
            string cryptoPrice = "";
            string cryptoChange = "";

            foreach (var cell in cells)
            {
                var anchorNode = cell.Descendants("a").FirstOrDefault();
                if (anchorNode != null && anchorNode.Attributes["title"] != null)
                {
                    cryptoName = anchorNode.Attributes["title"].Value;
                    //Console.WriteLine(cryptoName);
                }

                var priceNode = cell.Descendants("fin-streamer").FirstOrDefault();
                if (priceNode != null && priceNode.Attributes["data-field"] != null && priceNode.Attributes["data-field"].Value == "regularMarketPrice")
                {
                    cryptoPrice = priceNode.Attributes["value"].Value;
                    //Console.WriteLine(cryptoPrice);
                }

                var changeNode = cell.Descendants("span").FirstOrDefault(x => x.Attributes["class"]?.Value.Contains("C($") == true);
                if (changeNode != null)
                {
                    if (!firstChangeFound)
                    {
                        //A este punto nos encontraremos con dos tipos de cambios, uno que será de tipo decimal o otro de porcentaje
                        //Como solo nos interesa el porcentaje, saltamos esa primera vez que lo hallamos, y ya tendremos la columna de %
                        firstChangeFound = true;
                        continue; // saltar a la siguiente iteración del bucle
                    }

                    cryptoChange = changeNode.InnerText;
                    //Console.WriteLine(cryptoChange);

                    //Ahora que ya tenemos todo lo que necesitamos de la criptomoneda, creamos un objeto para luego compararlos
                    var cryptoCurrency = new CryptoCurrency(cryptoName, cryptoPrice, cryptoChange);
                    cryptoCurrencies.Add(cryptoCurrency);
                    firstChangeFound = false;
                }
            }
        }
        
        //Creamos una nueva lista con las 10 criptomonedas con mayor % de change                                                       
        var top10Currencies = cryptoCurrencies.OrderByDescending(x => x.Change).Take(10);//con OrderByDescending ordenamos de manera descente en base al change %
        Console.WriteLine("");
        Console.WriteLine("Nueva actualización!");
        Console.WriteLine("TOP 10 CRIPTOMENDAS CON MEJOR PORCENTAJE DE CAMBIO:");
        foreach (var currency in top10Currencies)
        {
            Console.WriteLine(currency.Name + " - " + "Cambio: " + currency.Change);
        }
        /*
        if (NewCall)
        {
            Console.WriteLine("");
            Console.WriteLine("Nueva actualización!");

            
            foreach (var currency in top10Currencies)
            {
                if (currency.Change != 0)
                {
                    Console.WriteLine($"{currency.Name} ha cambiado:");
                    Console.WriteLine($"Charge: {currency.Change}");
                    //Console.WriteLine($"% Change: {currency.PercentChange}");
                    Console.WriteLine("===============================");
                }
            }
            
    }
        */
        //Utilizamos la lista de las que ya tienen el mejor cambio, y buscamos cuales son las mas baratas
        var best5Currencies = top10Currencies.OrderByDescending(x => x.Price).Take(5);
        Console.WriteLine("\nTOP 5 CRIPTOMENDAS CON MEJOR RENDIMIENTO:");
        foreach (var currency in best5Currencies)
        {
            Console.WriteLine(currency.Name + " - " + "Precio " + currency.Price);
        }

        Console.WriteLine($"=== Informacion suministrada desde las {DateTime.Now} ===");
        
    }

}
