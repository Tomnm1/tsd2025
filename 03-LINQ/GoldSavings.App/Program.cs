using System.Xml.Serialization;
using GoldSavings.App.Model;
using GoldSavings.App.Client;
namespace GoldSavings.App;

class Program
{
    private static void Display(string name, IEnumerable<GoldPrice> prices)
    {
        Console.WriteLine(name);
        foreach(var price in prices)
        {
            Console.Write($"The price for {price.Date} is {price.Price}\n");
        }
        
    }
    private static void A(List<GoldPrice> thisYearPrices)
    {
        var top3HighestMethod = thisYearPrices.OrderByDescending(p => p.Price).Take(3);
        Display("top3HighestMethod", top3HighestMethod);
        
        var top3LowestMethod = thisYearPrices.OrderBy(p => p.Price).Take(3);
        Display("top3LowestMethod", top3LowestMethod);
        
        var top3HighestQuery = (from p in thisYearPrices
            orderby p.Price descending
            select p).Take(3);
        Display("top3HighestQuery", top3HighestQuery);


        var top3LowestQuery = (from p in thisYearPrices
            orderby p.Price ascending
            select p).Take(3);
        Display("top3LowestQuery", top3LowestQuery);
    }

    private static void E()
    {
        GoldClient goldClient = new GoldClient();
        DateTime start = new DateTime(2020, 01, 01);
        DateTime end = new DateTime(2024, 12, 31);

        List<GoldPrice> prices2020to2024 = new List<GoldPrice>();

        DateTime currentStart = start;
        while (currentStart <= end)
        {
            DateTime currentEnd = currentStart.AddDays(29);
            if (currentEnd > end)
                currentEnd = end;

            var monthlyPrices = goldClient.GetGoldPrices(currentStart, currentEnd).GetAwaiter().GetResult();
            prices2020to2024.AddRange(monthlyPrices);

            currentStart = currentEnd.AddDays(1);
        }

        GoldPrice bestBuy = null;
        GoldPrice bestSell = null;
        decimal maxROI = -1m;

        for (int i = 0; i < prices2020to2024.Count; i++)
        {
            for (int j = i + 1; j < prices2020to2024.Count; j++)
            {
                decimal buyPrice = (decimal)prices2020to2024[i].Price;
                decimal sellPrice = (decimal)prices2020to2024[j].Price;

                decimal currentROI = (sellPrice - buyPrice) / buyPrice;

                if (currentROI > maxROI)
                {
                    maxROI = currentROI;
                    bestBuy = prices2020to2024[i];
                    bestSell = prices2020to2024[j];
                }
            }
        }

        if (bestBuy != null && bestSell != null && maxROI > 0)
        {
            Console.WriteLine($"Best date to BUY: {bestBuy.Date:dd.MM.yyyy} at price {bestBuy.Price} PLN");
            Console.WriteLine($"Best date to SELL: {bestSell.Date:dd.MM.yyyy}, Price: {bestSell.Price} PLN");
            Console.WriteLine($"Return on Investment: {maxROI * 100:F2}%");
        }
        else
        {
            Console.WriteLine("Nope.");
        }
    }
    
