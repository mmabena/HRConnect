namespace HRConnect.Api.Utils.Enums
{
  using System.Text.RegularExpressions;

  public static class MedicalOptionEnumMapper
  {
    // Forward Mapping : Category -> Variant
    private static readonly Dictionary<string, Choice> ChoiceMappings = new()
    {
      { "Network Choice", Choice.Network },
      { "First Choice", Choice.First }
    };
    
        
    private static readonly Dictionary<string, Essential> EssentialMappings = new()
    {
      { "Network Essential", Essential.Network },
      { "Plus Essential", Essential.Plus }
    };
        
    private static readonly Dictionary<string, Vital> VitalMappings = new()
    {
      { "Plus Vital", Vital.Plus },
      { "Network Vital", Vital.Network }
    };
    private static readonly Dictionary<string, Alliance> AllianceMappings = new()
    {
      { "Alliance Plus", Alliance.Plus },
      { "Alliance Network", Alliance.Network }
    };

    private static readonly Dictionary<string, @Double> DoubleMappings = new()
    {
      { "Double Plus", @Double.Plus },
      { "Double Network", @Double.Network }
    };
    
    // Reverse Mapping varient ->  category
    private static readonly Dictionary<Choice, string> ChoiceReverseMappings = new()
    {
      { Choice.Network, "Network Choice" },
      { Choice.First, "First Choice" }
    };

    private static readonly Dictionary<Essential, string> EssentialReverseMappings = new()
    {
      { Essential.Network, "Network Essential" },
      { Essential.Plus, "Plus Essential" },
    };
    
    private static readonly Dictionary<Vital, string> VitalReverseMappings = new()
    {
      { Vital.Network, "Network Vital" },
      { Vital.Plus, "Plus Vital" },
    };
    private static readonly Dictionary<string, Alliance> AllianceReverseMappings = new()
    {
      { "Alliance Plus", Alliance.Plus },
      { "Alliance Network", Alliance.Network }
    };

    private static readonly Dictionary<string, @Double> DoubleReverseMappings = new()
    {
      { "Double Plus", @Double.Plus },
      { "Double Network", @Double.Network }
    };

    // Category Patterns Dictionary
    private static readonly Dictionary<string, (string Category, Type EnumType)> CategoryPatterns =
      new()
      {
        { @"Network.*Choice", ("Network Choice", typeof(Choice)) },
        { @"First.*Choice", ("First Choice", typeof(Choice)) },
        { @"^Essential$", ("Essential", typeof(Essential)) },
        { @"Essential.*Plus", ("Essential", typeof(Essential)) },
        { @"Essential.*Network", ("Essential", typeof(Essential)) },
        { @"^Vital$", ("Vital", typeof(Vital)) },
        { @"Vital.*Plus", ("Vital", typeof(Vital)) },
        { @"Vital.*Network", ("Vital", typeof(Vital)) },
        { @"^Alliance$", ("Alliance", typeof(Alliance)) },
        { @"Alliance.*Plus", ("Alliance", typeof(Alliance))},
        { @"Alliance.*Network", ("Alliance", typeof(Alliance))},
        { @"^Double$", ("Double", typeof(Double)) },
        { @"Double.*Plus", ("Double", typeof(Double))},
        { @"Double.*Network", ("Double", typeof(Double))}
      };
    
    // Obtains the category name and enum type from a variant name.
    public static (string? CategoryName, Type? EnumType) GetCategoryInfoFromVariant
      (string variantName)
    {
      foreach (var pattern in CategoryPatterns)
      {
        if (Regex.IsMatch(variantName, pattern.Key, RegexOptions.IgnoreCase))
        {
          return pattern.Value;
        }
      }
      return (null, null);
    }
    
    // Simplified interface when you only need the category name, not the enum type.
    public static string? GetCategoryNameFromVariant(string variantName)
    {
      var (categoryName, _) = GetCategoryInfoFromVariant(variantName);
      return categoryName;
    }

    public static T? GetEnumVariant<T>(string categoryName) where T : struct, System.Enum
    {
      return typeof(T) switch
      {
        var t when t == typeof(Choice) => 
          ChoiceMappings.TryGetValue(categoryName, out var choice) ? (T?)(object)choice : null,
        var t when t == typeof(Essential) => 
          EssentialMappings.TryGetValue(categoryName, out var essential) ? (T?)(object)essential : 
            null,
        var t when t == typeof(Vital) => 
          VitalMappings.TryGetValue(categoryName, out var vital) ? (T?)(object)vital : null,
        var t when t == typeof(Double) => 
          DoubleMappings.TryGetValue(categoryName, out var @double) ? (T)(object)@double : null,
        var t when t == typeof(Alliance) => 
          AllianceMappings.TryGetValue(categoryName, out var alliance) ? (T)(object)alliance : null
      };
    }

    public static object? GetVariantByName(string enumTypeName, string categoryName)
    {
      return enumTypeName switch
      {
        "Choice" => MedicalOptionEnumMapper.GetEnumVariant<Choice>(categoryName),
        "Essential" => MedicalOptionEnumMapper.GetEnumVariant<Essential>(categoryName),
        "vital" => MedicalOptionEnumMapper.GetEnumVariant<Vital>(categoryName),
        _ => null
      };
    }

    public static bool ContainsCategory<T>(string categoryName) where T : struct, System.Enum
    {
      return GetEnumVariant<T>(categoryName).HasValue;
    }
  }
}