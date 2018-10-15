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

using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace WebSocketRPC
{
    /// <summary>
    /// Contains functions for creating RPC JavaScript API from a provided class/interface.
    /// </summary>
    public static class RPCJs
    {
        /// <summary>
        /// Generates JavaScript code from the provided class or interface type.
        /// </summary>
        /// <typeparam name="T">Class or interface type.</typeparam>
        /// <param name="settings">RPC-Js settings used for Javascript code generation.</param>
        /// <returns>Javascript API.</returns>
        public static string GenerateCaller<T>(RPCJsSettings<T> settings = null)
        {
            settings = settings ?? new RPCJsSettings<T>();
            var (tName, mInfos) = JsCallerGenerator.GetMethods(settings.OmittedMethods);
            tName = settings.NameOverwrite ?? tName;

            var xmlMemberNodes = default(XmlNodeList);

            if (settings.Documentation)
            {
                xmlMemberNodes = JsDocGenerator.GetMemberNodes(Path.ChangeExtension(typeof(T).Assembly.Location, ".xml"));
            }

            var sb = new StringBuilder();

            switch(settings.Format)
            {
                case RPCJsSettings<T>.Module.RequireJS:
                    sb.Append(JsCallerGenerator.GenerateRequireJsHeader(tName));
                    break;
                case RPCJsSettings<T>.Module.CommonJS:
                    sb.Append(JsCallerGenerator.GenerateCommonJsHeader(tName));
                    break;
            }

            if (settings.Documentation)
            {
                sb.Append(JsDocGenerator.GetClassDoc(xmlMemberNodes, tName));
            }

            sb.Append(JsCallerGenerator.GenerateHeader(tName));

            foreach (var m in mInfos)
            {
                var mName = m.Name;
                var pNames = m.GetParameters().Select(x => x.Name).ToArray();

                if (settings.Documentation)
                {
                    var pTypes = m.GetParameters().Select(x => x.ParameterType).ToArray();
                    sb.Append(JsDocGenerator.GetMethodDoc(xmlMemberNodes, mName, pNames, pTypes, m.ReturnType));
                }

                sb.Append(JsCallerGenerator.GenerateMethod(mName, pNames));
            }

            sb.Append(JsCallerGenerator.GenerateFooter());
            return sb.ToString();
        }
    }
}
