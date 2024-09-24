namespace OceanTechLevel1.Services
{
    public class PagingService
    {
        public List<T> GetPagedResult<T>(IQueryable<T> query, int page, int pageSize, out int totalPages)
        {
            int totalRecords = query.Count();
            totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalRecords) / pageSize));
            int recordsToSkip = (page - 1) * pageSize;

            return query.Skip(recordsToSkip).Take(pageSize).ToList();
        }
    }

}
