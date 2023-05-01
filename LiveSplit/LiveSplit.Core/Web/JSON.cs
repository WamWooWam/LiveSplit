using LiveSplit.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LiveSplit.Web
{
    internal static class JSON
    {
        public static dynamic FromResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                return FromStream(stream);
            }
        }

        public static dynamic FromStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = "";
            try
            {
                json = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return FromString(json);
        }

        public static dynamic FromString(string value)
        {
            return JObject.Parse(value);
        }

        [Obsolete("TODO: rewrite this because it's fucking awful")]
        public static dynamic FromUri(Uri uri)
        {
            var request = WebRequest.Create(uri);

            using (var response = request.GetResponse())
            {
                return FromResponse(response);
            }
        }

        public static string Escape(string value)
        {
            return HttpUtility.JavaScriptStringEncode(value);
        }

        [Obsolete("TODO: rewrite this because it's fucking awful")]
        public static dynamic FromUriPost(Uri uri, params string[] postValues)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json";

            var parameters = new StringBuilder();

            parameters.Append("{");

            for (var i = 0; i < postValues.Length; i += 2)
            {
                parameters.AppendFormat("\"{0}\": \"{1}\", ",
                    Escape(postValues[i]),
                    Escape(postValues[i + 1]));
            }

            parameters.Length -= 2;

            parameters.Append("}");

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(parameters.ToString());
            }

            using (var response = request.GetResponse())
            {
                return FromResponse(response);
            }
        }
    }
}
