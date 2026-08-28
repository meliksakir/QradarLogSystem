using System.Text;

namespace QradarLogSystem.Api.Services.Parsers
{
    public class CsvEventParser
    {
        public List<string> ConvertToKeyValueEvents(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("CSV content cannot be empty.");

            var lines = content
                .Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count < 2)
                throw new FormatException(
                    "CSV dataset must contain a header and at least one data row.");

            var headers = ParseCsvLine(lines[0])
                .Select(header => header.Trim())
                .ToList();

            var rawEvents = new List<string>();

            for (var i = 1; i < lines.Count; i++)
            {
                var values = ParseCsvLine(lines[i]);

                if (values.Count == 0)
                    continue;

                var row = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                for (var columnIndex = 0;
                     columnIndex < headers.Count;
                     columnIndex++)
                {
                    var value =
                        columnIndex < values.Count
                            ? values[columnIndex].Trim()
                            : string.Empty;

                    row[headers[columnIndex]] = value;
                }

                rawEvents.Add(
                    BuildKeyValueEvent(row));
            }

            return rawEvents;
        }

        private string BuildKeyValueEvent(
            Dictionary<string, string> row)
        {
            var qid =
                GetValue(row, "qid")
                ?? GetValue(row, "eventid")
                ?? "0";

            var eventName =
                GetValue(row, "eventname")
                ?? GetValue(row, "event_name")
                ?? GetValue(row, "name")
                ?? "CSV Event";

            var sourceIp =
                GetValue(row, "sourceip")
                ?? GetValue(row, "source_ip")
                ?? GetValue(row, "src")
                ?? string.Empty;

            var destinationIp =
                GetValue(row, "destinationip")
                ?? GetValue(row, "destination_ip")
                ?? GetValue(row, "dst")
                ?? string.Empty;

            var sourcePort =
                GetValue(row, "sourceport")
                ?? GetValue(row, "source_port")
                ?? GetValue(row, "srcport")
                ?? "0";

            var destinationPort =
                GetValue(row, "destinationport")
                ?? GetValue(row, "destination_port")
                ?? GetValue(row, "dstport")
                ?? "0";

            var username =
                GetValue(row, "username")
                ?? GetValue(row, "user")
                ?? GetValue(row, "usrname")
                ?? string.Empty;

            var severity =
                GetValue(row, "severity")
                ?? GetValue(row, "sev")
                ?? "1";

            var magnitude =
                GetValue(row, "magnitude")
                ?? severity;

            var logSourceId =
                GetValue(row, "logsourceid")
                ?? GetValue(row, "log_source_id")
                ?? "0";

            var logSourceName =
                GetValue(row, "logsourcename")
                ?? GetValue(row, "log_source_name")
                ?? GetValue(row, "logsource")
                ?? "CSV Dataset";

            var payload =
                GetValue(row, "payload")
                ?? GetValue(row, "message")
                ?? GetValue(row, "msg")
                ?? string.Empty;

            var builder = new StringBuilder();

            builder.Append($"qid={Sanitize(qid)}|");
            builder.Append($"eventName={Sanitize(eventName)}|");
            builder.Append($"sourceIp={Sanitize(sourceIp)}|");
            builder.Append($"destinationIp={Sanitize(destinationIp)}|");
            builder.Append($"sourcePort={Sanitize(sourcePort)}|");
            builder.Append($"destinationPort={Sanitize(destinationPort)}|");
            builder.Append($"username={Sanitize(username)}|");
            builder.Append($"severity={Sanitize(severity)}|");
            builder.Append($"magnitude={Sanitize(magnitude)}|");
            builder.Append($"logSourceId={Sanitize(logSourceId)}|");
            builder.Append($"logSourceName={Sanitize(logSourceName)}|");
            builder.Append($"payload={Sanitize(payload)}");

            return builder.ToString();
        }

        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();

            var currentValue = new StringBuilder();
            var insideQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (character == '"')
                {
                    if (insideQuotes &&
                        i + 1 < line.Length &&
                        line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }

                    continue;
                }

                if (character == ',' && !insideQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                    continue;
                }

                currentValue.Append(character);
            }

            values.Add(currentValue.ToString());

            return values;
        }

        private string? GetValue(
            Dictionary<string, string> row,
            string key)
        {
            return row.TryGetValue(key, out var value) &&
                   !string.IsNullOrWhiteSpace(value)
                ? value
                : null;
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