    public static void SavePricesToXml(List<GoldPrice> prices, string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<GoldPrice>));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, prices);
        }

        Console.WriteLine("Done");
    }
    
    public static List<GoldPrice> LoadPricesFromXml(string filePath) =>
        (List<GoldPrice>)new XmlSerializer(typeof(List<GoldPrice>)).Deserialize(File.OpenRead(filePath));


    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Gold Saver!");

        GoldClient goldClient = new GoldClient();

        GoldPrice currentPrice = goldClient.GetCurrentGoldPrice().GetAwaiter().GetResult();
        Console.WriteLine($"The price for today is {currentPrice.Price}");

        List<GoldPrice> thisMonthPrices = goldClient.GetGoldPrices(new DateTime(2024, 03, 01), new DateTime(2024, 03, 11)).GetAwaiter().GetResult();
        foreach(var goldPrice in thisMonthPrices)
        {
            Console.WriteLine($"The price for {goldPrice.Date} is {goldPrice.Price}");
        }
        List<GoldPrice> thisYearPrices = goldClient.GetGoldPrices(new DateTime(2025, 01, 01), new DateTime(2025, 03, 01)).GetAwaiter().GetResult();

        A(thisYearPrices);
        
        // B
        List<GoldPrice> january2020Prices = goldClient
            .GetGoldPrices(new DateTime(2020, 01, 01), new DateTime(2020, 01, 31))
            .GetAwaiter().GetResult();

        decimal minJanuary2020Price = january2020Prices.Min(p => (decimal)p.Price);

        List<GoldPrice> pricesSinceFebruary2020 = new List<GoldPrice>();

        DateTime start = new DateTime(2020, 02, 01);
        DateTime end = DateTime.Today;

        while (start <= end)
        {
            var chunkEnd = start.AddMonths(1).AddDays(-1);
            if (chunkEnd > end)
                chunkEnd = end;

            var monthlyPrices = goldClient.GetGoldPrices(start, chunkEnd).GetAwaiter().GetResult();
            pricesSinceFebruary2020.AddRange(monthlyPrices);

            start = chunkEnd.AddDays(1);
        }

        var profitableDays = pricesSinceFebruary2020
            .Where(p => (((decimal)p.Price - minJanuary2020Price) / minJanuary2020Price) > 0.05m)
            .ToList();

        Console.WriteLine("\nWhen 5% or more:");
        foreach (var price in profitableDays)
        {
            decimal roi = (((decimal)price.Price - minJanuary2020Price) / minJanuary2020Price) * 100m;
            Console.WriteLine($"Date: {price.Date}, Price: {price.Price}, ROI: {roi:F2}%");
        }
        // C
        var prices2019to2022 = new List<GoldPrice>();

        DateTime startDate = new DateTime(2019, 01, 01);
        DateTime endDate = new DateTime(2022, 12, 31);

        while (startDate <= endDate)
        {
            var chunkEnd = startDate.AddDays(29);
            if (chunkEnd > endDate)
                chunkEnd = endDate;

            var monthlyPrices = goldClient.GetGoldPrices(startDate, chunkEnd).GetAwaiter().GetResult();
            prices2019to2022.AddRange(monthlyPrices);

            startDate = chunkEnd.AddDays(1);
        }

        var secondTenDates = prices2019to2022
            .OrderByDescending(p => p.Price)
            .Skip(10)
            .Take(3)
            .Select(p => p.Date)
            .ToList();

        Console.WriteLine("\nPlaces 11, 12, 13");
        foreach (var date in secondTenDates)
        {
            Console.WriteLine($"{date}");
        }
        
        //D
        var prices2020 = new List<GoldPrice>();

        DateTime start2020 = new DateTime(2020, 01, 01);
        DateTime stop2020 = new DateTime(2020, 12, 31);

        while (start2020 <= stop2020)
        {
            var chunkEnd = start2020.AddDays(29);
            if (chunkEnd > stop2020)
                chunkEnd = stop2020;

            var monthlyPrices = goldClient.GetGoldPrices(start2020, chunkEnd).GetAwaiter().GetResult();
            prices2020.AddRange(monthlyPrices);

            start2020 = chunkEnd.AddDays(1);
        }
        var av2020 = (from price in prices2020
            select price.Price).Average();
        Console.WriteLine($"The price for 2020 is {av2020:F2}");
        
        var prices2023 = new List<GoldPrice>();

        DateTime start2023 = new DateTime(2023, 01, 01);
        DateTime stop2023 = new DateTime(2023, 12, 31);

        while (start2023 <= stop2023)
        {
            var chunkEnd = start2023.AddDays(29);
            if (chunkEnd > stop2023)
                chunkEnd = stop2023;

            var monthlyPrices = goldClient.GetGoldPrices(start2023, chunkEnd).GetAwaiter().GetResult();
            prices2023.AddRange(monthlyPrices);

            start2023 = chunkEnd.AddDays(1);
        }
        var av2023 = (from price in prices2023
            select price.Price).Average();
        Console.WriteLine($"The price for 2023 is {av2023:F2}");
        
        var prices2024 = new List<GoldPrice>();

        DateTime start2024 = new DateTime(2024, 01, 01);
        DateTime stop2024 = new DateTime(2024, 12, 31);

        while (start2024 <= stop2024)
        {
            var chunkEnd = start2024.AddDays(29);
            if (chunkEnd > stop2024)
                chunkEnd = stop2024;

            var monthlyPrices = goldClient.GetGoldPrices(start2024, chunkEnd).GetAwaiter().GetResult();
            prices2024.AddRange(monthlyPrices);

            start2024 = chunkEnd.AddDays(1);
        }
        
        var av2024 = (from price in prices2024
            select price.Price).Average();
        Console.WriteLine($"The price for 2024 is {av2024:F2}");
        
        //E
        E();
        
        //xml
        
        List<GoldPrice> goldPricesVol4 = goldClient.GetGoldPrices(
            new DateTime(2024, 01, 01),
            new DateTime(2024, 03, 17)).GetAwaiter().GetResult();

        SavePricesToXml(goldPricesVol4, "GoldPrices.xml");
        
        
        string path = "GoldPrices.xml";
        List<GoldPrice> loadedPrices = LoadPricesFromXml(path);

        Console.WriteLine($"Loaded {loadedPrices.Count} prices from XML.");
        foreach(var price in loadedPrices)
            Console.WriteLine($"{price.Date:dd.MM.yyyy}: {price.Price}");
    }
}
