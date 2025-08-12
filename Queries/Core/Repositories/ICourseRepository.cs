using System.Collections.Generic;

namespace Queries.Core.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        // Inhertis all methods from IRepository<Course> and adds two more specific methods for Course entity
        IEnumerable<Course> GetTopSellingCourses(int count);
        IEnumerable<Course> GetCoursesWithAuthors(int pageIndex, int pageSize);
    }
}
