// See https://aka.ms/new-console-template for more information

namespace MarkdownGenerator

{
    class Program
    {
        static void Main(string[] args)

        {
            string title = getUserInput("Title: ");
            string[] tags = getTags();
            Dictionary<string, string> additionalProperties = getAdditionalProperties();

            foreach (KeyValuePair<string, string> property in additionalProperties)
            {
                Console.WriteLine($"Key: {property.Key}, Value: {property.Value}");
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

        static Dictionary<string, string> getAdditionalProperties()
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
    }
}
