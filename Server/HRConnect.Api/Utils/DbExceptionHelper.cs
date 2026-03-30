namespace HRConnect.Api.Utils
{
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public static class DbExceptionHelper
    {
        public static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlException)
            {
                return sqlException.Number == 2601
                    || sqlException.Number == 2627;
            }

            return false;
        }
    }
}
