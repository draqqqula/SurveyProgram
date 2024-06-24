using System.Text;
using Tesseract;

using var engine = new TesseractEngine("", "rus+eng", EngineMode.LstmOnly);
var builder = new StringBuilder();

for (int i = 1; i <= 296; i++)
{
    Console.WriteLine($"Now parsing image number {i}");
    using var result = engine.Process(Pix.LoadFromFile($"images\\image{i}.png"));
    builder.Append(Format(result.GetText()));
    builder.Append("\n<s=>\n");
}
File.WriteAllText("output.txt", builder.ToString());

string Format(string input)
{
    var filtered = input.Split("\n")
        .Where(it => !string.IsNullOrEmpty(it) && !it.Contains("Очистить мой выбор"));

    var questionRelated = filtered.TakeWhile(it => it.Contains('?') || it.Contains(':'));

    var questionEndFound = false;

    var questionText = AddTags('q', string.Join(' ', filtered.TakeWhile(it =>
    {
        if (questionEndFound)
        {
            return false;
        }
        if (it.Contains('?') || it.Contains(':'))
        {
            questionEndFound = true;
        }
        return true;
    }).ToArray()));

    var answerRelated = filtered.Except(questionRelated).Select(it => AddTags('a', FormatAnswer(it)));

    var fulltext = string.Join('\n', new string[1] { questionText }.Concat(answerRelated));
    return fulltext;
}

string AddTags(char letter, string target)
{
    return $"<{letter}>{target}</{letter}>";
}

string FormatAnswer(string text)
{
    return text.TrimStart('О').TrimStart('©').Trim();
}