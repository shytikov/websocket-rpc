using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketRPC;

namespace Tests
{
    partial class Program
    {
        static Task[] TestMultiClient(CancellationTokenSource cts)
        {
            var tasks = new List<Task>();

            var ts = Server.ListenAsync($"http://{address}", cts.Token, (c, wc) => 
            {
                c.OnReceive += msg => Task.Run(() => Console.WriteLine("Received: " + msg));
            });
            tasks.Add(ts);

            var id = 0;
            for (var i = 0; i < 10; i++)
            {
                var tc = Client.ConnectAsync($"ws://{address}", cts.Token, c =>
                {
                    Interlocked.Increment(ref id);

                    c.OnOpen += async () =>
                    {
                        var n = 10;
                        while (n > 0)
                        {
                            await c.SendAsync("Hello from: " + id);
                            await Task.Delay(10);
                            n--;
                        }
                    };

                    c.OnError += e => Task.Run(() => Console.WriteLine("Error: " + e.Message));
                    c.OnClose += (s, d) => Task.Run(() => Console.WriteLine("Close: " + s));
                },
                reconnectOnError: false);

                tasks.Add(tc);
            }
           

            return tasks.ToArray();
        }
    }
}
