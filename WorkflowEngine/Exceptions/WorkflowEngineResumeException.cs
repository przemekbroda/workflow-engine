namespace EventSourcingEngine.Exceptions;

public class WorkflowEngineResumeException(string message) : Exception(message);