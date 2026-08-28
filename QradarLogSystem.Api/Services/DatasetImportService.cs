using QradarLogSystem.Api.Models;
using QradarLogSystem.Api.Services.Parsers;

namespace QradarLogSystem.Api.Services
{
    public class DatasetImportService
    {
        private readonly DatasetFormatDetector _formatDetector;
        private readonly LeefEventParser _leefEventParser;
        private readonly CsvEventParser _csvEventParser;
        private readonly JsonEventParser _jsonEventParser;
        private readonly EventProcessingService _eventProcessingService;

        public DatasetImportService(
            DatasetFormatDetector formatDetector,
            LeefEventParser leefEventParser,
            CsvEventParser csvEventParser,
            JsonEventParser jsonEventParser,
            EventProcessingService eventProcessingService)
        {
            _formatDetector = formatDetector;
            _leefEventParser = leefEventParser;
            _csvEventParser = csvEventParser;
            _jsonEventParser = jsonEventParser;
            _eventProcessingService = eventProcessingService;
        }

        public DatasetImportResult Import(
            string content,
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "Dataset content cannot be empty.");
            }

            var detectedFormat =
                _formatDetector.Detect(content, fileName);

            var rawEvents =
                ConvertDatasetToRawEvents(
                    content,
                    detectedFormat);

            if (rawEvents.Count == 0)
            {
                throw new FormatException(
                    "No processable event records were found in the dataset.");
            }

            var processingResult =
                _eventProcessingService.ProcessMultiple(rawEvents);

            return new DatasetImportResult
            {
                FileName = fileName,
                DetectedFormat = detectedFormat,

                TotalRecords =
                    processingResult.TotalCount,

                SuccessCount =
                    processingResult.SuccessCount,

                FailedCount =
                    processingResult.FailedCount,

                SuccessfulEvents =
                    processingResult.SuccessfulEvents,

                FailedEvents =
                    processingResult.FailedEvents,

                TotalProcessingTimeMs =
                    processingResult.TotalProcessingTimeMs
            };
        }

        private List<string> ConvertDatasetToRawEvents(
            string content,
            string detectedFormat)
        {
            var lines = content
                .Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line =>
                    !string.IsNullOrWhiteSpace(line))
                .ToList();

            switch (detectedFormat)
            {
                case "KEY_VALUE":
                    return lines;

                case "LEEF_1_0":
                case "LEEF_2_0":
                    return lines
                        .Where(line =>
                            line.StartsWith(
                                "LEEF:",
                                StringComparison.OrdinalIgnoreCase))
                        .Select(line =>
                            _leefEventParser
                                .ConvertToKeyValue(line))
                        .ToList();

                case "CSV":
                    return _csvEventParser
                        .ConvertToKeyValueEvents(content);

                case "JSON":
                    return _jsonEventParser
                        .ConvertToKeyValueEvents(content);

                case "JSONL":
                    return _jsonEventParser
                        .ConvertJsonLinesToKeyValueEvents(content);

                case "EMPTY":
                    throw new ArgumentException(
                        "Dataset is empty.");

                default:
                    throw new FormatException(
                        $"Unsupported or unknown dataset format: {detectedFormat}");
            }
        }
    }
}