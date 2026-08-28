namespace QradarLogSystem.Api.Models
{
    public class MultipleEventRequest
    {
        public List<string> RawEvents { get; set; } = new();
    }
}