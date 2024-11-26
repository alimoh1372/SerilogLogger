using System.Text;

namespace SerilogLogger.Implementation.LogFormatter.ApplicationMonitor
{
    public class ApplicationMonitorXmlSerializer
    {
        private const char LtCharacter = '<';
        private const char GtCharacter = '>';
        private const char AmpCharacter = '&';
        private const char QuotCharacter = '\"';
        private const char AposCharacter = '\'';
        private const char LfCharacter = '\n';
        private const char CrCharacter = '\r';
        private const char TabCharacter = '\t';

        private const string LfString = "\n";
        private const string CrString = "\r";
        private const string TabString = "\t";

        private const string SerializedLt = "&lt;";
        private const string SerializedGt = "&gt;";
        private const string SerializedAmp = "&amp;";
        private const string SerializedQuot = "&quot;";
        private const string SerializedApos = "&apos;";
        private const string SerializedLf = "&#xA;";
        private const string SerializedCr = "&#xD;";
        private const string SerializedTab = "&#x9;";

        public void SerializeXmlValue(StringBuilder stringBuilder, string text, bool isAttribute)
        {
            foreach (var character in text)
            {
                stringBuilder.Append(SerializeXmlValue(character, isAttribute));
            }
        }

        internal string SerializeXmlValue(string text, bool isAttribute)
        {
            var builder = new StringBuilder();

            foreach (var character in text)
            {
                builder.Append(SerializeXmlValue(character, isAttribute));
            }

            return builder.ToString();
        }

        private static string SerializeXmlValue(char character, bool isAttribute)
        {
            if (character == LtCharacter)
            {
                return SerializedLt;
            }

            if (character == GtCharacter)
            {
                return SerializedGt;
            }

            if (character == AmpCharacter)
            {
                return SerializedAmp;
            }

            // Special handling for quotes
            if (isAttribute && character == QuotCharacter)
            {
                return SerializedQuot;
            }

            if (isAttribute && character == AposCharacter)
            {
                return SerializedApos;
            }

            // Legal sub-chr32 characters
            if (character == LfCharacter)
            {
                return isAttribute ? SerializedLf : LfString;
            }

            if (character == CrCharacter)
            {
                return isAttribute ? SerializedCr : CrString;
            }

            if (character == TabCharacter)
            {
                return isAttribute ? SerializedTab : TabString;
            }

            return character.ToString();
        }
    }
}
