using DotnetPrompt.Chains;
using DotnetPrompt.LLM.CohereAI;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;
using System.ComponentModel;

namespace DotnetPrompt.Tests.Examples.Chains;

#region Example_ObjectExtension
public static class ObjectExtensions
{
    // helper
    public static IDictionary<string, string> ToDictionary(this object obj)
    {
        var result = new Dictionary<string, string>();
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
        foreach (PropertyDescriptor property in properties)
        {
            result.Add(property.Name, property.GetValue(obj).ToString());
        }
        return result;
    }
}
#endregion


public class FewShotModelExamples
{
    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_SentimentAnalysis()
    {
        #region Example_ModelChainFewShotPromptTemplate_SentimentAnalysis
        var example = new PromptTemplate("Message: {message}\nSentiment: {sentiment}");
        var suffix = new PromptTemplate("Message: {message}\nSentiment: ");

        var examples = new List<IDictionary<string, string>>()
        {
            new Dictionary<string, string>() { { "message", "Support has been terrible for 2 weeks..." }, {"sentiment", "Negative" } },
            new Dictionary<string, string>() { { "message", "I love your framework, it is simple and so fast!" }, {"sentiment", "Positive" } },
            new Dictionary<string, string>() { { "message", "ChatGPT has been released 10 months ago." }, {"sentiment", "Neutral" } },
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f });
        //var llm = new CohereAIModel(Constants.CohereAIKey, CohereAIModelConfiguration.Default with { Temperature = 0, MaxTokens = 5, StopSequences = new[] {"\n\n"}});
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("The reactivity of your team was very good, thanks!");
        Console.WriteLine($"> {answer1}");

        var answer2 = await executor.PromptAsync("I hate you work, it's sloppy and lazy!");
        Console.WriteLine($"> {answer2}");

        var answer3 = await executor.PromptAsync("Today is a monday");
        Console.WriteLine($"> {answer3}");
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_HTMLGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_HTMLGeneration
        var example = new PromptTemplate("Description: {description}\nCode: {code}");
        var suffix = new PromptTemplate("Description: {message}:\nCode: ");

