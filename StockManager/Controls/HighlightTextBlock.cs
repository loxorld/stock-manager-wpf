using StockManager.Application.Text;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StockManager.Controls;

public class HighlightTextBlock : TextBlock
{
    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(
            nameof(SourceText),
            typeof(string),
            typeof(HighlightTextBlock),
            new PropertyMetadata(string.Empty, OnHighlightPropertyChanged));

    public static readonly DependencyProperty HighlightTextProperty =
        DependencyProperty.Register(
            nameof(HighlightText),
            typeof(string),
            typeof(HighlightTextBlock),
            new PropertyMetadata(string.Empty, OnHighlightPropertyChanged));

    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.Register(
            nameof(HighlightBackground),
            typeof(Brush),
            typeof(HighlightTextBlock),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0x9F)), OnHighlightPropertyChanged));

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(HighlightTextBlock),
            new PropertyMetadata(Brushes.Black, OnHighlightPropertyChanged));

    public string SourceText
    {
        get => (string)GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    public string HighlightText
    {
        get => (string)GetValue(HighlightTextProperty);
        set => SetValue(HighlightTextProperty, value);
    }

    public Brush HighlightBackground
    {
        get => (Brush)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }

    public Brush HighlightForeground
    {
        get => (Brush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    private static void OnHighlightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HighlightTextBlock textBlock)
            textBlock.RefreshInlines();
    }

    private void RefreshInlines()
    {
        Inlines.Clear();

        if (string.IsNullOrEmpty(SourceText))
            return;

        var ranges = BuildHighlightRanges(SourceText, HighlightText);
        if (ranges.Count == 0)
        {
            Inlines.Add(new Run(SourceText));
            return;
        }

        var currentIndex = 0;
        foreach (var (start, end) in ranges)
        {
            if (start > currentIndex)
                Inlines.Add(new Run(SourceText[currentIndex..start]));

            Inlines.Add(new Run(SourceText[start..end])
            {
                Background = HighlightBackground,
                Foreground = HighlightForeground,
                FontWeight = FontWeights.SemiBold
            });

            currentIndex = end;
        }

        if (currentIndex < SourceText.Length)
            Inlines.Add(new Run(SourceText[currentIndex..]));
    }

    private static List<(int start, int end)> BuildHighlightRanges(string text, string highlightText)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(highlightText))
            return [];

        var searchTerms = SearchTextNormalizer.Normalize(highlightText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (searchTerms.Length == 0)
            return [];

        BuildNormalizedText(text, out var normalizedText, out var indexMap);
        if (normalizedText.Length == 0)
            return [];

        var ranges = new List<(int start, int end)>();
        foreach (var term in searchTerms)
        {
            var searchIndex = 0;
            while (searchIndex < normalizedText.Length)
            {
                var found = normalizedText.IndexOf(term, searchIndex, StringComparison.Ordinal);
                if (found < 0)
                    break;

                var originalStart = indexMap[found];
                var originalEnd = indexMap[found + term.Length - 1] + 1;
                ranges.Add((originalStart, originalEnd));
                searchIndex = found + term.Length;
            }
        }

        if (ranges.Count == 0)
            return ranges;

        ranges.Sort(static (left, right) => left.start.CompareTo(right.start));

        var merged = new List<(int start, int end)> { ranges[0] };
        for (var i = 1; i < ranges.Count; i++)
        {
            var current = ranges[i];
            var last = merged[^1];

            if (current.start <= last.end)
            {
                merged[^1] = (last.start, Math.Max(last.end, current.end));
                continue;
            }

            merged.Add(current);
        }

        return merged;
    }

    private static void BuildNormalizedText(string text, out string normalizedText, out List<int> indexMap)
    {
        var buffer = new List<char>(text.Length);
        indexMap = new List<int>(text.Length);
        var previousWasSpace = false;

        for (var index = 0; index < text.Length; index++)
        {
            var sourceChar = text[index];
            var normalized = sourceChar.ToString().Normalize(NormalizationForm.FormD);

            foreach (var normalizedChar in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(normalizedChar);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(normalizedChar))
                {
                    buffer.Add(char.ToUpperInvariant(normalizedChar));
                    indexMap.Add(index);
                    previousWasSpace = false;
                    continue;
                }

                if (previousWasSpace)
                    continue;

                buffer.Add(' ');
                indexMap.Add(index);
                previousWasSpace = true;
            }
        }

        normalizedText = new string([.. buffer]).Trim();

        // Keep the index map aligned with the trimmed text.
        if (normalizedText.Length == 0 || buffer.Count == normalizedText.Length)
            return;

        var leadingTrim = 0;
        while (leadingTrim < buffer.Count && buffer[leadingTrim] == ' ')
            leadingTrim++;

        if (leadingTrim > 0)
            indexMap.RemoveRange(0, leadingTrim);

        var trailingToRemove = buffer.Count - leadingTrim - normalizedText.Length;
        if (trailingToRemove > 0)
            indexMap.RemoveRange(indexMap.Count - trailingToRemove, trailingToRemove);
    }
}
