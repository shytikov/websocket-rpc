﻿using SampleBase;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WebSocketRPC;

namespace MultiService
{
    /// <summary>
    /// Numeric service providing operations on numbers.
    /// </summary>
    class NumericService
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
    class TextService
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

    class Program
    {
        //if access denied execute: "netsh http delete urlacl url=http://+:8001/" (delete for 'localhost', add for public address)
        //open Index.html to run the client
        static void Main(string[] args)
        {
            //generate js code
            File.WriteAllText($"./Site/{nameof(NumericService)}.js", RPCJs.GenerateCaller<NumericService>());
            File.WriteAllText($"./Site/{nameof(TextService)}.js", RPCJs.GenerateCaller<TextService>());

            //start server and bind its local and remote APIs
            var cts = new CancellationTokenSource();
            var t = Server.ListenAsync("http://localhost:8001/", cts.Token, (c, wc) => 
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
            });

            Console.Write("{0} ", nameof(MultiService));
            Process.Start(new ProcessStartInfo(Path.GetFullPath("./Site/Index.html")) { UseShellExecute = true });
            AppExit.WaitFor(cts, t);
        }
    }
}
