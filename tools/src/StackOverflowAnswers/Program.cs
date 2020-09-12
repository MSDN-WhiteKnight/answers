using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using RuSoLib;

namespace StackOverflowAnswers
{
    class Program
    {
        public static string FixPostBody(string source)
        {
            //insert newlines before codeblocks - fix for CommonMark
            int i = 0;
            string body = source;

            while (true)
            {
                int next = body.IndexOf("<pre><code>", i);
                if (next < 0) break;
                char c = body[next - 3];

                if (next > 5)
                {
                    bool has_newline = false;

                    string sub = body.Substring(next - 4, 4);
                    if (sub == "\r\n\r\n") has_newline = true;

                    sub = body.Substring(next - 2, 2);
                    if (sub == "\n\n") has_newline = true;

                    if (!has_newline)
                    {
                        string s1 = body.Substring(0, next);
                        string s2 = body.Substring(next);
                        body = s1 + Environment.NewLine + s2;
                    }
                }
                i = next + 12;

                if (i >= body.Length) break;
            }

            return body;
        }

        public static void SaveUserAnswers(string site, int userid)
        {
            string datadir = "..\\..\\..\\..\\data\\" + site + "\\";
            string postsdir = Path.Combine(datadir, "posts\\");
            string path;

            if (!Directory.Exists(postsdir)) Directory.CreateDirectory(postsdir);

            SeApiClient client = new SeApiClient(Archive.APIURL, site);
            Dictionary<int, object> answers = client.LoadUserAnswers(userid);

            Console.WriteLine("Saving {0} answers...", answers.Count);

            foreach (int key in answers.Keys)
            {
                path = Path.Combine(postsdir, "A" + key.ToString() + ".md");
                string title = "Answer " + key.ToString();

                //if answer is already saved, use title from existing file

                try
                {
                    if (File.Exists(path))
                    {
                        TextReader rd = new StreamReader(path, Encoding.UTF8);
                        using (rd)
                        {
                            AnswerMarkdown prev = AnswerMarkdown.FromMarkdown(site, rd);

                            if (!String.IsNullOrEmpty(prev.Title)) title = prev.Title;
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error when trying to read existing answer data");
                    Console.WriteLine(ex.ToString());
                }

                TextWriter wr = new StreamWriter(path, false, Encoding.UTF8);
                using (wr)
                {
                    AnswerMarkdown post = AnswerMarkdown.FromJsonData(site, answers[key]);
                    post.Title = title;

                    //insert newlines before code blocks - fix for CommonMark
                    string body = post.Body;
                    post.Body = FixPostBody(body);

                    post.ToMarkdown(wr);
                }
            }
        }

        public static void SaveSingleAnswer(string site, int id)
        {
            string datadir = "..\\..\\..\\..\\data\\" + site + "\\";
            string postsdir = Path.Combine(datadir, "posts\\");
            string path;

            if (!Directory.Exists(postsdir)) Directory.CreateDirectory(postsdir);
            Console.WriteLine("Saving single answer {0} from {1}...", id, site);

            SeApiClient client = new SeApiClient(Archive.APIURL, site);
            string a = client.LoadSingleAnswer(id);

            if (a == null)
            {
                throw new Exception("Failed to load answer " + id.ToString() + " from " + site);
            }

            path = Path.Combine(postsdir, "A" + id.ToString() + ".md");
            string title = "Answer " + id.ToString();

            //if answer is already saved, use title from existing file

            try
            {
                if (File.Exists(path))
                {
                    TextReader rd = new StreamReader(path, Encoding.UTF8);
                    using (rd)
                    {
                        AnswerMarkdown prev = AnswerMarkdown.FromMarkdown(site, rd);

                        if (!String.IsNullOrEmpty(prev.Title)) title = prev.Title;
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error when trying to read existing answer data");
                Console.WriteLine(ex.ToString());
            }

            TextWriter wr = new StreamWriter(path, false, Encoding.UTF8);
            using (wr)
            {
                dynamic data = JSON.Parse(a);
                AnswerMarkdown post = AnswerMarkdown.FromJsonData(site, data);
                post.Title = title;

                //insert newlines before code blocks - fix for CommonMark
                string body = post.Body;
                post.Body = FixPostBody(body);

                post.ToMarkdown(wr);
            }

            Console.WriteLine("Success");
        }

        [STAThread]
        static void Main(string[] args)
        {
            HTML.SiteURL = "https://github.com/MSDN-WhiteKnight/answers/";
            HTML.SiteTitle = "MSDN.WhiteKnight - Stack Overflow answers";
            HTML.LicenseURL = "https://github.com/MSDN-WhiteKnight/answers/blob/master/LICENSE";
            HTML.EnableAttribution = false;

            if (args.Length == 0)
            {
                Archive.Generate("ru.stackoverflow.com", "posts", "Posts");

                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }
            else if (args.Length >= 3 && args[0] == "saveq")
            {
                Archive.SaveQuestion(args[1], Convert.ToInt32(args[2]));
                Console.WriteLine("Done");
            }
            else if (args.Length >= 3 && args[0] == "savea")
            {
                SaveSingleAnswer(args[1], Convert.ToInt32(args[2]));
                Console.WriteLine("Done");
            }
            else if (args.Length >= 3 && args[0] == "saveu")
            {
                SaveUserAnswers(args[1], Convert.ToInt32(args[2]));
                Console.WriteLine("Done");
            }
            else if (args.Length >= 3 && args[0] == "sync")
            {
                Archive.SaveQuestionsForSavedAnswers(args[1], args[2]);
                Console.WriteLine("Done");
            }
            else if (args.Length >= 1 && args[0] == "generate")
            {                
                Archive.Generate("ru.stackoverflow.com", "posts", "Posts");
                Console.WriteLine("Done");
            }            
            else
            {
                Console.WriteLine(" *** Stack Overflow answers static website generator *** ");
                Console.WriteLine(" Usage: ");
                Console.WriteLine("StackOverflowAnswers saveq [site] [question_id] | Save question and its answers");
                Console.WriteLine("StackOverflowAnswers savea [site] [answer_id]   | Save single answer");
                Console.WriteLine("StackOverflowAnswers saveu [site] [user_id]     | Save all answers of user");
                Console.WriteLine("StackOverflowAnswers generate                   | Generate website");
                Console.WriteLine();

                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }
        }
    }
}
