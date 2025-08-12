using Queries.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity; // For Include method

namespace Queries.Persistence.Repositories
{
    // Inhertis from Repository<TEntity> since we don't want to repeat all of that code
    // Implements ICourseRepository interface which contains methods that are specific to Course entity
    public class CourseRepository : Repository<Course>, ICourseRepository 
    { 
        // Constructor that takes a PlutoContext and passes it to the base Repository class constructor (Repository.cs)
        public CourseRepository(PlutoContext context) 
            : base(context) { }

        // Property that casts the Context to PlutoContext type for simplicity
        public PlutoContext PlutoContext
        {
            get { return Context as PlutoContext; } // casting Context inherited from Repository<TEntity> to PlutoContext
        }

        // This method returns the "top-selling" courses based on the FullPrice property, actually we order descending by FullPrice and take the specified count (top N)
        public IEnumerable<Course> GetTopSellingCourses(int count)
        {
            return PlutoContext.Courses.OrderByDescending(c => c.FullPrice).Take(count).ToList();
        }

        // This method eager loads the courses and their authors using Include method from Entity Framework
        public IEnumerable<Course> GetCoursesWithAuthors(int pageIndex, int pageSize)
        {
            return PlutoContext.Courses
                .Include(c => c.Author)
                .OrderBy(c => c.Name)
                .Skip((pageIndex - 1) * pageSize) // Skip the previous pages based on pageIndex and pageSize
                .Take(pageSize) // Take the specified page size
                .ToList();
        }
    }
}
