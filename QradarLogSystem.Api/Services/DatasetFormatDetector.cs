namespace QradarLogSystem.Api.Services
{
    public class DatasetFormatDetector
    {
        public string Detect(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "EMPTY";

            var extension = Path.GetExtension(fileName)
                .ToLowerInvariant();

            var trimmedContent = content.Trim();

            // LEEF kontrolü
            if (trimmedContent.Contains(
                "LEEF:1.0",
                StringComparison.OrdinalIgnoreCase))
            {
                return "LEEF_1_0";
            }

            if (trimmedContent.Contains(
                "LEEF:2.0",
                StringComparison.OrdinalIgnoreCase))
            {
                return "LEEF_2_0";
            }

            // JSONL kontrolü
            // Her satır bağımsız bir JSON nesnesidir.
            if (extension == ".jsonl")
            {
                var lines = content
                    .Split(
                        new[] { "\r\n", "\n" },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line =>
                        !string.IsNullOrWhiteSpace(line))
                    .ToList();

                if (lines.Count > 0 &&
                    lines.All(line =>
                        line.StartsWith("{") &&
                        line.EndsWith("}")))
                {
                    return "JSONL";
                }

                return "UNKNOWN";
            }

            // JSON kontrolü
            if (extension == ".json")
            {
                if ((trimmedContent.StartsWith("{") &&
                     trimmedContent.EndsWith("}")) ||
                    (trimmedContent.StartsWith("[") &&
                     trimmedContent.EndsWith("]")))
                {
                    return "JSON";
                }

                return "UNKNOWN";
            }

            // CSV kontrolü
            if (extension == ".csv")
            {
                return "CSV";
            }

            // Mevcut key=value formatı
            if (trimmedContent.Contains(
                    "qid=",
                    StringComparison.OrdinalIgnoreCase) &&
                trimmedContent.Contains("|"))
            {
                return "KEY_VALUE";
            }

            return "UNKNOWN";
        }
    }
}