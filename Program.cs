// See https://aka.ms/new-console-template for more information

namespace MarkdownGenerator

{
    class Program
    {
        static void Main(string[] args)
        {
            string title = getUserInput("Title: ");
            string[] tags = getTags();

            for (int i = 0; i < tags.Count(); i++)
            {
                Console.WriteLine(tags[i]);
            }




        }

        static string? getUserInput(string prompt, bool allowEmpty = false)
        {
            bool waitingForUserInput = true;
            while (waitingForUserInput)
            {
                Console.WriteLine($"{prompt}");

                string? userInput = Console.ReadLine();
                if (userInput != null && userInput.Trim() != "")
                {
                    return userInput.Trim();
                }
                else if (allowEmpty == true)
                {
                    return null;
                }

            }
            return null;




        }

        static string[] getTags()
        {
            string? tags = getUserInput("Tags:", true);
            string[] tagList;

            if (tags != null)
            {
                tagList = tags.Split(",");
                for (int i = 0; i < tagList.Count(); i++)
                {
                    if (tagList[i] != "")
                    {
                        tagList[i] = tagList[i].TrimStart().Trim();
                    }
                }
                return tagList;
            }
            return null;
        }
    }
}
