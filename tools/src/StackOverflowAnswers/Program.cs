using System;
using System.Collections.Generic;
using System.Text;
using RuSoLib;

namespace StackOverflowAnswers
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            HTML.SiteURL = "https://github.com/MSDN-WhiteKnight/answers/";
            HTML.SiteTitle = "Stack Overflow answers";
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
                Archive.SaveSingleAnswer(args[1], Convert.ToInt32(args[2]));
                Console.WriteLine("Done");
            }
            else if (args.Length >= 3 && args[0] == "saveu")
            {
                Archive.SaveUserAnswers(args[1], Convert.ToInt32(args[2]));
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
