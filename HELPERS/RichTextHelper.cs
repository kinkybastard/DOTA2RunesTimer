using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DOTA2RunesTimer.HELPERS
{
    public static class RichTextHelper
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(RichTextHelper), new UIPropertyMetadata("", OnFormattedTextChanged));
        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }
        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichTextBox rtb) return;
            var text = e.NewValue as string ?? "";

            rtb.Document.Blocks.Clear();
            var paragraph = new Paragraph();
            {
                rtb.Document.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./MEDIA/TRAJAN PRO FONTS/#Trajan Pro");
                rtb.Document.FontSize = 16;
            };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                Brush color = GetBrushForWord(word);
                var run = new Run(word + " ") { Foreground = color };
                paragraph.Inlines.Add(run);
            }

            rtb.Document.Blocks.Add(paragraph);
        }
        private static Brush GetBrushForWord(string word)
        {
            return word switch
            {
                "POWER" => Brushes.IndianRed,
                "LOTUS" => Brushes.SkyBlue,
                "WATER" => Brushes.Cyan,
                "BOUNTY" => Brushes.Gold,
                "ROSHAN" => Brushes.Pink,
                "WISDOM" => Brushes.MediumOrchid,
                "TORMENTOR" => Brushes.Orange, _ => Brushes.White 
            };
        }
    }
}
