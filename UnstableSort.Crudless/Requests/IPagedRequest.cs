namespace UnstableSort.Crudless.Requests
{
    public interface IPagedRequest : ICrudlessRequest
    {
        int PageNumber { get; set; }

        int PageSize { get; set; }
    }
}
