using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poster
{
    public class Tagger
    {
        public void Concat(){
            GetFooterTags();
        }

        internal static string GetFooterTags(MainWindow context)
        {
            var sb = new StringBuilder();
            sb.Append($"#{context.TextBox_Artist.Text.Replace(" ", "_")}");
            if (context.TextBox_Artist.Text.IndexOf(" ") > -1)
            {
                sb.Append($" #{context.TextBox_Artist.Text.Replace(" ", "")}");
            }
            sb.Append($" #{context.TextBox_Album.Text.Replace(" ", "_")}");
            if (context.TextBox_Album.Text.IndexOf(" ") > -1)
            {
                sb.Append($" #{context.TextBox_Album.Text.Replace(" ", "")}");
            }
            sb.AppendLine();

            foreach (string style in context.StylesList.Items)
            {
                if (style.IndexOf(" ") > -1 || style.IndexOf("-") > -1)
                {
                    sb.Append($"#{style.Replace(" ", "").Replace("-", "")} ");
                }
            }

            sb.Append(GenerateLabelTags(context.currentRelease.Labels));

            return sb.ToString();
        }

        private static string GenerateLabelTags(List<Label> labels)
        {
            var result = string.Empty;
            var last = labels.Last();

            foreach (var label in labels)
            {
                if (label.Name.IndexOf(" ") > -1)
                {
                    result = string.Concat(result, "#", label.Name.Replace(" ", "_"), " ");
                    result = string.Concat(result, "#", label.Name.Replace(" ", ""));
                }
                else
                {
                    result = string.Concat(result, "#", label.Name);
                }
                if (!label.Equals(last))
                {
                    result = string.Concat(result, " ");
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = string.Concat(Environment.NewLine, result);
            }
            return result;
        }

        internal static string GetHeaderTags(MainWindow context)
        {
            var sb = new StringBuilder();

            foreach (var style in context.StylesList.Items)
            {
                sb.Append($"#{(style as string).Replace(" ", "_")}");
                if (style != context.StylesList.Items[context.StylesList.Items.Count - 1])
                {
                    sb.Append($" / ");
                }
            }

            return sb.ToString();
        }
    }
}
