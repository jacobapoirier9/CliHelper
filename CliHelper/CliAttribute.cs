﻿namespace CliHelper;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
public class CliAttribute : Attribute
{
    public string Alias { get; init; }
    public CliAttribute(string alias)
    { 
        Alias = alias; 
    }

    public object DefaultValue { get; set; }
}