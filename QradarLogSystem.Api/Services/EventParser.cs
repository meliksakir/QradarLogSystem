using QradarLogSystem.Api.Models;

namespace QradarLogSystem.Api.Services
{
    public class EventParser
    {
        public QradarEvent Parse(string rawEvent)
        {
            if (string.IsNullOrWhiteSpace(rawEvent))
                throw new ArgumentException("Raw event cannot be empty.");

            var parsedEvent = new QradarEvent();

            var fields = rawEvent.Split('|');

            var parsedFieldCount = 0;

            foreach (var field in fields)
            {
                var parts = field.Split('=', 2);

                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "qid":
                        if (long.TryParse(value, out var qid))
                        {
                            parsedEvent.Qid = qid;
                            parsedFieldCount++;
                        }
                        break;

                    case "eventname":
                        parsedEvent.EventName = value;
                        parsedFieldCount++;
                        break;

                    case "sourceip":
                        parsedEvent.SourceIp = value;
                        parsedFieldCount++;
                        break;

                    case "destinationip":
                        parsedEvent.DestinationIp = value;
                        parsedFieldCount++;
                        break;

                    case "sourceport":
                        if (int.TryParse(value, out var sourcePort))
                        {
                            parsedEvent.SourcePort = sourcePort;
                            parsedFieldCount++;
                        }
                        break;

                    case "destinationport":
                        if (int.TryParse(value, out var destinationPort))
                        {
                            parsedEvent.DestinationPort = destinationPort;
                            parsedFieldCount++;
                        }
                        break;

                    case "username":
                        parsedEvent.Username = value;
                        parsedFieldCount++;
                        break;

                    case "severity":
                        if (int.TryParse(value, out var severity))
                        {
                            parsedEvent.Severity = severity;
                            parsedFieldCount++;
                        }
                        break;

                    case "magnitude":
                        if (int.TryParse(value, out var magnitude))
                        {
                            parsedEvent.Magnitude = magnitude;
                            parsedFieldCount++;
                        }
                        break;

                    case "logsourceid":
                        if (int.TryParse(value, out var logSourceId))
                        {
                            parsedEvent.LogSourceId = logSourceId;
                            parsedFieldCount++;
                        }
                        break;

                    case "logsourcename":
                        parsedEvent.LogSourceName = value;
                        parsedFieldCount++;
                        break;

                    case "payload":
                        parsedEvent.Payload = value;
                        parsedFieldCount++;
                        break;
                }
            }

            if (parsedFieldCount == 0)
                throw new FormatException("The raw event could not be parsed.");

            return parsedEvent;
        }
    }
}