namespace UnstableSort.Crudless.Requests
{
    public interface IPagedRequest
    {
        int PageNumber { get; set; }

        int PageSize { get; set; }
    }
}
