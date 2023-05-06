using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
                int next = body.IndexOf("<pre", i);
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

        public static Dictionary<int, QuestionMarkdown> LoadQuestionsSequence(string site,IEnumerable<int> question_ids)
        {
            int[] sequence;

            //load questions
            int[] q_arr = question_ids.ToArray();
            int i1 = 0;
            int i2 = 99;
            if (i2 >= q_arr.Length) i2 = q_arr.Length - 1;

            SeApiClient client = new SeApiClient(Archive.APIURL, site);
            Dictionary<int, QuestionMarkdown> ret = new Dictionary<int, QuestionMarkdown>();

            while (true)
            {
                sequence = new int[i2 - i1 + 1];
                Console.WriteLine("Loading questions from #{0} to #{1}...", i1, i2);
                Array.Copy(q_arr, i1, sequence, 0, sequence.Length);
                Dictionary<int, object> questions = client.LoadQuestionsSequence(sequence);

                Console.WriteLine("{0} questions loaded", questions.Count);

                for (int i = 0; i < sequence.Length; i++)
                {
                    int id = sequence[i];
                    QuestionMarkdown q = QuestionMarkdown.FromJsonData(site, questions[id]);
                    ret[id] = q; 
                }

                i1 = i2 + 1;
                if (i1 >= q_arr.Length) break;
                i2 = i1 + 99;
                if (i2 >= q_arr.Length) i2 = q_arr.Length - 1;
            }

            return ret;
        }

        public static void UpdateTitles(string site, string subdir)
        {
            string datadir = "..\\..\\..\\..\\data\\" + site + "\\";
            string postsdir = Path.Combine(datadir, subdir + "\\");

            Console.WriteLine("Updating titles for saved answers ({0}, {1})...", site, subdir);

            PostSet posts = PostSet.LoadFromDir(postsdir, site);
            Dictionary<int, Question> questions = posts.Questions;

            Console.WriteLine("Answers without parent question: {0}", posts.MarkdownAnswers.Count);

            int n = 0;
            List<int> question_ids = new List<int>(posts.MarkdownAnswers.Count);

            foreach (int a in posts.MarkdownAnswers.Keys)
            {
                try
                {
                    question_ids.Add(posts.MarkdownAnswers[a].QuestionId);
                    n++;
                    //if (n > 70) break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType() + ": " + ex.Message);
                    //System.Threading.Thread.Sleep(20 * 1000);
                }
            }
            
            Dictionary<int,QuestionMarkdown> loaded = LoadQuestionsSequence(site,question_ids);

            foreach (int a in posts.MarkdownAnswers.Keys)
            {
                try
                {
                    int key = posts.MarkdownAnswers[a].QuestionId;

                    if (!loaded.ContainsKey(key))
                    {
                        Console.WriteLine("Not found Q" + key.ToString());
                        continue;
                    }

                    QuestionMarkdown qmd = loaded[key];
                    string newtitle = posts.MarkdownAnswers[a].Title;

                    if (!String.IsNullOrEmpty(qmd.Title))
                    {
                        newtitle = "Ответ на \"" + qmd.Title + "\"";
                    }

                    posts.MarkdownAnswers[a].Title = newtitle;
                    string filepath = Path.Combine(postsdir, "A" + a.ToString() + ".md");
                    TextWriter wr = new StreamWriter(filepath, false, Encoding.UTF8);

                    using (wr)
                    {
                        posts.MarkdownAnswers[a].ToMarkdown(wr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType() + ": " + ex.Message);
                }
            }
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
                UpdateTitles(args[1], args[2]);
                Console.WriteLine("Done");
            }
            else if (args.Length >= 1 && args[0] == "generate")
            {                
                Archive.Generate("ru.stackoverflow.com", "posts", "Posts (RU)");
                Archive.Generate("stackoverflow.com", "posts", "Posts (EN)");
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
