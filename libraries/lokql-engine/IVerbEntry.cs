using System.Reflection;
using CommandLine;

namespace Lokql.Engine;

public interface IVerbEntry
{
    string Name { get; }
    string HelpText { get; }
    bool SupportsFiles { get; }
    string[] Aliases { get; }
}

public readonly record struct VerbDto() : IVerbEntry
{
    public string Name { get; init; } = "";
    public string HelpText { get; init; } = "";
    public bool SupportsFiles { get; init; } = false;
    public string[] Aliases { get; init; } = [];
};

public class VerbEntry : IVerbEntry
{
    private readonly VerbAttribute _verb;
    private readonly Type _optionType;
    private ValueEntry[]? _valueEntries;
    public string Name => _verb.Name;
    public string[] Aliases => _verb.Aliases;
    public string HelpText => _verb.HelpText;
    public bool SupportsFiles => _optionType.IsAssignableTo(typeof(IFileCommandOption));

    public VerbEntry(Type optionType)
    {
        _optionType = optionType;
        _verb = optionType.GetTypeInfo().GetCustomAttribute<VerbAttribute>()
                     ?? throw new InvalidOperationException(
                         $"All registered command options should have a {nameof(VerbAttribute)}. {optionType.FullName ?? optionType.Name} does not."
                     );
    }


    private IEnumerable<ValueEntry> GetValueEntries()
    {
        if (_valueEntries is not null)
        {
            return _valueEntries;
        }

        _valueEntries = (
                from p in _optionType.GetProperties()
                let v = p.GetCustomAttribute<ValueAttribute>()
                where v is not null
                select new ValueEntry(p, v)
            )
            .ToArray();

        return _valueEntries;
    }

    private class ValueEntry(PropertyInfo property, ValueAttribute value)
    {
        public string Name => property.Name;
        public ValueAttribute Value => value;
    }

}

