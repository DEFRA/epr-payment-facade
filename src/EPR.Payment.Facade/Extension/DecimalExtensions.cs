namespace EPR.Payment.Facade.Extension
{
    public static class DecimalExtensions
    {
        public static bool IsNullableDecimal(this decimal decimalProperty)
        {
            Type type = decimalProperty.GetType();

            return Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type) == typeof(decimal);
        }

        public static bool IsNullableDecimal(this decimal? decimalProperty)
        {
            Type type = decimalProperty.GetType();

            return Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type) == typeof(decimal);
        }

        public static bool IsNullableDecimalSimple(this decimal decimalProperty)
        {
            return false;
        }

        public static bool IsNullableDecimalSimple(this decimal? decimalProperty)
        {
            return true;
        }
    }
}
