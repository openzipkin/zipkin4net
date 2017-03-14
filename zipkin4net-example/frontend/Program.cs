using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseSetting("urls", args[0])
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