        var examples = new List<IDictionary<string, string>>()
        {
            new Dictionary<string, string>() { { "description", "a red button that says stop" }, {"code", "<button style = color:white; background-color:red;>Stop</button>" } },
            new Dictionary<string, string>() { { "description", "a blue box that contains yellow circles with red borders" }, { "code", "<div style = background - color: blue; padding: 20px;><div style = background - color: yellow; border: 5px solid red; border-radius: 50%; padding: 20px; width: 100px; height: 100px;>" } },
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("a Headline saying Welcome to AI");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("a list of 5 countries in yellow on black background");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_SQLGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_SQLGeneration
        var example = new PromptTemplate("Description: {Question}\nCode: {Answer}");
        var suffix = new PromptTemplate("Description: {Question}\nCode: ");

        var examples = new List<object>()
        {
            new { Question = "Fetch the companies that have less than five people in it.", Answer = "SELECT COMPANY, COUNT(EMPLOYEE_ID) FROM Employee GROUP BY COMPANY HAVING COUNT(EMPLOYEE_ID) < 5;" },
            new { Question = "Show all companies along with the number of employees in each department", Answer = "SELECT COMPANY, COUNT(COMPANY) FROM Employee GROUP BY COMPANY;" },
            new { Question = "Show the last record of the Employee table", Answer = "SELECT* FROM Employee ORDER BY LAST_NAME DESC LIMIT 1;"}
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples.Select(i => i.ToDictionary()).ToList());

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Fetch three employees from the Employee table");
        Console.WriteLine($"> {answer1}");
        var answer2 = await executor.PromptAsync("Join employes from the Employee on their company name from Company table");
        Console.WriteLine($"> {answer2}");
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_NERGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_NERGeneration
        var example = new PromptTemplate("[Text]: {text}\n" +
                                         "[Name]: {name}\n" +
                                         "[Position]: {position}\n" +
                                         "[Company]: {company}");

        var suffix = new PromptTemplate("[Text]: {input}\n" +
                                        "[Name]: ");

        var examples = new List<object>()
        {
            new {text = "Fred is a serial entrepreneur.Co-founder and CEO of Platform.sh, he previously co-founded Commerce Guys, a leading Drupal ecommerce provider.His mission is to guarantee that as we continue on an ambitious journey to profoundly transform how cloud computing is used and perceived, we keep our feet well on the ground continuing the rapid growth we have enjoyed up until now.",
                name = "Fred", position = "Co-founder and CEO", company = "Platform.sh"},
            new {text = "Microsoft (the word being a portmanteau of \"microcomputer software\") was founded by Bill Gates on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800. Steve Ballmer replaced Gates as CEO in 2000, and later envisioned a \"devices and services\" strategy.",
                name = "Steve Ballmer", position = "CEO", company = "Microsoft"},
            new {text = "Franck Riboud was born on 7 November 1955 in Lyon.He is the son of Antoine Riboud, the previous CEO, who transformed the former European glassmaker BSN Group into a leading player in the food industry.He is the CEO at Danone.",
                name = "Franck Riboud", position = "CEO", company = "Danone"}
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples.Select(i => i.ToDictionary()).ToList())
        {
            ExampleSeparator = "\n---\n" // use "---" as separator, and as a stop sequence
        };

        var llm = new OpenAIModel(Constants.OpenAIKey, new OpenAIModelConfiguration() { 
            NucleusSamplingFactor = 0,
            Model = "text-davinci-003",
            MaxTokens = 30,
            SnippetCount = 1,
            GenerationSampleCount = 1,
            Stop = new[] { "---" } });

        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("David Melvin is an investment and financial services professional at CITIC CLSA with over 30 years’ experience in investment banking and private equity.He is currently a Senior Adviser of CITIC CLSA.");
        Console.WriteLine($"> {answer1}");
        var answer2 = await executor.PromptAsync("Pat Gelsinger is a highly respected technology executive with over four decades of experience in the industry. He started his career at Intel, where he spent over 30 years in various roles, including as the company's first chief technology officer. During his tenure at Intel, Gelsinger was widely recognized for his technical expertise and leadership in driving innovation.");
        Console.WriteLine($"> {answer2}");
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_QuestionAnswering()
    {
        #region Example_ModelChainFewShotPromptTemplate_QuestionAnswering
        var example = new PromptTemplate("Context: {context}\n" +
                                         "Question: {question}\n" +
                                         "Answer: {answer}");

        var suffix = new PromptTemplate("Context: {context}\n" +
                                        "Question: {question}\n" +
                                        "Answer: ");

        var examples = new List<object>()
        {
            new { context = "OpenAI is an artificial intelligence research laboratory consisting of the for-profit corporation OpenAI LP and its parent company, the non-profit OpenAI Inc. It was founded in 2015 by a group of technology leaders, including Elon Musk and Sam Altman.", 
                question = "When was OpenAI founded?", answer = "2015" },
            new { context = "OpenAI has developed a number of significant artificial intelligence models and technologies, including the GPT series of natural language processing models, the DALL-E image generation model, and a range of robotics systems.", 
                question = "What did OpenAI develop?", answer = "AI models" },
            new { context = "OpenAI offers various plans for its products and services, including a free tier of access to its API, paid plans for businesses, and custom solutions for enterprises.", 
                question = "What plans are available in OpenAI?", answer = "Various plans" },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync(new { context = "OpenAI offers several plans for GPT-3, ranging from a free tier to paid plans with larger amounts of access and dedicated support. The recommended plan for GPT-3 would depend on the specific needs and use case of the individual or organization. However, the most commonly used paid plan for GPT-3 is the \"Pro\" plan, which provides a significant amount of access to the API and is suitable for most applications.", 
            question = "Which plan is recommended for GPT-3?" }.ToDictionary());
        Console.WriteLine(answer1.Values.Last());
        var answer2 = await executor.PromptAsync(new { context = "GPT-3 supports many different natural languages, including English, Spanish, French, German, Italian, Dutch, Portuguese, Japanese, Korean, Chinese, and more. However, English is the language that GPT-3 has been most extensively trained on, and for which it has produced the most impressive results. Therefore, English is generally considered the most preferable language for GPT-3.", 
            question = "Which language is preferable for GPT-3?"
        }.ToDictionary());
        Console.WriteLine(answer2.Values.Last());
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_GrammarCorrection()
    {
        #region Example_ModelChainFewShotPromptTemplate_GrammarCorrection
        var example = new PromptTemplate("{phrase}\n" +
                                         "Correction: {correction}");

        var suffix = new PromptTemplate("{phrase}\n" +
                                        "Correction: ");

        var examples = new List<object>()
        {
            new { phrase = "I love goin to the beach.", correction = "I love going to the beach." },
            new { phrase = "Let me hav it!", correction = "Let me have it!" },
            new { phrase = "It have too many drawbacks.", correction = "It has too many drawbacks." },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("I do not wan to go");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync(@"Wat ar u doing?");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_MachineTranslation()
    {
        #region Example_ModelChainFewShotPromptTemplate_MachineTranslation
        var example = new PromptTemplate("{phrase}\n" +
                                         "Translation: {translation}");

        var suffix = new PromptTemplate("{phrase}\n" +
                                        "Translation: ");

        var examples = new List<object>()
        {
            new { phrase = "Hugging Face a révolutionné le NLP.", translation = "Hugging Face revolutionized NLP." },
            new { phrase = "Cela est incroyable!", translation = "This is unbelievable!" },
            new { phrase = "Désolé je ne peux pas.", translation = "Sorry but I cannot." },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Parlez-vous français?");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync(@"Comment ça va? ");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_TweetGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_TweetGeneration
        var example = new PromptTemplate("Keyword: {keyword}\n" +
                                         "Tweet: {tweet}");
        var suffix = new PromptTemplate("Keyword: {keyword}\n" +
                                        "Tweet: ");

        var examples = new List<object>()
        {
            new { keyword = "markets", tweet = "Take feedback from nature and markets, not from people" },
            new { keyword = "children", tweet = "Maybe we die so we can come back as children." },
            new { keyword = "startups", tweet = " Startups should not worry about how to put out fires, they should worry about how to start them." },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.7f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("cats");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("NLP");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_Chatbot()
    {
        #region Example_ModelChainFewShotPromptTemplate_Chatbot

        var prefix = new PromptTemplate("This is a discussion between a [human] and a [robot].\n" +
                                        "The [robot] is very nice and empathetic.");

        var example = new PromptTemplate("[human]: {human_phrase}\n" +
                                         "[robot]: {robot_phrase}");

        var suffix = new PromptTemplate("[human]: {human_phrase}\n" +
                                        "[robot]: ");

        var examples = new List<object>()
        {
            new { human_phrase = "Hello nice to meet you.", robot_phrase = "Nice to meet you too." },
            new { human_phrase = "How is it going today?", robot_phrase = "Not so bad, thank you! How about you?" },
            new { human_phrase = "I am ok, but I am a bit sad...", robot_phrase = "Oh? Why that?" },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(prefix, example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.7f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("I broke up with my girlfriend...");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("I won a lot of money today");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_IntentClassification()
    {
        #region Example_ModelChainFewShotPromptTemplate_IntentClassification

        var example = new PromptTemplate("Statement: {statement}\nIntent: {intent}");
        var suffix = new PromptTemplate("Statement: {statement}\nIntent: ");

        var examples = new List<object>()
        {
            new { statement = "I want to start coding tomorrow because it seems to be so fun!", intent = "start coding" },
            new { statement = "Show me the last pictures you have please.", intent = "show pictures" },
            new { statement = "Search all these files as fast as possible.", intent = "search files" },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Can you please teach me Chinese next week?");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("Open the fridge, and put giraffe inside");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_Paraphrasing()
    {
        #region Example_ModelChainFewShotPromptTemplate_Paraphrasing

        var example = new PromptTemplate("[Original]: {original}\n" +
                                         "[Paraphrase]: {paraphrase}");
        var suffix = new PromptTemplate("[Original]: {original}\n" +
                                        "[Paraphrase]: ");

        var examples = new List<object>()
        {
            new { original = "If you tire of lazing on the beach after 10 minutes, this one's for you: eduvacations are getaways that are all about learning new things.", 
                paraphrase = "For those who get bored of lounging on the beach in just 10 minutes, an eduvacation might be a better fit since it is focused on learning new things." },
            new { original = "As lockdowns lifted, 2022 saw a new phenomenon of COVID-19 induced ‘revenge travel’ as people scrambled to make up for lost time.", 
                paraphrase = "In 2022, there was a new trend called \"revenge travel\" that emerged as people tried to make up for the time they lost due to COVID-19 lockdowns." },
            new { original = "From e-bikes to e-scooters to e-sleds, motorised personal transport is taking over cities and destinations across the world. More and more travel agencies are also offering electric bike escapes, allowing outdoor exploration without the gym bunny prerequisite.", 
                paraphrase = "Motorized personal transport, such as e-bikes, e-scooters, and e-sleds, is becoming increasingly popular in cities and tourist destinations around the world. More travel agencies are also offering electric bike trips, which allow outdoor exploration without requiring extreme physical fitness." },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Nature positive travel is set to take over in 2023 as holidaymakers seek ways to reduce and reverse their environmental impact.");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("The emergence of remote work in 2022 brought digital nomadism into the mainstream.");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_Summarization()
    {
        #region Example_ModelChainFewShotPromptTemplate_Summarization

        var example = new PromptTemplate("Original: {original}\n" +
                                         "Summary: {summary}");
        var suffix = new PromptTemplate("Original: {original}\n" +
                                        "Summary: ");

        var examples = new List<object>()
        {
            new { original = "America has changed dramatically during recent years. Not only has the number of graduates in traditional engineering disciplines such as mechanical, civil, electrical, chemical, and aeronautical engineering declined, but in most of the premier American universities engineering curricula now concentrate on and encourage largely the study of engineering science.  As a result, there are declining offerings in engineering subjects dealing with infrastructure, the environment, and related issues, and greater concentration on high technology subjects, largely supporting increasingly complex scientific developments. While the latter is important, it should not be at the expense of more traditional engineering.\r\nRapidly developing economies such as China and India, as well as other industrial countries in Europe and Asia, continue to encourage and advance the teaching of engineering. Both China and India, respectively, graduate six and eight times as many traditional engineers as does the United States. Other industrial countries at minimum maintain their output, while America suffers an increasingly serious decline in the number of engineering graduates and a lack of well-educated engineers. \r\n(Source:  Excerpted from Frankel, E.G. (2008, May/June) Change in education: The cost of sacrificing fundamentals. MIT Faculty ",
                summary = "MIT Professor Emeritus Ernst G. Frankel (2008) has called for a return to a course of study that emphasizes the traditional skills of engineering, noting that the number of American engineering graduates with these skills has fallen sharply when compared to the number coming from other countries."},
            new { original = "So how do you go about identifying your strengths and weaknesses, and analyzing the opportunities and threats that flow from them? SWOT Analysis is a useful technique that helps you to do this.\r\nWhat makes SWOT especially powerful is that, with a little thought, it can help you to uncover opportunities that you would not otherwise have spotted. And by understanding your weaknesses, you can manage and eliminate threats that might otherwise hurt your ability to move forward in your role.\r\nIf you look at yourself using the SWOT framework, you can start to separate yourself from your peers, and further develop the specialized talents and abilities that you need in order to advance your career and to help you achieve your personal goals.",
                summary = "SWOT Analysis is a technique that helps you identify strengths, weakness, opportunities, and threats. Understanding and managing these factors helps you to develop the abilities you need to achieve your goals and progress in your career."},
            new { original = "Jupiter is the fifth planet from the Sun and the largest in the Solar System. It is a gas giant with a mass one-thousandth that of the Sun, but two-and-a-half times that of all the other planets in the Solar System combined. Jupiter is one of the brightest objects visible to the naked eye in the night sky, and has been known to ancient civilizations since before recorded history. It is named after the Roman god Jupiter.[19] When viewed from Earth, Jupiter can be bright enough for its reflected light to cast visible shadows,[20] and is on average the third-brightest natural object in the night sky after the Moon and Venus.\r\nJupiter is primarily composed of hydrogen with a quarter of its mass being helium, though helium comprises only about a tenth of the number of molecules. It may also have a rocky core of heavier elements,[21] but like the other giant planets, Jupiter lacks a well-defined solid surface. Because of its rapid rotation, the planet's shape is that of an oblate spheroid (it has a slight but noticeable bulge around the equator).",
                summary = "Jupiter is the largest planet in the solar system. It is a gas giant, and is the fifth planet from the sun."}
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.1f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("For all its whizz-bang caper-gone-wrong energy, and for all its subsequent emotional troughs, this week’s Succession finale might have been the most important in its entire run. Because, unless I am very much wrong, Succession – a show about people trying to forcefully mount a succession – just had its succession. And now everything has to change.\r\nThe episode ended with Logan Roy defying his children by selling Waystar Royco to idiosyncratic Swedish tech bro Lukas Matsson. It’s an unexpected twist, like if King Lear contained a weird new beat where Lear hands the British crown to Jack Dorsey for a laugh, but it sets up a bold new future for the show. What will happen in season four? Here are some theories.\r\nSeason three of Succession picked up seconds after season two ended. It was a smart move, showing the immediate swirl of confusion that followed Kendall Roy’s decision to undo his father, and something similar could happen here. This week’s episode ended with three of the Roy siblings heartbroken and angry at their father’s grand betrayal. Perhaps season four could pick up at that precise moment, and show their efforts to reorganise their rebellion against him. This is something that Succession undoubtedly does very well – for the most part, its greatest moments have been those heart-thumping scenes where Kendall scraps for support to unseat his dad – and Jesse Armstrong has more than enough dramatic clout to centre the entire season around the battle to stop the Matsson deal dead in its tracks.");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("Roizman is the founder of Museum of Nevyansk Icon at Sverdlovsk region. This is the first private museum to collect icon-paintings. It is located in the city of Yekaterinburg. This museum has over 600 exhibits, including icons, gospel covers, crosses, books and wooden sculptures. The earliest icon is The Egyptian Mother of God (1734), the latest is Christ Pantocrator (1919). Roizman worked in finding, searching and restoration of the icons.");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_TextClassification()
    {
        #region Example_ModelChainFewShotPromptTemplate_TextClassification
        var example = new PromptTemplate("Message: {message}\n" +
                                         "Topic: {topic}");
        var suffix = new PromptTemplate("Message: {message}\n" +
                                        "Topic: ");

        var examples = new List<object>()
        {
            new { message = "When the spaceship landed on Mars, the whole humanity was excited", topic = "space"},
            new { message = "I love playing tennis and golf. I'm practicing twice a week.", topic = "sport"},
            new { message = "Managing a team of sales people is a tough but rewarding job.", topic = "business"},
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("I am trying to cook chicken with tomatoes.");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("2022 has been the year of solo female travel, with many women taking to the road after feeling the isolation of the pandemic.");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_KeywordExtraction()
    {
        #region Example_ModelChainFewShotPromptTemplate_KeywordExtraction
        var example = new PromptTemplate("Information: {information}\n" +
                                         "Keywords: {keywords}");
        var suffix = new PromptTemplate("Information: {information}\n" +
                                        "Keywords: ");

        var examples = new List<object>()
        {
            new { information = "Information Retrieval (IR) is the process of obtaining resources relevant to the information need. For instance, a search query on a web search engine can be an information need. The search engine can return web pages that represent relevant resources.", 
                keywords = "information, search, resources"},
            new { information = "David Robinson has been in Arizona for the last three months searching for his 24-year-old son, Daniel Robinson, who went missing after leaving a work site in the desert in his Jeep Renegade on June 23. ", 
                keywords = "searching, missing, desert"},
            new { information = "I believe that using a document about a topic that the readers know quite a bit about helps you understand if the resulting keyphrases are of quality.", 
                keywords = "document, understand, keyphrases"},
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Since transformer models have a token limit, you might run into some errors when inputting large documents. In that case, you could consider splitting up your document into paragraphs and mean pooling (taking the average of) the resulting vectors.");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("If your classes are not known in advance (e.g., they are set by a user or generated on the fly), you can try zero-shot classification by either giving an instruction containing the classes or even by using embeddings to see which class label (or other classified texts) are most similar to the text");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_KeyphraseExtraction()
    {
        #region Example_ModelChainFewShotPromptTemplate_KeyphraseExtraction
        var example = new PromptTemplate("Information: {information}\n" +
                                         "Keywords: {keywords}");
        var suffix = new PromptTemplate("Information: {information}\n" +
                                        "Keywords: ");

        var examples = new List<object>()
        {
            new { information = "Information Retrieval (IR) is the process of obtaining resources relevant to the information need. For instance, a search query on a web search engine can be an information need. The search engine can return web pages that represent relevant resources.",
                keywords = "information retrieval, search query, relevant resource"},
            new { information = "David Robinson has been in Arizona for the last three months searching for his 24-year-old son, Daniel Robinson, who went missing after leaving a work site in the desert in his Jeep Renegade on June 23. ",
                keywords = "searching son, missing after work, desert"},
            new { information = "I believe that using a document about a topic that the readers know quite a bit about helps you understand if the resulting keyphrases are of quality.",
                keywords = "document, help understand, resulting keyphrases"},
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("Since transformer models have a token limit, you might run into some errors when inputting large documents. In that case, you could consider splitting up your document into paragraphs and mean pooling (taking the average of) the resulting vectors.");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("If your classes are not known in advance (e.g., they are set by a user or generated on the fly), you can try zero-shot classification by either giving an instruction containing the classes or even by using embeddings to see which class label (or other classified texts) are most similar to the text");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_AdGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_AdGeneration
        var example = new PromptTemplate("Keywords: {keywords}\n" +
                                         "Sentence: {sentence}");
        var suffix = new PromptTemplate("Keywords: {keywords}\n" +
                                        "Sentence: ");

        var examples = new List<object>()
        {
            new { keywords = "shoes, women, $59", sentence = "Beautiful shoes for women at the price of $59."},
            new { keywords = "trousers, men, $69", sentence = "Modern trousers for men, for $69 only."},
            new { keywords = "gloves, winter, $19", sentence = "Amazingly hot gloves for cold winters, at $19."},
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("t-shirt, men, $39");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("car, dog, 199$");
        Console.WriteLine(answer2);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_PromptGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_PromptGeneration
        var example = new PromptTemplate("{phrase}\n" +
                                         "Prompt:\n{prompt}");

        var suffix = new PromptTemplate("{phrase}\n" +
                                        "Prompt:\n");

        var examples = new List<object>()
        {
            new { phrase = "Hugging Face a révolutionné le NLP.\r\nTranslation: Hugging Face revolutionized NLP.",
                prompt = "var example = new PromptTemplate(\"{phrase}\\nTranslation: {translation}\");\r\nvar suffix = new PromptTemplate(\"{phrase}\\nTranslation: \");" },
            new { phrase = "Description: Fetch the companies that have less than five people in it.\r\nCode: SELECT COMPANY, COUNT(EMPLOYEE_ID) FROM Employee GROUP BY COMPANY HAVING COUNT(EMPLOYEE_ID) < 5;",
                prompt = "var example = new PromptTemplate(\"Description: {Question}\\nCode: {Answer}\");\r\nvar suffix = new PromptTemplate(\"Description: {Question}\\nCode: \");" },
            new { phrase = "Context: NLP Cloud was founded in 2021 when the team realized there was no easy way to reliably leverage Natural Language Processing in production.\r\nQuestion: When was NLP Cloud founded?\r\nAnswer: 2021",
                prompt = "var example = new PromptTemplate(\"Context: {context}\\nQuestion: {question}\\nAnswer: {answer}\");\r\nvar suffix = new PromptTemplate(\"Context: {context}\\nQuestion: {question}\\nAnswer: \");" },
        }.Select(i => i.ToDictionary()).ToList();

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { MaxTokens = 200 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("keyword: markets\r\ntweet: Take feedback from nature and markets, not from people");
        Console.WriteLine(answer1);
        #endregion
    }
}