using System.Text.RegularExpressions;
using DocuGuardAI.Application.Common.Interfaces;

namespace DocuGuardAI.Infrastructure.NaturalLanguageProcessing;

public class TextPreprocessor : ITextPreprocessor
{
    public string Preprocess(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        var text = rawText.Trim();

        text = Regex.Replace(text, @"\s+", " ");

        text = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

        return text;
    }
}