using System;

/// <summary>
/// Custom attribute used to store descriptive metadata for API methods.
/// This data is retrieved at runtime via Reflection for UI documentation purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class APIDocAttribute : Attribute
{
    public string Description { get; }

    public APIDocAttribute(string description)
    {
        Description = description;
    }
}