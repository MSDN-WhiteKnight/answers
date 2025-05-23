﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MsdnReader
{
    class WebPage : IComparable<WebPage>
    {
        public string Title { get; set; }
        public string Name { get; set; }

        public int CompareTo(WebPage other)
        {
            return string.Compare(this.Title, other.Title);
        }
    }

    class Program
    {
        const string AuthorName = "VadimTagil";
        const string AuthorHomepage = "https://smallsoftru.wordpress.com";

        static void CreateIndex(string dir)
        {
            string[] files = Directory.GetFiles(dir, "*.html");
            List<WebPage> pages = new List<WebPage>(files.Length);
            string outdir = Path.Combine(dir, "Output");
            Directory.CreateDirectory(outdir);

            // Read input files
            for (int i = 0; i < files.Length; i++)
            {
                // Read page name and title
                string name = Path.GetFileName(files[i]);
                string content = File.ReadAllText(files[i]);
                string title = Path.GetFileNameWithoutExtension(files[i]);

                int startIndex = content.IndexOf("<h2>");
                int endIndex = content.IndexOf("</h2>");

                if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                {
                    startIndex = startIndex + 4;
                    title = content.Substring(startIndex, endIndex - startIndex);
                }

                Console.WriteLine(name + " " + title);
                WebPage page = new WebPage();
                page.Title = title;
                page.Name = name;

                if (name == "index.html") continue;

                pages.Add(page);

                // Copy resulting file into output directory
                StringBuilder sb = new StringBuilder((int)(content.Length * 1.5));
                sb.Append("<html><head><title>");
                sb.Append(title);
                sb.AppendLine("</title></head><body>");
                sb.AppendLine("<p><a href=\"index.html\">Ответы с форумов MSDN</a></p>");
                sb.AppendLine(content);
                sb.Append("<hr/>");
                sb.AppendLine("<p>Автор: <a href=\"" + AuthorHomepage + "\">" + AuthorName + "</a></p>");
                sb.Append("<p><a href=\"https://msdn-whiteknight.github.io/answers/html/\">Главная страница</a> - ");
                sb.Append("<a href=\"index.html\">Список тем</a> - ");
                sb.AppendLine("<a href=\"https://github.com/MSDN-WhiteKnight/answers\">Репозиторий на GitHub</a></p>");
                sb.AppendLine("</body></html>");

                string targetName = Path.Combine(outdir, name);
                File.WriteAllText(targetName, sb.ToString());
            }

            // Build index
            Console.WriteLine("Pages: " + pages.Count);
            pages.Sort();
            string indexPath = Path.Combine(outdir, "index.html");
            StreamWriter wr = new StreamWriter(indexPath);
            
            using (wr)
            {
                wr.WriteLine("<html><head><title>Ответы с форумов MSDN</title></head><body>");
                wr.WriteLine("<h1>Ответы с форумов MSDN</h1>");
                wr.WriteLine("<p>Данный раздел содержит мои ответы с форумов MSDN (пользователь VadimTagil), ");
                wr.WriteLine("написанные в период с 2014 по 2022 год. В январе 2024 года форумы были закрыты, ");
                wr.WriteLine("поэтому я публикую архив на своем сайте. Ответы в основном по программированию ");
                wr.WriteLine("на С++ и .NET, администрированию Windows и СУБД Microsoft SQL Server.");
                wr.WriteLine("</p>");
                wr.WriteLine("<div>");

                for (int i = 0; i < pages.Count; i++)
                {
                    wr.Write("<a href=\"" + pages[i].Name + "\">" + pages[i].Title);
                    wr.WriteLine("</a><br/>");
                }

                wr.WriteLine("</div><hr/>");
                wr.WriteLine("<p>Автор: <a href=\"" + AuthorHomepage + "\">" + AuthorName + "</a></p>");
                wr.WriteLine("<p><a href=\"https://msdn-whiteknight.github.io/answers/html/\">Главная страница</a> - ");
                wr.WriteLine("<a href=\"https://github.com/MSDN-WhiteKnight/answers\">Репозиторий на GitHub</a></p>");
                wr.WriteLine("</body></html>");
            }

            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Reads JSON dump of MSDN forum messages and writes content to HTML files
        /// </summary>
        static void ExtractMessages()
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
        }
        
        static void Main()
        {
            ExtractMessages();
            Console.ReadLine();
        }
    }
}
