namespace EPR.Payment.Facade.Extension
{
    public static class DecimalExtensions
    {
        public static bool IsNullableDecimal(this decimal decimalToCheck)
        {
            Type type = decimalToCheck.GetType();

            return Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type) == typeof(decimal);
        }

        public static bool IsNullableDecimal(this decimal? decimalToCheck)
        {
            Type type = decimalToCheck.GetType();

            return Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type) == typeof(decimal);
        }

        public static bool IsNullableDecimalSimple(this decimal decimalToCheck)
        {
            return false;
        }

        public static bool IsNullableDecimalSimple(this decimal? decimalToCheck)
        {
            return true;
        }
    }
}
