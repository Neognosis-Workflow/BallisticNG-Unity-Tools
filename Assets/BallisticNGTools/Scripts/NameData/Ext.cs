using System;

namespace NgData.NameData
{
    public static class Ext
    {
        public static string[] CommaSeparate(this string value, bool trimWhiteSpace, bool trimQuotes)
        {
            if (string.IsNullOrEmpty(value)) return Array.Empty<string>();
            
            string[] split = value.Split(',');
            if (!trimWhiteSpace && !trimQuotes) return split;
            
            for (int i = 0; i < split.Length; ++i)
            {
                string s = split[i];
                if (trimWhiteSpace) s = s.Trim();
                if (trimQuotes) s = s.Trim('"');
                split[i] = s;
            }

            return split;
        }

    }
}