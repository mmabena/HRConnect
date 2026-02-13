namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Utils.Enums;
  
  public static class MedicalOptionEnumMapper
  {
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
    
        
    public static T? GetEnumVariant<T>(string categoryName) where T : struct, Enum
    {
      return typeof(T) switch
      {
        var t when t == typeof(Choice) => 
          ChoiceMappings.TryGetValue(categoryName, out var choice) ? (T?)(object)choice : null,
        var t when t == typeof(Essential) => 
          EssentialMappings.TryGetValue(categoryName, out var essential) ? (T?)(object)essential : null,
        var t when t == typeof(Vital) => 
          VitalMappings.TryGetValue(categoryName, out var vital) ? (T?)(object)vital : null,
      };
    }
        
    public static bool ContainsCategory<T>(string categoryName) where T : struct, Enum
    {
      return GetEnumVariant<T>(categoryName).HasValue;
    }
  }
}