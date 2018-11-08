using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace producer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

            // A Producer for sending messages with null keys and UTF-8 encoded values.
            using (var p = new Producer<string, string>(config))
            {
                string message = string.Empty;
                
                do 
                {
                    Console.WriteLine("Digite uma mensagem para o kafka");
                    message = Console.ReadLine();

                    var messageKafka = new Message<string, string> { Value = message };
                    messageKafka.Key = "cliente-assinatura-123456";
                    messageKafka.Headers = new Headers();
                    messageKafka.Headers.Add("clientId", "181781723".ToByte());

                    try
                    {
                        var dr = await p.ProduceAsync("topic_test", messageKafka);
                        Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                    }
                    catch (KafkaException e)
                    {
                        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                    }
                } while (message != "exit");
            }
        }
    }

    static class StringEnxtensions
    {
        public static byte[] ToByte(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string ToStringJson(this string value)
        {
            var dic = new Dictionary<string, string>();
            var values = value.Split(";");

            foreach (var valuesSequence in values)
            {
                var keys = valuesSequence.Split(":");

                dic.Add(Convert.ToString(keys[0]), Convert.ToString(keys[1]));
            }

            return JsonConvert.SerializeObject(dic);
        }
    }
}
