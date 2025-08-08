// See https://aka.ms/new-console-template for more information

namespace MarkdownGenerator

{
    class Program
    {
        static void Main(string[] args)
        {
            string title = getUserInput("Title: ");
            string tags = getUserInput("Tags: ", true);
            List<String> tagList = new List<String>();
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
    }
}
