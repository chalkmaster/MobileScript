using System;

namespace Seculus.MobileScript.Core.Helpers
{
    public static class MigratorHelper
    {
        private const string CompanyTableName = "companies";

        public static object GetCompanyIdValue(string tableName, string[] columns, object[] values)
        {
            var columnName = tableName.Equals(CompanyTableName, StringComparison.InvariantCultureIgnoreCase) ? "Id" : "CompanyId";

            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i].ToLowerInvariant().Equals(columnName.ToLowerInvariant()))
                {
                    return values[i];
                }
            }

            return null;
        }

        //public static object GetCompanyIdValue(Filter filter)
        //{
        //    var columnName = tableName.Equals(CompanyTableName, StringComparison.InvariantCultureIgnoreCase) ? "Id" : "CompanyId";

        //    for (var i = 0; i < columns.Length; i++)
        //    {
        //        if (columns[i].ToLowerInvariant().Equals(columnName.ToLowerInvariant()))
        //        {
        //            return values[i];
        //        }
        //    }

        //    return null;
        //}  
    }
}