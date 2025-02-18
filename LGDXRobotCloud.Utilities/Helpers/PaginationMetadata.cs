namespace LGDXRobotCloud.Utilities.Helpers;

public class PaginationHelper(int itemCount, int currentPage, int pageSize)
{
  public int ItemCount { get; set; } = itemCount;
  public int PageCount { get; set; } = (int)Math.Ceiling(itemCount / (double)pageSize);
  public int PageSize { get; set; } = pageSize;
  public int CurrentPage { get; set; } = currentPage;
}