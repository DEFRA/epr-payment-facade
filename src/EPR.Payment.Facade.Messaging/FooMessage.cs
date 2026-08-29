using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Messaging;

[ExcludeFromCodeCoverage]
public record FooMessage(string Data1, string Data2);