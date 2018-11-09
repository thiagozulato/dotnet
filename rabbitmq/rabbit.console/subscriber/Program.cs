using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbit.subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            Subscribe();
        }
        static void Subscribe()
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

                var queueName = channel.QueueDeclare("logs_frontend", true, false, false, null);
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "log_key");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                ? string.Join(" ", args)
                : "info: Hello World!");
        }
    }
}
