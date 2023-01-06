namespace CliHelper;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
public class CliAttribute : Attribute
{
    public string Alias { get; init; }
    public CliAttribute(string alias)
    { 
        Alias = alias; 
    }
}