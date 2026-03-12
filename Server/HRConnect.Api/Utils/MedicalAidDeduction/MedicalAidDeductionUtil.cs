namespace HRConnect.Api.Utils.MedicalAidDeduction
{
  public static class MedicalAidDeductionUtil
  {
    public static bool EffectDateBeforeMidMonth(DateTime queryDate)
    {

      return queryDate.Day <= 15;
    }


  }
}

