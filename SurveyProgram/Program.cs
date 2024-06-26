using System.Text;
using System.Text.RegularExpressions;

var text = File.ReadAllText("questions.txt");
var regex = new Regex(@"(?'q'<q>[^<>]+<\/q>).*?(?'s'<s=[\d,]+>)", RegexOptions.Singleline);
var matches = regex.Matches(text);
var questions = matches.Select(QuestionInfo.FromRegex).ToArray();
var survey = new Survey();
survey.Start(questions);

class Survey
{
    public void Start(QuestionInfo[] Questions)
    {
        if (Questions.Length == 0)
        {
            Console.WriteLine("No questions provided");
            return;
        }
        var mistakes = new List<QuestionInfo>();
        Random.Shared.Shuffle(Questions);
        var counter = 1;
        foreach (var question in Questions)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Question { counter }/{ Questions.Length }, {mistakes.Count} mistakes");
            var result = Ask(question);
            if (!result)
            {
                mistakes.Add(question);
            }
            counter++;
        }
        Console.WriteLine($"Survey finished, {Questions.Length - mistakes.Count}/{Questions.Length}.");
        Start(mistakes.ToArray());
    }

    private bool Ask(QuestionInfo info)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(info.Question);
        var counter = 0;
        foreach (var answer in info.Answers)
        {
            counter++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{counter}) {answer};");
        }
        var input = Console.ReadLine();
        var number = Convert.ToInt32(Regex.Match(input, @"\d+").Value);
        if (info.Test(input))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Correct");
            return true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Wrong!, correct answer is {info.CorrectAnswerString}");
            return false;
        }
    }
}

class QuestionInfo
{
    public required string Question { get; init; }
    public required string[] Answers { get; init; }
    public required int[] Solution { get; init; }

    public static QuestionInfo FromRegex(Match match)
    {
        var question = TrimTags(match.Groups["q"].Value);
        var solution = GetSolutions(match.Groups["s"].Value);
        var answers = Regex.Matches(match.Value, "(?'a'<a>[^<>]+<\\/a>)")
            .Where(it => it.Value.Length > 0)
            .Select(it => TrimTags(it.Value));
        return new QuestionInfo()
        {
            Question = question,
            Solution = solution,
            Answers = answers.ToArray(),
        };
    }

    private static string TrimTags(string input)
    {
        var regex = new Regex(@"(?<=<.>)[^<>]+(?=<\/.>)");
        return regex.Match(input).Value;
    }

    private static int[] GetSolutions(string tag)
    {
        var regex = new Regex(@"(?<=<s=)(\d+,?)+(?=>)");
        return regex.Match(tag)
            .Value.Split(',')
            .Select(it => Convert.ToInt32(it))
            .ToArray();
    }

    public bool Test(string input)
    {
        return input.Split(' ')
            .Select(it => Convert.ToInt32(it))
            .OrderBy(it => it)
            .SequenceEqual(Solution);
    }

    public string CorrectAnswerString
    {
        get
        {
            var builder = new StringBuilder();
            foreach (var number in Solution)
            {
                builder.AppendLine(Answers[number - 1]);
            }
            return builder.ToString();
        }
    }
}