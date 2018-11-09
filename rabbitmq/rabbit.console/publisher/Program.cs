using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbit.publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Publish(args);
        }

        static void Publish(string[] args)
        {
            var factory = new ConnectionFactory() 
            { 
                HostName = "localhost",
                UserName = "admin",
                Password = "@bc123"
            };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Direct);

                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                    routingKey: "log_key",
                                    basicProperties: null,
                                    body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                ? string.Join(" ", args)
                : "info: Hello World!");
        }
    }
}
