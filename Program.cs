// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.InteropServices;

namespace MarkdownGenerator

{
    class Program
    {
        static void Main(string[] args)

        {

            string SAVE_LOCATION = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/sync-vault/Inbox/";

            // Build frontmatter
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("yyyy-MM-dd");
            string title = getUserInput("Title: ");
            string[] tags = getTags();

            if (tags.Contains<string>("upcoming") && tags.Contains<string>("event"))
            {
                formattedDate = getUserInput("Date of Event (YYYY-MM-DD)");
                Console.Write($"date: {date.ToString()}");
                Console.WriteLine("There's an upcoming event!");
            }

            Dictionary<string, string> additionalProperties = getAdditionalProperties();
            List<string> frontMatter = generateFrontMatter(formattedDate, title, additionalProperties, tags);


            // Build file
            string safeTitle = title.ToLower().Replace('/', '-');
            string fullPath = Path.Join(SAVE_LOCATION, safeTitle);

            using (StreamWriter outputFile = new StreamWriter($"{fullPath}.md"))
            {
                Console.WriteLine($"Saving to {fullPath}.md");
                foreach (string line in frontMatter)
                    outputFile.WriteLine(line);
            }



            int result = execlp($"nvim", "nvim", $"{fullPath}.md", $"+ 52", null);


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

        static string[]? getTags()
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

        static Dictionary<string, string>? getAdditionalProperties()
        {
            Dictionary<string, string>? additionalProperties = new Dictionary<string, string>();
            bool moreKeyValues = true;
            while (moreKeyValues == true)
            {
                string? input = getUserInput("Additional properties? (key=value or blank to finish", true);
                if (String.IsNullOrEmpty(input))
                {
                    return additionalProperties;
                }
                for (int i = 0; i < input.Count(); i++)
                {
                    if (input[i] == '=')
                    {
                        break;
                    }

                }

                {
                    string[] splitKeyValue = input.Split("=");
                    additionalProperties.Add(splitKeyValue[0], splitKeyValue[1]);
                }

            }
            return additionalProperties;
        }
        static void testOutputs(string formattedDate, string title, string[] tags, Dictionary<string, string> additionalProperties)
        {
            Console.WriteLine(formattedDate);
            Console.WriteLine(title);
            foreach (string tag in tags)
            {
                Console.Write($"tag, ");
            }
            foreach (KeyValuePair<string, string> property in additionalProperties)
            {
                Console.WriteLine($"Key: {property.Key}, Value: {property.Value}");
            }

            List<string> frontmatterLines = generateFrontMatter(formattedDate, title, additionalProperties, tags);
            foreach (string line in frontmatterLines)
            {
                Console.WriteLine(line);
            }
        }

        static List<string> generateFrontMatter(string date, string title, Dictionary<string, string> additionalProperties, string[] tags = null)
        {
            List<string> frontMatterLines = new List<string> { "---", $"date: {date}", $"title: {title}" };

            if (tags != null)
            {
                string tagsJoined = string.Join(", ", tags);
                frontMatterLines.Add($"tags: {tagsJoined}");
            }


            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties)
                {
                    frontMatterLines.Add($"{property.Key}: {property.Value}");
                }
            }

            frontMatterLines.Add("---");
            frontMatterLines.Add("");

            return frontMatterLines;


        }

        [DllImport("libc", EntryPoint = "execlp", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int execlp(string program, string arg0, string arg1, string arg2, string? end);

    }
}
