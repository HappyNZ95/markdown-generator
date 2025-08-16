// See https://aka.ms/new-console-template for more information
using System;
using System;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Text; //Encoding.UTF8
using System.Text.Json;

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

            bool upcomingEvent = false;


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

            if (tags.Contains<string>("upcoming") && tags.Contains<string>("event"))
            {
                upcomingEvent = true;
                formattedDate = getUserInput("Date of Event (YYYY-MM-DD)");
                Console.Write($"date: {date.ToString()}");

                string additionalLLMContext = getUserInput("Additional context for LLM?", true);


                string prompt = $@"Update the following obsidian note that outlines a plan for an upcoming event. 
		    Ensure you keep the frontmatter untouched and edit afterwards. The highest priority
		    is that the upcoming event is organised with intention - attention to detail is key.
		    Write a plan for things that will be needed, a timeframe of which things should be achieved,
	    what to do on the day, potential pitfalls to look out for, etc. Format the text using some but minimal
		    obsidian markdown i.e not emphasising with bold often, not numbering headings, etc. The human
		    will end up editing your syntactic sugar out.
		    Do not respond as if you are talking to me, just write the note for my reference.
		    Here is some additional context provided by the user: {additionalLLMContext}";

                string fileContent = File.ReadAllText($"{fullPath}.md");
                prompt += $"\n{fileContent}";
                string geminiResponse = queryGeminiModel(prompt).GetAwaiter().GetResult();

                Console.Write(geminiResponse);
                File.WriteAllText($"{fullPath}.md", geminiResponse);
            }



            int executeNeovim = execlp($"nvim", "nvim", $"{fullPath}.md", $"+ {frontMatter.Count + 1}", null);


        }

        static async Task<string> queryGeminiModel(string prompt)
        {
            string GEMINI_API_KEY = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            Console.Write($"Api Key: {GEMINI_API_KEY}");
            string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent";
            string jsonPayload = $@"
{{
    ""contents"": [
        {{
            ""parts"": [
                {{
                    ""text"": ""{prompt}""
                }}
            ]
        }}
    ],
    ""generationConfig"": {{
        ""thinkingConfig"": {{
            ""thinkingBudget"": 128
        }}
    }}
}}";


            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-goog-api-key", GEMINI_API_KEY);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}\n{errorBody}");
            }


            string jsonString = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(jsonString);

            string text = doc.RootElement
        .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

            return text;

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
