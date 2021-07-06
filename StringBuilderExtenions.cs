using System.Text;
using System.Windows.Controls;

namespace PlayDocumentation
{
    public static class StringBuilderUtility
    {
        public static void AppendKeyValue(this StringBuilder stringBuilder, ComboBox key, TextBox value)
        {
            if (!string.IsNullOrWhiteSpace(key.Text))
            {
                AppendKeyValue(stringBuilder, key.Text.Trim(), value);
            }
        }

        public static void AppendKeyValue(this StringBuilder stringBuilder, string key, TextBox value)
        {
            AppendKeyValue(stringBuilder, key, value.Text);
        }

        public static void AppendKeyValue(this StringBuilder stringBuilder, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("<dt>" + key.Replace(":", "").Trim() + ":</dt>");
                stringBuilder.AppendLine("<dd>" + value.Trim() + "</dd>");
            }
        }
    }
}
