namespace CollectorShop.API;

public static class Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Manager = "Manager";
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int DefaultPageNumber = 1;
    }

    public static class Cache
    {
        public const int ShortCacheDurationSeconds = 60;
        public const int MediumCacheDurationSeconds = 300;
        public const int LongCacheDurationSeconds = 3600;
    }

    public static class Defaults
    {
        public const int FeaturedProductsCount = 8;
        public const int LowStockThreshold = 10;
        public const string DefaultCurrency = "EUR";
    }

    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found";
        public const string InvalidCredentials = "Invalid email or password";
        public const string AccountLocked = "Account is locked. Please try again later.";
        public const string CustomerProfileNotFound = "Customer profile not found";
        public const string CartEmpty = "Cart is empty";
        public const string InsufficientStock = "Insufficient stock for product";
        public const string InvalidToken = "Invalid token";
        public const string TokenRevoked = "Token already revoked or expired";
    }
}
