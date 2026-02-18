namespace HRConnect.Api.Utils.Enums.MedicalOption
{
 
  public static class MedicalOptionValidator
  {
    //TODO:
    //1. Validate Entity Count against bulkPayload Count
    //2. Validate if each ID is valid from the bulk payload
    //3. Validate Salary Bracket for the following:
    //   -=> Alliance and Double must not have a salary bracket cap
    //   -=> Updates can only be done between November and December (31st 59:59:59)
    //   -=> There should be no gaps in between the salary ranges (they should be apart by half a
    //       cent/ or a rand)
    //   -=> There should be no overlapping salary bracket caps within the same category
    //       (where one option overlaps 1+ other options in the same category)
    //   -=> Only perform an update if all the entities within the payload exist in the database
    //       (only on valid ID numbers) -- out of the scope of this file's operation
    //4. Do cross-checks on the contribution values if the payload's value balance out
    //   (will need to cater for nulls as not all options have RISK + MSA = Total Contrib)
    
 
  } 
}