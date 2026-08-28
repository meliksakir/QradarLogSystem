using System.Text;
using System.Text.Json;

namespace QradarLogSystem.Api.Services.Parsers
{
    public class JsonEventParser
    {
        // Normal JSON dosyalarını işler:
        // JSON Array veya tek JSON Object
        public List<string> ConvertToKeyValueEvents(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException(
                    "JSON content cannot be empty.");

            var trimmedContent = content.Trim();

            if (trimmedContent.StartsWith("["))
            {
                return ParseJsonArray(trimmedContent);
            }

            if (trimmedContent.StartsWith("{"))
            {
                return ParseSingleJsonObject(trimmedContent);
            }

            throw new FormatException(
                "Invalid JSON dataset format.");
        }

        // JSONL dosyalarını işler:
        // Her satır bağımsız bir JSON nesnesidir.
        public List<string> ConvertJsonLinesToKeyValueEvents(
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException(
                    "JSONL content cannot be empty.");

            return ParseJsonLines(content);
        }

        private List<string> ParseJsonArray(string content)
        {
            using var document =
                JsonDocument.Parse(content);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Array)
            {
                throw new FormatException(
                    "JSON root element must be an array.");
            }

            var rawEvents = new List<string>();

            foreach (var element in
                     document.RootElement.EnumerateArray())
            {
                if (element.ValueKind !=
                    JsonValueKind.Object)
                {
                    continue;
                }

                rawEvents.Add(
                    BuildKeyValueEvent(element));
            }

            if (rawEvents.Count == 0)
            {
                throw new FormatException(
                    "No valid JSON event records were found.");
            }

            return rawEvents;
        }

        private List<string> ParseSingleJsonObject(
            string content)
        {
            using var document =
                JsonDocument.Parse(content);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Object)
            {
                throw new FormatException(
                    "JSON root element must be an object.");
            }

            return new List<string>
            {
                BuildKeyValueEvent(
                    document.RootElement)
            };
        }

        private List<string> ParseJsonLines(
            string content)
        {
            var lines = content
                .Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line =>
                    !string.IsNullOrWhiteSpace(line))
                .ToList();

            var rawEvents =
                new List<string>();

            foreach (var line in lines)
            {
                try
                {
                    using var document =
                        JsonDocument.Parse(line);

                    if (document.RootElement.ValueKind !=
                        JsonValueKind.Object)
                    {
                        continue;
                    }

                    rawEvents.Add(
                        BuildKeyValueEvent(
                            document.RootElement));
                }
                catch (JsonException)
                {
                    // Hatalı JSONL satırı atlanır.
                    continue;
                }
            }

            if (rawEvents.Count == 0)
            {
                throw new FormatException(
                    "No valid JSONL event records were found.");
            }

            return rawEvents;
        }

        private string BuildKeyValueEvent(
            JsonElement element)
        {
            var qid =
                GetValue(element, "qid")
                ?? GetValue(element, "eventId")
                ?? GetValue(element, "event_id")
                ?? "0";

            var eventName =
                GetValue(element, "eventName")
                ?? GetValue(element, "event_name")
                ?? GetValue(element, "name")
                ?? "JSON Event";

            var sourceIp =
                GetValue(element, "sourceIp")
                ?? GetValue(element, "source_ip")
                ?? GetValue(element, "src")
                ?? string.Empty;

            var destinationIp =
                GetValue(element, "destinationIp")
                ?? GetValue(element, "destination_ip")
                ?? GetValue(element, "dst")
                ?? string.Empty;

            var sourcePort =
                GetValue(element, "sourcePort")
                ?? GetValue(element, "source_port")
                ?? GetValue(element, "srcPort")
                ?? "0";

            var destinationPort =
                GetValue(element, "destinationPort")
                ?? GetValue(element, "destination_port")
                ?? GetValue(element, "dstPort")
                ?? "0";

            var username =
                GetValue(element, "username")
                ?? GetValue(element, "user")
                ?? GetValue(element, "usrName")
                ?? string.Empty;

            var severity =
                GetValue(element, "severity")
                ?? GetValue(element, "sev")
                ?? "1";

            var magnitude =
                GetValue(element, "magnitude")
                ?? severity;

            var logSourceId =
                GetValue(element, "logSourceId")
                ?? GetValue(element, "log_source_id")
                ?? "0";

            var logSourceName =
                GetValue(element, "logSourceName")
                ?? GetValue(element, "log_source_name")
                ?? GetValue(element, "logSource")
                ?? "JSON Dataset";

            var payload =
                GetValue(element, "payload")
                ?? GetValue(element, "message")
                ?? GetValue(element, "msg")
                ?? string.Empty;

            var builder =
                new StringBuilder();

            builder.Append(
                $"qid={Sanitize(qid)}|");

            builder.Append(
                $"eventName={Sanitize(eventName)}|");

            builder.Append(
                $"sourceIp={Sanitize(sourceIp)}|");

            builder.Append(
                $"destinationIp={Sanitize(destinationIp)}|");

            builder.Append(
                $"sourcePort={Sanitize(sourcePort)}|");

            builder.Append(
                $"destinationPort={Sanitize(destinationPort)}|");

            builder.Append(
                $"username={Sanitize(username)}|");

            builder.Append(
                $"severity={Sanitize(severity)}|");

            builder.Append(
                $"magnitude={Sanitize(magnitude)}|");

            builder.Append(
                $"logSourceId={Sanitize(logSourceId)}|");

            builder.Append(
                $"logSourceName={Sanitize(logSourceName)}|");

            builder.Append(
                $"payload={Sanitize(payload)}");

            return builder.ToString();
        }

        private string? GetValue(
            JsonElement element,
            string propertyName)
        {
            foreach (var property in
                     element.EnumerateObject())
            {
                if (!property.Name.Equals(
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return property.Value.ValueKind switch
                {
                    JsonValueKind.String =>
                        property.Value.GetString(),

                    JsonValueKind.Number =>
                        property.Value.ToString(),

                    JsonValueKind.True =>
                        "true",

                    JsonValueKind.False =>
                        "false",

                    JsonValueKind.Null =>
                        null,

                    _ =>
                        property.Value.ToString()
                };
            }

            return null;
        }

        private string Sanitize(string value)
        {
            return value
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }
    }
}