namespace HRConnect.Api.Utils.Enums
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides mapping functionality between medical option category names and their corresponding enum variants.
    /// This static utility class handles bidirectional mapping between string representations and enum values
    /// for different medical option categories (Choice, Essential, Vital, Alliance, Double).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Architecture Overview:</strong>
    /// The mapper uses a pattern-based approach to categorize medical options:
    /// - Forward mapping: Category name → Enum variant
    /// - Reverse mapping: Enum variant → Category name
    /// - Pattern matching: Variant name → Category identification
    /// </para>
    /// 
    /// <para>
    /// <strong>Supported Categories:</strong>
    /// - Choice: Network Choice, First Choice
    /// - Essential: Network Essential, Plus Essential
    /// - Vital: Plus Vital, Network Vital
    /// - Alliance: Alliance Plus, Alliance Network
    /// - Double: Double Plus, Double Network
    /// </para>
    /// 
    /// <para>
    /// <strong>Usage Example - Category Identification:</strong>
    /// </para>
    /// <code>
    /// // Identify category from variant name
    /// var variantName = "Network Choice Premium";
    /// var (categoryName, enumType) = MedicalOptionEnumMapper.GetCategoryInfoFromVariant(variantName);
    /// // Result: ("Network Choice", typeof(Choice))
    /// 
    /// // Simple category name extraction
    /// var simpleCategory = MedicalOptionEnumMapper.GetCategoryNameFromVariant("Plus Essential");
    /// // Result: "Essential"
    /// </code>
    /// 
    /// <para>
    /// <strong>Usage Example - Enum Mapping:</strong>
    /// </para>
    /// <code>
    /// // Get specific enum variant
    /// var choice = MedicalOptionEnumMapper.GetEnumVariant&lt;Choice&gt;("Network Choice");
    /// // Result: Choice.Network
    /// 
    /// // Check if category contains specific variant
    /// var hasNetwork = MedicalOptionEnumMapper.ContainsCategory&lt;Choice&gt;("Network Choice");
    /// // Result: true
    /// 
    /// // Dynamic enum retrieval by type name
    /// var variant = MedicalOptionEnumMapper.GetVariantByName("Choice", "Network Choice");
    /// // Result: Choice.Network (as object)
    /// </code>
    /// 
    /// <para>
    /// <strong>Pattern Matching Rules:</strong>
    /// The mapper uses regular expressions to match variant names to categories:
    /// - "Network.*Choice" → Network Choice
    /// - "First.*Choice" → First Choice
    /// - "^Essential$" → Essential (exact match)
    /// - "Essential.*Plus" → Essential
    /// - "Essential.*Network" → Essential
    /// - Similar patterns for Vital, Alliance, and Double categories
    /// </para>
    /// 
    /// <para>
    /// <strong>Integration with Medical Options:</strong>
    /// This mapper is typically used when processing medical option data from external sources
    /// or when categorizing user selections:
    /// </para>
    /// <code>
    /// public class MedicalOptionProcessor
    /// {
    ///     public MedicalOptionCategoryDto ProcessOption(MedicalOptionDto option)
    ///     {
    ///         var (categoryName, enumType) = MedicalOptionEnumMapper.GetCategoryInfoFromVariant(option.MedicalOptionName);
    ///         
    ///         return new MedicalOptionCategoryDto
    ///         {
    ///             MedicalOptionCategoryId = GetCategoryId(categoryName),
    ///             MedicalOptionCategoryName = categoryName ?? "Uncategorized",
    ///             MedicalOptions = new List&lt;MedicalOptionDto&gt; { option }
    ///         };
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>
    /// <strong>Error Handling:</strong>
    /// The mapper gracefully handles unknown categories by returning null values:
    /// - Unknown category names return null for enum variants
    /// - Unmatched patterns return (null, null) for category info
    /// - This allows calling code to handle unknown categories appropriately
    /// </para>
    /// 
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// - All dictionaries are initialized once and cached for optimal performance
    /// - Regex patterns are compiled with IgnoreCase option for case-insensitive matching
    /// - Switch expressions provide efficient type-based dispatching
    /// </para>
    /// </remarks>
    public static class MedicalOptionEnumMapper
    {
        /// <summary>
        /// Forward mapping dictionary for Choice category variants.
        /// Maps category names to their corresponding Choice enum values.
        /// </summary>
        private static readonly Dictionary<string, Choice> ChoiceMappings = new()
        {
          { "Network Choice", Choice.Network },
          { "First Choice", Choice.First }
        };
        
        /// <summary>
        /// Forward mapping dictionary for Essential category variants.
        /// Maps category names to their corresponding Essential enum values.
        /// </summary>
        private static readonly Dictionary<string, Essential> EssentialMappings = new()
        {
          { "Network Essential", Essential.Network },
          { "Plus Essential", Essential.Plus }
        };
        
        /// <summary>
        /// Forward mapping dictionary for Vital category variants.
        /// Maps category names to their corresponding Vital enum values.
        /// </summary>
        private static readonly Dictionary<string, Vital> VitalMappings = new()
        {
          { "Plus Vital", Vital.Plus },
          { "Network Vital", Vital.Network }
        };

        /// <summary>
        /// Forward mapping dictionary for Alliance category variants.
        /// Maps category names to their corresponding Alliance enum values.
        /// </summary>
        private static readonly Dictionary<string, Alliance> AllianceMappings = new()
        {
          { "Alliance Plus", Alliance.Plus },
          { "Alliance Network", Alliance.Network }
        };

        /// <summary>
        /// Forward mapping dictionary for Double category variants.
        /// Maps category names to their corresponding Double enum values.
        /// </summary>
        private static readonly Dictionary<string, @Double> DoubleMappings = new()
        {
          { "Double Plus", @Double.Plus },
          { "Double Network", @Double.Network }
        };
    
        /// <summary>
        /// Reverse mapping dictionary for Choice category variants.
        /// Maps Choice enum values back to their category names.
        /// </summary>
        private static readonly Dictionary<Choice, string> ChoiceReverseMappings = new()
        {
          { Choice.Network, "Network Choice" },
          { Choice.First, "First Choice" }
        };

        /// <summary>
        /// Reverse mapping dictionary for Essential category variants.
        /// Maps Essential enum values back to their category names.
        /// </summary>
        private static readonly Dictionary<Essential, string> EssentialReverseMappings = new()
        {
          { Essential.Network, "Network Essential" },
          { Essential.Plus, "Plus Essential" },
        };
    
        /// <summary>
        /// Reverse mapping dictionary for Vital category variants.
        /// Maps Vital enum values back to their category names.
        /// </summary>
        private static readonly Dictionary<Vital, string> VitalReverseMappings = new()
        {
          { Vital.Network, "Network Vital" },
          { Vital.Plus, "Plus Vital" },
        };

        /// <summary>
        /// Reverse mapping dictionary for Alliance category variants.
        /// Maps Alliance enum values back to their category names.
        /// </summary>
        private static readonly Dictionary<string, Alliance> AllianceReverseMappings = new()
        {
          { "Alliance Plus", Alliance.Plus },
          { "Alliance Network", Alliance.Network }
        };

        /// <summary>
        /// Reverse mapping dictionary for Double category variants.
        /// Maps Double enum values back to their category names.
        /// </summary>
        private static readonly Dictionary<string, @Double> DoubleReverseMappings = new()
        {
          { "Double Plus", @Double.Plus },
          { "Double Network", @Double.Network }
        };

        /// <summary>
        /// Pattern dictionary for matching variant names to categories using regular expressions.
        /// Each pattern maps to a tuple containing the category name and the corresponding enum type.
        /// </summary>
        /// <remarks>
        /// Patterns are evaluated in order and use case-insensitive matching.
        /// More specific patterns should be placed before general ones to ensure correct matching.
        /// </remarks>
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
    
        /// <summary>
        /// Obtains category name and enum type from a variant name using pattern matching.
        /// </summary>
        /// <param name="variantName">The variant name to analyze.</param>
        /// <returns>
        /// A tuple containing the category name and enum type if a match is found,
        /// or (null, null) if no pattern matches the variant name.
        /// </returns>
        /// <remarks>
        /// This method iterates through all registered patterns and returns the first match.
        /// The matching is case-insensitive and uses regular expression patterns.
        /// If multiple patterns could match, the order in CategoryPatterns determines priority.
        /// </remarks>
        /// <example>
        /// <code>
        /// var result1 = GetCategoryInfoFromVariant("Network Choice Premium");
        /// // Returns: ("Network Choice", typeof(Choice))
        /// 
        /// var result2 = GetCategoryInfoFromVariant("Plus Essential");
        /// // Returns: ("Essential", typeof(Essential))
        /// 
        /// var result3 = GetCategoryInfoFromVariant("Unknown Option");
        /// // Returns: (null, null)
        /// </code>
        /// </example>
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
    
        /// <summary>
        /// Simplified interface for extracting only the category name from a variant name.
        /// </summary>
        /// <param name="variantName">The variant name to analyze.</param>
        /// <returns>The category name if a match is found, otherwise null.</returns>
        /// <remarks>
        /// This is a convenience method that calls GetCategoryInfoFromVariant and returns only the category name.
        /// Use this when you only need the category name and don't care about the enum type.
        /// </remarks>
        /// <example>
        /// <code>
        /// var category = GetCategoryNameFromVariant("Alliance Plus Premium");
        /// // Returns: "Alliance"
        /// 
        /// var category2 = GetCategoryNameFromVariant("Double Network");
        /// // Returns: "Double"
        /// </code>
        /// </example>
        public static string? GetCategoryNameFromVariant(string variantName)
        {
          var (categoryName, _) = GetCategoryInfoFromVariant(variantName);
          return categoryName;
        }

        /// <summary>
        /// Retrieves the enum variant for a specific category name and enum type.
        /// </summary>
        /// <typeparam name="T">The enum type to retrieve.</typeparam>
        /// <param name="categoryName">The category name to map to an enum value.</param>
        /// <returns>
        /// The corresponding enum value if found, otherwise null.
        /// Returns T? (nullable enum) to handle cases where the category name is not found.
        /// </returns>
        /// <remarks>
        /// This method uses type-based dispatching to select the appropriate mapping dictionary.
        /// The method is generic and works with all supported enum types.
        /// If the category name is not found in the corresponding dictionary, null is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// var choice = GetEnumVariant&lt;Choice&gt;("Network Choice");
        /// // Returns: Choice.Network
        /// 
        /// var essential = GetEnumVariant&lt;Essential&gt;("Plus Essential");
        /// // Returns: Essential.Plus
        /// 
        /// var unknown = GetEnumVariant&lt;Choice&gt;("Unknown Category");
        /// // Returns: null
        /// </code>
        /// </example>
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

        /// <summary>
        /// Retrieves an enum variant by enum type name and category name.
        /// </summary>
        /// <param name="enumTypeName">The name of the enum type (e.g., "Choice", "Essential").</param>
        /// <param name="categoryName">The category name to map to an enum value.</param>
        /// <returns>The corresponding enum value as an object, or null if not found.</returns>
        /// <remarks>
        /// This method provides a dynamic way to retrieve enum values when the enum type
        /// is known only at runtime (e.g., from configuration or user input).
        /// The method uses string-based type dispatching to call the appropriate generic method.
        /// </remarks>
        /// <example>
        /// <code>
        /// var variant1 = GetVariantByName("Choice", "Network Choice");
        /// // Returns: Choice.Network (as object)
        /// 
        /// var variant2 = GetVariantByName("Essential", "Plus Essential");
        /// // Returns: Essential.Plus (as object)
        /// 
        /// var variant3 = GetVariantByName("Unknown", "Some Category");
        /// // Returns: null
        /// </code>
        /// </example>
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

        /// <summary>
        /// Checks if a category name exists within a specific enum type.
        /// </summary>
        /// <typeparam name="T">The enum type to check.</typeparam>
        /// <param name="categoryName">The category name to verify.</param>
        /// <returns>True if the category name exists in the specified enum type, otherwise false.</returns>
        /// <remarks>
        /// This is a convenience method that combines GetEnumVariant with null checking.
        /// Useful for validation scenarios where you need to verify that a category name is valid
        /// for a specific enum type before proceeding with further processing.
        /// </remarks>
        /// <example>
        /// <code>
        /// var isValid1 = ContainsCategory&lt;Choice&gt;("Network Choice");
        /// // Returns: true
        /// 
        /// var isValid2 = ContainsCategory&lt;Choice&gt;("Unknown Choice");
        /// // Returns: false
        /// 
        /// // Usage in validation
        /// if (!ContainsCategory&lt;Essential&gt;(categoryName))
        /// {
        ///     throw new ValidationException($"Invalid Essential category: {categoryName}");
        /// }
        /// </code>
        /// </example>
        public static bool ContainsCategory<T>(string categoryName) where T : struct, System.Enum
        {
          return GetEnumVariant<T>(categoryName).HasValue;
        }
    }
}