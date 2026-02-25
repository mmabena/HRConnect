namespace HRConnect.Api.Utils.Enums
{
  public enum Choice
  {
    Network,
    First
  }
  public enum FirstChoice
  {
    //Example: FirstChoice1, FirstChoice2, FirstChoice3, FirstChoice4, FirstChoice5
  }
  public enum Essential
  {
    Network,
    Plus
  }
  public enum Vital
  {
    Plus,
    Network
  }
  #pragma warning disable CA1716 // Rename type Double so that it no longer conflicts with reserved keyword
  #pragma warning disable CA1720 // Identifier contains type name
  public enum @Double
  {
    Plus = 0,
    Network = 1
  }
  #pragma warning restore CA1716
  #pragma warning restore CA1720
  public enum Alliance
  {
    Plus,
    Network
  }
  
}