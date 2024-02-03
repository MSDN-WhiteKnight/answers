using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MsdnReader
{
    class Program
    {
        /// <summary>
        /// Reads JSON dump of MSDN forum messages and writes content to HTML files
        /// </summary>
        static void Main(string[] args)
        {
            string input = @"..\..\..\..\..\..\MSDN\forum.json";
            string outputDir = @"..\..\..\..\..\..\MSDN\";
            FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read);
            JsonDocument doc = JsonDocument.Parse(fs);
            JsonElement root = doc.RootElement;
            JsonElement messages = root.GetProperty("message");
            int n = 0;
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> topics = new Dictionary<string, string>();

            // Read messages
            foreach (JsonElement msg in messages.EnumerateArray())
            {
                n++;

                //read message
                string topic = msg.GetProperty("DiscussionID").GetString();
                DateTime dt = msg.GetProperty("CreatedDate").GetDateTime();
                string body = msg.GetProperty("BodyText").GetString();

                //build HTML
                sb.Clear();
                sb.AppendLine("<h2>Message " + n.ToString() + "</h2>");
                sb.AppendLine("<p>Date: " + dt.ToString() + "</p>");
                sb.AppendLine("<div>");
                sb.AppendLine(body);
                sb.AppendLine("</div>");

                //save HTML
                if (topics.ContainsKey(topic))
                {
                    string html = topics[topic];
                    html = sb.ToString() + html;
                    topics[topic] = html;
                }
                else
                {
                    topics[topic] = sb.ToString();
                }
            }

            // Write topics
            n = 0;

            foreach (string topic in topics.Keys)
            {
                string html = topics[topic];
                string name = topic + ".html";
                string outputFile = Path.Combine(outputDir, name);
                File.WriteAllText(outputFile, html);
                n++;
            }

            Console.WriteLine("Files processed: " + n.ToString());
            Console.ReadLine();
        }
    }
}
