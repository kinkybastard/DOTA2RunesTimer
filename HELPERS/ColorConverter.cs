using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DOTA2RunesTimer.HELPERS
{
    public class ColorConverter : IValueConverter
    {
        // Словарь
        private readonly Dictionary<string, Brush> _colors = new()
        {
                { "LOTUS", Brushes.Blue },
                { "POWER", Brushes.Red },
                { "WATER", Brushes.Cyan },
                { "BOUNTY", Brushes.Gold },
                { "ROSHAN", Brushes.Pink },
                { "WISDOM", Brushes.Purple },
                { "TERMENTOR", Brushes.DarkOrange }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text || targetType != typeof(FlowDocument)) return new FlowDocument();

            var doc = new FlowDocument();
            var paragraph = new Paragraph();

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                Brush color = _colors.TryGetValue(word, out var foundBrush) ? foundBrush : Brushes.White;

                var run = new Run(word) { Foreground = color };
                paragraph.Inlines.Add(run);

                if (i < words.Length - 1)
                {
                    paragraph.Inlines.Add(new Run(" "));
                }

            }

            doc.Blocks.Add(paragraph);
            return doc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        { 
            throw new NotImplementedException();
        }

    }
}
