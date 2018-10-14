﻿#region License
// Copyright © 2018 Darko Jurić
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebSocketRPC
{
    abstract class Binder : IBinder
    {
        public Connection Connection { get; private set; }

        protected Binder(Connection connection)
        {
            Connection = connection;
            lock(RPC.AllBinders)
            {
                RPC.AllBinders.Add(this);
            }

            Connection.OnClose += (s, d) =>
            {
                Debug.WriteLine("Close");

                lock (RPC.AllBinders)
                {
                    RPC.AllBinders.Remove(this);
                }

                return Task.FromResult(true);
            };

            Connection.OnError += e =>
            {
                Debug.WriteLine("Error");

                lock (RPC.AllBinders)
                {
                    RPC.AllBinders.Remove(this);
                }

                return Task.FromResult(true);
            };
        }
    }
}
