using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class AddCommandAttribute: Attribute
{
    public string CommandName { get; private set; }

    public AddCommandAttribute(string commandName)
    {
        CommandName = commandName;
    }
}
