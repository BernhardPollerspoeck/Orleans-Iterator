namespace Orleans.Iterator.Abstraction;

[GenerateSerializer]
public record GrainDescriptor(
    string GrainType,
    string StateName);