namespace JoyGame.CaseStudy.Application.Exceptions;

public class EntityNotFoundException(string name, object key)
    : ApplicationException($"Entity '{name}' with identifier '{key}' was not found.");