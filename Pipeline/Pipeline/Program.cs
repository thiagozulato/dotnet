using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Dotnet
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Pipeline pipeline = new Pipeline();

            pipeline.Register(new OtherEvent());
            pipeline.Register(new FirstEvent());

            try
            {
                await pipeline.ExecuteAsync();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);              
            }
        }
    }

    public class FirstEvent : IPipelineEvent
    {
        public int Order => 0;

        public AsyncEventHandler OnBefore => null;

        public AsyncEventHandler OnAfter => null;

        public Task HandleAsync()
        {
            throw new Exception("Failure test for the FirstEvent class");

            return Task.CompletedTask;
        }
    }

    public class OtherEvent : IPipelineEvent
    {
        public AsyncEventHandler OnBefore => async (o, ea) => {
            await Task.Delay(1000);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i.ToString());
                Thread.Sleep(1000);
            }

            Console.WriteLine("OtherEvent -> OnBefore Event");
        };

        public AsyncEventHandler OnAfter => async (o, ea) => {
            await Task.Delay(1000);

            Console.WriteLine("OtherEvent -> OnAfter Event");
        };

        public int Order => 1;

        public Task HandleAsync()
        {
            Console.WriteLine("OtherEvent has been executed successfully.");

            return Task.CompletedTask;
        }
    }

    public class Pipeline
    {
        public Dictionary<string, IPipelineEvent> Events { get; } = 
            new Dictionary<string, IPipelineEvent>();

        public async Task ExecuteAsync()
        {
            if (Events.Count == 0) return; 

            IReadOnlyCollection<IPipelineEvent> events = GetEventsOrdered();

            foreach (var pipelineEvent in events)
            {
                try
                {
                    if (pipelineEvent.OnBefore != null ) 
                        await pipelineEvent.OnBefore(pipelineEvent, new EventArgs());
                    
                    await pipelineEvent.HandleAsync();
                    
                    if (pipelineEvent.OnAfter != null ) 
                        await pipelineEvent.OnAfter(pipelineEvent, new EventArgs());
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Pipeline failed: {ex.Message}");
                }
            }
        }

        internal IReadOnlyCollection<IPipelineEvent> GetEventsOrdered()
        {
            IReadOnlyCollection<IPipelineEvent> readonlyList = Events.Values.OrderBy(p => p.Order).ToList();

            return readonlyList;
        }
    }

    public static class PipelineExtensions
    {
        public static void Register(this Pipeline pipeline, IPipelineEvent @event)
        {
            string eventName = @event.GetType().Name;
            if (pipeline.Events.ContainsKey(eventName)) return;

            pipeline.Events.Add(eventName, @event);
        }
    }

    public interface IPipelineEvent
    {
        int Order { get; }
        AsyncEventHandler OnBefore { get; }
        AsyncEventHandler OnAfter { get; }
        Task HandleAsync();
    }

    public delegate Task AsyncEventHandler(object sender, EventArgs args);
}
