namespace EventSourcingEngine.Exceptions;

public class EventSourceEngineResumeException(string message) : Exception(message);