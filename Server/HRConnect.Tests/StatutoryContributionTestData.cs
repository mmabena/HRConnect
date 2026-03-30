namespace HRConnect.Tests
{
  public static class StatutoryContributionTestData
  {
    public static TheoryData<decimal, decimal> SdlAmountTestData =>
       new TheoryData<decimal, decimal>
   {
     {35000m,350m},
     {1771200m,17712m},
   };
    public static TheoryData<decimal, decimal> UifForSalaryBelowCap =>
 new TheoryData<decimal, decimal>
    {
    {7000m,70m}
    };

    public static TheoryData<decimal, decimal, decimal> UifEmployeeAndEmployerTestData =>
        new TheoryData<decimal, decimal, decimal>
    {
     {5000m,50m,50m},
     {1771200m,177.12m,177.12m},
     {17712m,177.12m,177.12m}
    };
    public static TheoryData<decimal, decimal, decimal> UifAboveCapTestData => new TheoryData<decimal, decimal, decimal>
    {
      {2000000m,177.12m,177.12m}
    };
  }
}