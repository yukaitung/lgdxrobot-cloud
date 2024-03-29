namespace LGDXRobot2Cloud.API.Services
{
  public class PaginationMetadata
  {
    public int ItemCount { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }

    public PaginationMetadata(int itemCount, int pageSize, int currentPage)
    {
      ItemCount = itemCount;
      PageSize = pageSize;
      CurrentPage = currentPage;
      PageCount = (int)Math.Ceiling(itemCount / (double)pageSize);
    }
  }
}