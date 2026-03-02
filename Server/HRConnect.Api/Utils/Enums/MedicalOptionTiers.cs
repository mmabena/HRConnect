namespace HRConnect.Api.Utils.Enums
{
  /// <summary>
  /// Defines the tier structure for Choice medical option categories.
  /// This enum represents the different benefit levels available within the Choice category,
  /// providing a hierarchical structure for medical plan options with varying coverage levels.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Business Context:</strong>
  /// The Choice category typically offers flexible medical plan options with different
  /// network access levels and benefit structures. Each tier represents a distinct
  /// combination of coverage features and cost-sharing arrangements.
  /// </para>
  /// 
  /// <para>
  /// <strong>Tier Hierarchy:</strong>
  /// - Network: Standard network access with basic coverage
  /// - First: Enhanced network access with broader provider choice
  /// </para>
  /// 
  /// <para>
  /// <strong>Usage in Validation:</strong>
  /// This enum is used in validation logic to ensure that medical options are properly
  /// categorized and that tier-specific business rules are applied correctly.
  /// </para>
  /// </remarks>
  public enum Choice
  {
    /// <summary>
    /// Represents the Network tier within the Choice category.
    /// This tier provides standard network access with basic coverage options
    /// and typically has lower premium costs compared to higher tiers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Standard provider network access
    /// - Basic coverage benefits
    /// - Lower premium structure
    /// - Defined provider network limitations
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Employees seeking cost-effective coverage with adequate network access.
    /// </para>
    /// </remarks>
    Network,

    /// <summary>
    /// Represents the First tier within the Choice category.
    /// This tier provides enhanced network access with broader provider choice
    /// and typically includes additional coverage benefits at higher premium costs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Enhanced provider network access
    /// - Broader provider choice
    /// - Additional coverage benefits
    /// - Higher premium structure
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Employees seeking maximum provider choice and comprehensive coverage.
    /// </para>
    /// </remarks>
    First
  }

  /// <summary>
  /// Defines the tier structure for Essential medical option categories.
  /// This enum represents the different benefit levels available within the Essential category,
  /// providing a balanced approach between coverage and cost for essential medical services.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Business Context:</strong>
  /// The Essential category offers fundamental medical coverage options designed to provide
  /// necessary healthcare services at affordable price points. Each tier represents a different
  /// level of coverage breadth and network access.
  /// </para>
  /// 
  /// <para>
  /// <strong>Tier Hierarchy:</strong>
  /// - Network: Standard network access with essential coverage
  /// - Plus: Enhanced coverage with additional benefits
  /// </para>
  /// 
  /// <para>
  /// <strong>Value Proposition:</strong>
  /// Essential plans focus on providing necessary medical services while maintaining
  /// affordability for employees and employers.
  /// </para>
  /// </remarks>
  public enum Essential
  {
    /// <summary>
    /// Represents the Network tier within the Essential category.
    /// This tier provides standard network access with essential medical coverage
    /// designed to meet basic healthcare needs at competitive pricing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Standard provider network
    /// - Essential medical coverage
    /// - Competitive pricing
    /// - Focus on necessary healthcare services
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Cost-conscious employees seeking basic medical coverage.
    /// </para>
    /// </remarks>
    Network,

    /// <summary>
    /// Represents the Plus tier within the Essential category.
    /// This tier provides enhanced coverage with additional benefits beyond
    /// the essential level, offering more comprehensive protection at moderate cost.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Enhanced coverage benefits
    /// - Additional medical services
    /// - Moderate premium increase
    /// - Broader service coverage
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Employees seeking additional coverage beyond essential benefits.
    /// </para>
    /// </remarks>
    Plus
  }

  /// <summary>
  /// Defines the tier structure for Vital medical option categories.
  /// This enum represents the different benefit levels available within the Vital category,
  /// providing comprehensive coverage options focused on essential healthcare services.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Business Context:</strong>
  /// The Vital category emphasizes comprehensive coverage for vital healthcare services,
  /// ensuring employees have access to necessary medical care with varying levels of
  /// network access and benefit structures.
  /// </para>
  /// 
  /// <para>
  /// <strong>Tier Hierarchy:</strong>
  /// - Plus: Enhanced coverage with comprehensive benefits
  /// - Network: Standard network access with vital coverage
  /// </para>
  /// 
  /// <para>
  /// <strong>Coverage Focus:</strong>
  /// Vital plans prioritize comprehensive coverage for essential medical services
  /// while offering different network access options.
  /// </para>
  /// </remarks>
  public enum Vital
  {
    /// <summary>
    /// Represents the Plus tier within the Vital category.
    /// This tier provides enhanced coverage with comprehensive benefits designed
    /// to offer extensive protection for vital healthcare services.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Comprehensive coverage benefits
    /// - Enhanced medical services
    /// - Premium tier protection
    /// - Extensive service coverage
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Employees seeking maximum coverage for vital healthcare services.
    /// </para>
    /// </remarks>
    Plus,

    /// <summary>
    /// Represents the Network tier within the Vital category.
    /// This tier provides standard network access with vital medical coverage,
    /// focusing on essential healthcare services within defined provider networks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Standard provider network
    /// - Vital medical coverage
    /// - Essential service focus
    /// - Defined network boundaries
    /// </para>
    /// 
    /// <para>
    /// <strong>Target Audience:</strong>
    /// Employees seeking vital coverage within standard provider networks.
    /// </para>
    /// </remarks>
    Network
  }

  /// <summary>
  /// Defines the tier structure for Double medical option categories.
  /// This enum represents the different benefit levels available within the Double category,
  /// providing specialized coverage options with unique tier characteristics.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Business Context:</strong>
  /// The Double category offers specialized medical coverage options with distinct
  /// tier characteristics. The use of the @ symbol prefix is required to avoid
  /// conflicts with the C# reserved keyword "double".
  /// </para>
  /// 
  /// <para>
  /// <strong>Special Considerations:</strong>
  /// - Uses @ symbol to avoid C# keyword conflicts
  /// - Explicit enum values for serialization consistency
  /// - Warning suppressions for analyzer rules CA1716 and CA1720
  /// </para>
  /// 
  /// <para>
  /// <strong>Tier Structure:</strong>
  /// - Plus: Enhanced coverage tier
  /// - Network: Standard network access tier
  /// </para>
  /// </remarks>
  #pragma warning disable CA1716 // Rename type Double so that it no longer conflicts with reserved keyword
  #pragma warning disable CA1720 // Identifier contains type name
  public enum @Double
  {
    /// <summary>
    /// Represents the Plus tier within the Double category.
    /// This tier provides enhanced coverage with additional benefits and services
    /// beyond the standard network tier, offering comprehensive protection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Enhanced coverage benefits
    /// - Additional medical services
    /// - Premium tier features
    /// - Comprehensive protection
    /// </para>
    /// 
    /// <para>
    /// <strong>Enum Value:</strong>
    /// Explicitly set to 0 for serialization consistency and default behavior.
    /// </para>
    /// </remarks>
    Plus = 0,

    /// <summary>
    /// Represents the Network tier within the Double category.
    /// This tier provides standard network access with essential coverage,
    /// serving as the baseline option within the Double category structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Standard provider network
    /// - Essential coverage benefits
    /// - Baseline tier features
    /// - Network-based access
    /// </para>
    /// 
    /// <para>
    /// <strong>Enum Value:</strong>
    /// Explicitly set to 1 for serialization consistency and ordering.
    /// </para>
    /// </remarks>
    Network = 1
  }
  #pragma warning restore CA1716
  #pragma warning restore CA1720

  /// <summary>
  /// Defines the tier structure for Alliance medical option categories.
  /// This enum represents the different benefit levels available within the Alliance category,
  /// providing collaborative coverage options with partnership-based benefits.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Business Context:</strong>
  /// The Alliance category represents partnership-based medical coverage options,
  /// often involving collaborative arrangements between healthcare providers
  /// and insurance carriers to deliver enhanced benefits and services.
  /// </para>
  /// 
  /// <para>
  /// <strong>Tier Hierarchy:</strong>
  /// - Plus: Enhanced alliance benefits with expanded coverage
  /// - Network: Standard alliance network access
  /// </para>
  /// 
  /// <para>
  /// <strong>Partnership Benefits:</strong>
  /// Alliance plans may offer unique benefits through provider partnerships,
  /// including specialized services and coordinated care options.
  /// </para>
  /// </remarks>
  public enum Alliance
  {
    /// <summary>
    /// Represents the Plus tier within the Alliance category.
    /// This tier provides enhanced alliance benefits with expanded coverage options
    /// and specialized services available through provider partnerships.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Enhanced alliance benefits
    /// - Expanded coverage options
    /// - Specialized partnership services
    /// - Coordinated care features
    /// </para>
    /// 
    /// <para>
    /// <strong>Partnership Advantages:</strong>
    /// Access to specialized providers and coordinated care through alliance partnerships.
    /// </para>
    /// </remarks>
    Plus,

    /// <summary>
    /// Represents the Network tier within the Alliance category.
    /// This tier provides standard alliance network access with essential coverage,
    /// offering baseline benefits through the alliance provider network.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Characteristics:</strong>
    /// - Standard alliance network
    /// - Essential coverage benefits
    /// - Baseline alliance features
    /// - Network-based access
    /// </para>
    /// 
    /// <para>
    /// <strong>Network Benefits:</strong>
    /// Access to alliance provider network with essential coverage options.
    /// </para>
    /// </remarks>
    Network
  }
}