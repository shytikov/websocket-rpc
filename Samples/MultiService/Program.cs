﻿using System;
using System.IO;
using System.Threading;
using WebsocketRPC;

namespace ClientJsMultiService
{
    /// <summary>
    /// Numeric service providing operations on numbers.
    /// </summary>
    public class NumericService
    {
        /// <summary>
        /// Adds two numbers.
        /// </summary>
        /// <param name="a">First number.</param>
        /// <param name="b">Second number.</param>
        /// <returns>Result.</returns>
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    /// <summary>
    /// Text service providing operations on strings.
    /// </summary>
    public class TextService
    {
        /// <summary>
        /// Concatenates two strings.
        /// </summary>
        /// <param name="a">First string.</param>
        /// <param name="b">Second string.</param>
        /// <returns>Concatenated string.</returns>
        public string Add(string a, string b)
        {
            return a + b;
        }
    }

    public class Program
    {
        //if access denied execute: "netsh http delete urlacl url=http://+:8001/"
        static void Main(string[] args)
        {
            //generate js code
            File.WriteAllText($"../Site/{nameof(NumericService)}.js", RPCJs.GenerateCallerWithDoc<NumericService>());
            File.WriteAllText($"../Site/{nameof(TextService)}.js", RPCJs.GenerateCallerWithDoc<TextService>());

            //start server and bind its local and remote APIs
            var cts = new CancellationTokenSource();
            Server.ListenAsync("http://localhost:8001/", cts.Token, (c, wc) => 
            {
                var path = wc.RequestUri.AbsolutePath;
                if (path == "/numericService")
                {
                    c.Bind(new NumericService());
                }
                else if (path == "/textService")
                {
                    c.Bind(new TextService());
                }
            })
            .Wait(0);

            Console.Write("Running: '{0}'. Press [Enter] to exit.", nameof(ClientJsMultiService));
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
