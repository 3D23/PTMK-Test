namespace PTMK_Test.Application.Common
{
    public static class PaginationConstants
    {
        public const short MaxPageSize = 1024;
        public const short DefaultPageSize = 128;
        public const short DefaultPageNumber = 1;
    }

    public readonly struct PaginationParameters
    {
        public int PageNumber { get; }
        public int PageSize { get; }

        public PaginationParameters(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1
                ? PaginationConstants.DefaultPageNumber
                : pageNumber;

            if (pageSize < 1)
                PageSize = PaginationConstants.DefaultPageSize;
            else if (pageSize > PaginationConstants.MaxPageSize)
                PageSize = PaginationConstants.MaxPageSize;
            else
                PageSize = pageSize;
        }
    }
}
