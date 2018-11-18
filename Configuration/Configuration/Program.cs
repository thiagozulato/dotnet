using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Configuration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) => {
                    config.AddApiKeyVault();
                })
                .UseStartup<Startup>();
    }

    public class ApiConfigurationProvider : ConfigurationProvider
    {
        public override void Load()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            Data.Add("ClientId", "24085747524572047852904752");
            Data.Add("SecretKey", "75uhf7464yfgst53re5eg8f5jf8h5g==");
            Data.Add("IdsUrl", "https://nwidsconfig.com.br/connect/token");
        }
    }

    public class ApiConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ApiConfigurationProvider();
        }
    }

    public static class ApiConfigurationExtensions
    {
        public static IConfigurationBuilder AddApiKeyVault(this IConfigurationBuilder builder)
        {
            builder.Add(new ApiConfigurationSource());

            return builder;
        }
    }
}
