using System.Text;

namespace QradarLogSystem.Api.Services.Parsers
{
    public class LeefEventParser
    {
        public string ConvertToKeyValue(string leefEvent)
        {
            if (string.IsNullOrWhiteSpace(leefEvent))
                throw new ArgumentException("LEEF event cannot be empty.");

            if (!leefEvent.StartsWith(
                    "LEEF:",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    "The event is not a valid LEEF event.");
            }

            // LEEF header:
            // LEEF:1.0|Vendor|Product|Version|EventId|Attributes

            var headerParts = leefEvent.Split('|', 6);

            if (headerParts.Length < 6)
            {
                throw new FormatException(
                    "LEEF header is incomplete.");
            }

            var leefVersion = headerParts[0].Trim();
            var vendor = headerParts[1].Trim();
            var product = headerParts[2].Trim();
            var productVersion = headerParts[3].Trim();
            var eventId = headerParts[4].Trim();
            var attributesPart = headerParts[5];

            var attributes =
                ParseAttributes(attributesPart);

            // -------------------------------------------------
            // QID
            // -------------------------------------------------
            // Öncelikle açık biçimde qid alanı aranır.
            // Eğer bulunamazsa yalnızca LEEF EventId sayısal ise
            // QID olarak kullanılır.
            //
            // EventId metinsel ise sahte bir QID oluşturulmaz.
            // Böyle durumlarda 0 kullanılır.
            // -------------------------------------------------

            var qid = ResolveQid(
                attributes,
                eventId);

            // -------------------------------------------------
            // EVENT NAME
            // -------------------------------------------------
            // Öncelik:
            // 1. eventName attribute
            // 2. Metinsel LEEF EventId
            // 3. cat/category
            // 4. Genel LEEF event adı
            // -------------------------------------------------

            var eventName = ResolveEventName(
                attributes,
                eventId);

            // -------------------------------------------------
            // SOURCE IP
            // -------------------------------------------------

            var sourceIp =
                GetFirstValue(
                    attributes,
                    "src",
                    "sourceIp",
                    "sourceAddress",
                    "srcIp")
                ?? string.Empty;

            // -------------------------------------------------
            // DESTINATION IP
            // -------------------------------------------------

            var destinationIp =
                GetFirstValue(
                    attributes,
                    "dst",
                    "destinationIp",
                    "destinationAddress",
                    "dstIp")
                ?? string.Empty;

            // -------------------------------------------------
            // SOURCE PORT
            // -------------------------------------------------

            var sourcePort =
                GetFirstValue(
                    attributes,
                    "srcPort",
                    "sourcePort",
                    "spt")
                ?? "0";

            // -------------------------------------------------
            // DESTINATION PORT
            // -------------------------------------------------

            var destinationPort =
                GetFirstValue(
                    attributes,
                    "dstPort",
                    "destinationPort",
                    "dpt")
                ?? "0";

            // -------------------------------------------------
            // USERNAME
            // -------------------------------------------------

            var username =
                GetFirstValue(
                    attributes,
                    "usrName",
                    "username",
                    "user",
                    "srcUser")
                ?? string.Empty;

            // -------------------------------------------------
            // SEVERITY
            // -------------------------------------------------

            var severity =
                ResolveSeverity(attributes);

            // -------------------------------------------------
            // MAGNITUDE
            // -------------------------------------------------

            var magnitude =
                GetFirstValue(
                    attributes,
                    "magnitude",
                    "mag")
                ?? severity;

            // -------------------------------------------------
            // LOG SOURCE ID
            // -------------------------------------------------

            var logSourceId =
                GetFirstValue(
                    attributes,
                    "logSourceId",
                    "deviceId")
                ?? "0";

            // -------------------------------------------------
            // LOG SOURCE NAME
            // -------------------------------------------------

            var logSourceName =
                GetFirstValue(
                    attributes,
                    "logSourceName",
                    "devName",
                    "deviceName")
                ?? product;

            // -------------------------------------------------
            // PAYLOAD
            // -------------------------------------------------

            var payload =
                GetFirstValue(
                    attributes,
                    "msg",
                    "message",
                    "payload")
                ?? leefEvent;

            return BuildKeyValueEvent(
                qid,
                eventName,
                sourceIp,
                destinationIp,
                sourcePort,
                destinationPort,
                username,
                severity,
                magnitude,
                logSourceId,
                logSourceName,
                payload
            );
        }

