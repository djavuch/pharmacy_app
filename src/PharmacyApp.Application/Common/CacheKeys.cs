using PharmacyApp.Application.Common.Pagination;

namespace PharmacyApp.Application.Common;

public static class CacheKeys
{
    public static class Categories
    {
        private const string Base = "categories";
        
        public const string AllPrefix = $"{Base}:all";

        public static string All(int pageIndex, int pageSize) => $"{AllPrefix}:{pageIndex}:{pageSize}";

        public static string ById(int id) => $"{Base}:id:{id}";

        public static string ByName(string name) => $"{Base}:name:{name}";
    }
    
    public static class Products
    {
        private const string Base = "products";
        
        public static string All(int version, QueryParams q)
            => $"{Base}_v{version}_{q.PageIndex}_{q.PageSize}_{q.FilterOn}_{q.FilterQuery}_{q.SortBy}_{q.IsAscending}";

        public static string ById(int version, int id)
            => $"{Base}_v{version}:id:{id}";
    }

    public static class Users
    {
        private const string BaseList = "users";
        private const string BaseSingle = "user";
        
        public static string Profile(int version, string userId) => $"{BaseSingle}_v{version}:profile:{userId}";
        
        public static string Orders(int version, string userId, QueryParams q)
            => $"{BaseSingle}_v{version}:orders:{userId}:{q.PageIndex}:{q.PageSize}";
        
        public static string Reviews(int version, string userId, ReviewQueryParams q)
            => $"{BaseSingle}_v{version}:reviews:{userId}:{q.PageIndex}:{q.PageSize}";
        
        public static string AllPaged(int version, QueryParams q)
            => $"{BaseList}_v{version}:all:{q.PageIndex}:{q.PageSize}:{q.FilterOn}:{q.FilterQuery}:{q.SortBy}:{q.IsAscending}";
    }

    public static class Discounts
    {
        private const string Base = "discounts";
        
        public const string All = $"{Base}:all";
        public static string Active => $"{Base}:active";

        public static string ById(Guid id) => $"{Base}:id:{id}";
        
        public static string ByProduct(int productId) => $"{Base}:product:{productId}";
        
        public static string ByCategory(int categoryId) => $"{Base}:category:{categoryId}";
    }

    public static class PromoCodes
    {
        private const string Base = "promocodes";
        
        public const string All = $"{Base}:all";
        public static string Active => $"{Base}:active";
        
        public static string ById(Guid id) => $"{Base}:id:{id}";
        
        public static string ByCode(string code) => $"{Base}:code:{code}";
    }

    public static class Reviews
    {
        private const string Base = "reviews";
        
        public static string ByProduct(int productId, QueryParams q)
            => $"{Base}:product:{productId}:{q.PageIndex}:{q.PageSize}";
    }

    public static class Bonus
    {
        private const string Base = "bonus";
        
        public const string Settings = $"{Base}:settings";
        
        public static string ByUser(string userId) => $"{Base}:user:{userId}";
    }
    
    public static class Wishlists
    {
        private const string Base = "wishlists";
        
        public static string ByUser(string userId) => $"{Base}:user:{userId}";
    }
}