        // =====================================================
        // QID RESOLUTION
        // =====================================================

        private string ResolveQid(
            Dictionary<string, string> attributes,
            string eventId)
        {
            var attributeQid =
                GetFirstValue(
                    attributes,
                    "qid",
                    "QID");

            if (!string.IsNullOrWhiteSpace(attributeQid) &&
                int.TryParse(attributeQid, out _))
            {
                return attributeQid;
            }

            // LEEF EventId yalnızca tamamen sayısal ise
            // mevcut sistemde QID olarak kullanılabilir.

            if (!string.IsNullOrWhiteSpace(eventId) &&
                int.TryParse(eventId, out _))
            {
                return eventId;
            }

            // Gerçek QID bulunamadı.
            // Sahte QID oluşturulmuyor.

            return "0";
        }

        // =====================================================
        // EVENT NAME RESOLUTION
        // =====================================================

        private string ResolveEventName(
            Dictionary<string, string> attributes,
            string eventId)
        {
            var explicitEventName =
                GetFirstValue(
                    attributes,
                    "eventName",
                    "name");

            if (!string.IsNullOrWhiteSpace(explicitEventName))
                return explicitEventName;

            // EventId sayısal değilse çoğu üreticide olay
            // imzasını / olay adını temsil edebilir.

            if (!string.IsNullOrWhiteSpace(eventId) &&
                !int.TryParse(eventId, out _))
            {
                return eventId;
            }

            var category =
                GetFirstValue(
                    attributes,
                    "cat",
                    "category");

            if (!string.IsNullOrWhiteSpace(category))
                return category;

            if (!string.IsNullOrWhiteSpace(eventId))
                return $"LEEF Event {eventId}";

            return "LEEF Event";
        }

        // =====================================================
        // SEVERITY RESOLUTION
        // =====================================================

        private string ResolveSeverity(
            Dictionary<string, string> attributes)
        {
            var rawSeverity =
                GetFirstValue(
                    attributes,
                    "sev",
                    "severity");

            if (string.IsNullOrWhiteSpace(rawSeverity))
                return "1";

            // Sayısal severity ise doğrudan kullan.
            if (int.TryParse(rawSeverity, out var numericSeverity))
            {
                // Sistemin severity aralığını koru.
                numericSeverity =
                    Math.Clamp(numericSeverity, 1, 10);

                return numericSeverity.ToString();
            }

            // Bazı vendor datasetlerinde severity metinsel olabilir.
            return rawSeverity
                .Trim()
                .ToLowerInvariant() switch
            {
                "informational" => "1",
                "info" => "1",

                "low" => "3",

                "medium" => "5",
                "moderate" => "5",

                "high" => "8",

                "critical" => "10",
                "severe" => "10",

                _ => "1"
            };
        }

        // =====================================================
        // ATTRIBUTE PARSER
        // =====================================================

        private Dictionary<string, string> ParseAttributes(
            string attributesPart)
        {
            var result =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            // Dosyada gerçek TAB veya "\t" metni
            // bulunması durumlarının ikisini de destekle.

            var normalized =
                attributesPart.Replace("\\t", "\t");

            var fields =
                normalized.Contains('\t')
                    ? normalized.Split(
                        '\t',
                        StringSplitOptions.RemoveEmptyEntries)
                    : normalized.Split(
                        '|',
                        StringSplitOptions.RemoveEmptyEntries);

            foreach (var field in fields)
            {
                var parts = field.Split('=', 2);

                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (!result.ContainsKey(key))
                {
                    result[key] = value;
                }
            }

            return result;
        }

        // =====================================================
        // ATTRIBUTE HELPERS
        // =====================================================

        private string? GetFirstValue(
            Dictionary<string, string> attributes,
            params string[] keys)
        {
            foreach (var key in keys)
            {
                if (attributes.TryGetValue(
                        key,
                        out var value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        // =====================================================
        // NORMALIZED KEY-VALUE EVENT
        // =====================================================

        private string BuildKeyValueEvent(
            string qid,
            string eventName,
            string sourceIp,
            string destinationIp,
            string sourcePort,
            string destinationPort,
            string username,
            string severity,
            string magnitude,
            string logSourceId,
            string logSourceName,
            string payload)
        {
            var builder = new StringBuilder();

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

        // =====================================================
        // SANITIZE
        // =====================================================

        private string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }
    }
}