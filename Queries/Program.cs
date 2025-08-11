using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace Queries
{
    class Program
    {
        static void LinqSintax(PlutoContext context)
        {
            // Restriction: Filtering data
            var coursesLevOne = from c in context.Courses
                                where c.Level == 1 && c.Author.Id == 1 // restriction by level and author
                                select c;
            // Ordering: Sorting data by their level
            var coursesLevOrd = from c in context.Courses
                                where c.Author.Id == 1
                                orderby c.Level descending, c.Name // ordering by level descending and then by name
                                select c;
            // Projection: Selecting specific fields
            var coursesProj = from c in context.Courses
                              where c.Author.Id == 1
                              orderby c.Level, c.Name
                              select new
                              {
                                  Name = c.Name,
                                  Author = c.Author.Name
                              };
            // Grouping: Grouping courses by their level without using aggregation
            var coursesGrouped = from c in context.Courses
                                 group c by c.Level
                               into g
                                 select g;
            foreach (var group in coursesGrouped)
            {
                Console.WriteLine($"\tLevel: {group.Key} | Course count: {group.Count()}"); // as you see there's no aggregation
                foreach (var course in group)
                {
                    Console.WriteLine($"{course.Name}");
                }
            }
            // Joining - Join: Joining courses with authors
            var coursesJoinNavProp = from c in context.Courses // we don't need to use join, we can do so by using navigation properties
                                     select new                 // this is automaically translated to a join by EF
                                     {
                                         CourseName = c.Name,
                                         AuthorName = c.Author.Name
                                     };
            var coursesJoin = from c in context.Courses // here we use explicit join
                              join a in context.Authors on c.Author.Id equals a.Id // they're joined by c.Author.Id with a.Id
                              select new
                              {
                                  CourseName = c.Name,
                                  AuthorName = a.Name
                              };
            // Joining - Group Join: Grouping courses by their author
            var coursesJoinGroup = from a in context.Authors
                                   join c in context.Courses on a.Id equals c.AuthorId into g // into makes it a group join
                                   select new
                                   {
                                       AuthorName = a.Name,
                                       Courses = g.Count() // g is a collection of courses for each author and we count them
                                   };
            Console.WriteLine("\n\tCount of courses by Author");
            foreach (var row in coursesJoinGroup)
            {
                Console.WriteLine($"Author: {row.AuthorName} | Courses: {row.Courses}");
            }
            // Joining - Cross Join: Cross join between authors and courses
            var coursesCrossJoin = from a in context.Authors
                                   from c in context.Courses // this is a cross join, every author with every course
                                   select new
                                   {
                                       AuthorName = a.Name, // every author
                                       CourseName = c.Name // every course
                                   };
            Console.WriteLine("\n\tCross Join Authors x Courses");
            foreach (var row in coursesCrossJoin)
            {
                Console.WriteLine($"{row.AuthorName} - {row.CourseName}");
            }
        }

        static void ExtensionMethods(PlutoContext context)
        {
            // LINQ Extension Methods

            // Restriction: Filtering data
            var coursesLvlOne = context.Courses.Where(c => c.Level == 1); // filtering courses by level 1
            Console.WriteLine("\tRestriction/Filtering - Courses filtered by Level 1");
            foreach (var course in coursesLvlOne)
            {
                Console.WriteLine($"{course.Name}");
            }

            // Ordering: Sorting data by their level
            var coursesLvlOneOrdered = context.Courses
                .Where(c => c.Level == 1)
                .OrderBy(c => c.Name); // filtering courses by level 1 and ordering them by name

            Console.WriteLine("\n\tOrdering - Courses ordered by Level and then by Full price");
            var coursesOrdered = context.Courses
                .OrderBy(c => c.Level)
                .ThenBy(c => c.FullPrice); // ordering courses by level and then by full price
            foreach (var course in coursesOrdered)
            {
                Console.WriteLine($"Lv.{course.Level} - {course.Name} - ${course.FullPrice}");
            }

            Console.WriteLine("\n\tOrdering - Courses ordered by Level and then by Full price (Descending)");
            var coursesOrderedDesc = context.Courses
                .OrderBy(c => c.Level)
                .ThenByDescending(c => c.FullPrice); // ordering courses by level and then by full price descending
            foreach (var course in coursesOrderedDesc)
            {
                Console.WriteLine($"Lv.{course.Level} - {course.Name} - ${course.FullPrice}");
            }

            // Projection: Selecting specific fields
            Console.WriteLine("\n\tProjection - List of lists of tags");
            var coursesListOfListTags = context.Courses
                .OrderByDescending(c => c.Level)
                .ThenByDescending(c => c.Name)
                .Select(c => c.Tags); // list of list of tags (each course has a list of tags, so we end up with a list of lists)

            foreach (var courseListTag in coursesListOfListTags) // course is a list of tags
            {
                foreach (var courseTag in courseListTag) // iterating over each tag in the list
                {
                    Console.WriteLine($"{courseTag.Name}"); // printing tag name
                }
            }

            Console.WriteLine("\n\tProjection - List of lists of tags (Flatten)");
            // if you want to flatten the list of lists into a single list, you can use SelectMany
            var listTagsFlatten = context.Courses
                .OrderByDescending(c => c.Level)
                .ThenByDescending(c => c.Name)
                .SelectMany(c => c.Tags); // list of tags (flattened)

            foreach (var tag in listTagsFlatten)
            {
                Console.WriteLine($"{tag.Name}");
            }
            //Lesson: when you're using the Select method if the target property is a list/collection, you would end up with a list of lists/collections

            //We can also use distinct to get unique values
            Console.WriteLine("\n\tProjection - List of lists of tags (Flatten and Distinct)");
            var listTagsFlattenDisct = context.Courses
                .OrderByDescending(c => c.Level)
                .ThenByDescending(c => c.Name)
                .SelectMany(c => c.Tags)
                .Distinct(); // makes the list of tags unique

            foreach (var tag in listTagsFlattenDisct)
            {
                Console.WriteLine($"{tag.Name}");
            }

            // Grouping: Grouping courses by their level without using aggregation
            var coursesGroupedByLevel = context.Courses.GroupBy(c => c.Level); // grouping courses by their level

            Console.Write("\n\tGroupung courses by level");
            foreach (var coursesLevel in coursesGroupedByLevel)
            {
                Console.WriteLine($"\nLevel {coursesLevel.Key}");

                foreach (var course in coursesLevel)
                {
                    Console.WriteLine("\t" + course.Name);
                }
            }

            // Joining

            // Inner Join: Joining courses with authors (imagine we don't have navigation property)
            var coursesJoin = context.Courses.Join( // primary collection which we want to join
                context.Authors, // secondary collection which we eant to join with
                c => c.AuthorId, // Courses key: for each course we take the AuthorId value
                a => a.Id, // Authors key: for each author we take the Id value
                (course, author) => new // results selector: how we want to join those entities
                {
                    CourseName = course.Name,
                    AuthorName = author.Name
                });

            /*
             *          Equivalent to: 
             * SELECT c.Name AS CourseName, a.Name AS AuthorName
             * FROM Courses c
             * INNER JOIN Authors a
             * ON c.AuthorId = a.Id;
            */

            // Group Join: Grouping authors with their courses
            var authorsGroupJoin = context.Authors.GroupJoin( // 1. FROM Authors a
                context.Courses, // 2. JOIN Courses c
                a => a.Id, // 3. ON a.Id
                c => c.AuthorId, // 4. = ON c.AuthorId
                (a, c) => // 5. SELECT a.Name as AuthorName, c.Count() as Courses
                new
                {
                    AuthorName = a.Name,
                    Courses = c.Count()
                });

            // Cross Join: Cross join between authors and courses - every combination of every object in these two collections
            // we don't have a direct method for cross join, but we can use SelectMany to achieve the same result
            var crossJoin = context.Authors.SelectMany(
                a => context.Courses,
                (author, course) =>
                new
                {
                    AuthorName = author.Name,
                    CourseName = course.Name
                });
        }

        static void AdditionalExtensionMethods(PlutoContext context)
        {
            // Additional Methods

            // Skip and Take: Skipping and taking records
            var firstFiveRecords = context.Courses
                .OrderBy(c => c.Id)
                .Skip(5) // Skipping the first 5 records
                .Take(5); // Taking the next 5 records after skipping
            Console.WriteLine("\n\tFirst 5 records after skipping 5:");
            foreach (var course in firstFiveRecords)
            {
                Console.WriteLine($"Course {course.Id}: {course.Name}");
            }

            // Element operators: First, FirstOrDefault, Single, SingleOrDefault

            // first: returns the first element of a sequence
            var first = context.Courses.First(); // Returns the first course in the collection
            Console.WriteLine($"\n\tFirst course in the collection:\n {first.Name}");

            var firstOrdered = context.Courses.OrderByDescending(c => c.Level).First(); // Returns the first course ordered by level
            Console.WriteLine($"\n\tFirst course in the collection ordered by level descending:\n {firstOrdered.Name}");


            // firstOrDefault: returns the first element of a sequence, or a default value if no element is found
            var firstOtDefault = context.Courses.OrderBy(c => c.Level).FirstOrDefault(); // this returns null if the collection is empty

            // this returns null if no course matches the condition
            var firstOtDefaultCond = context.Courses.OrderBy(c => c.Level).FirstOrDefault(c => c.FullPrice > 500);

            Console.WriteLine($"\n\tFirstOrDefault course in the collection ordered by level:\n {firstOtDefault.Name}");

            if (firstOtDefaultCond != null)
                Console.WriteLine($"\n\tFirstOrDefault course in the collection ordered by level with full price greather than 500:\n {firstOtDefaultCond.Name}");
            else
                Console.WriteLine($"\n\tNo course found with full price greather than 500");

            // diference between First and FirstOrDefault: First throws an exception if the collection is empty, FirstOrDefault returns
            // null (or default value for value types)

            // last: returns the last element of a sequence
            var array = new int[] { 4, 3, 1, 2, 9, 0 };
            var emptyArray = new int[] { };

            var last = array.Last(); // Returns the last course in the collection

            //var last = context.Courses.Last(); // for DBs throws an exception, since we don't have a direct operator to get the last record


            // lastOrDefault: returns the last element of a sequence, or a default value if no element is found, these methods cannot be
            // aplied within a database like SQL Server since in sql server we don't have an operator to get the last record directly
            // tipically we use these methods in memory collections, like lists or arrays, xml, etc.

            var lastOrDefault = emptyArray.LastOrDefault(e => e > 1); // Returns the last element of the array, or default value if the array is empty

            //var lastOrDefault = context.Courses.LastOrDefault(); // this also throws an exception because we can't use it against dbs

            // if we want to "simulate" the lastOrDefault in a database, we can use OrderByDescending and then take the first element
            var lastOrDefaultSimulated = context.Courses.OrderByDescending(c => c.Id).FirstOrDefault();
            Console.WriteLine($"\n\tLast course in the collection using decending and FirstOrDefault:\n {lastOrDefaultSimulated.Name}");

            // Single: returns a single element of a sequence, and throws an exception if there is more than one element
            var single = context.Courses.Single(c => c.Id == 1); // returns the course with id 1, if there's more then returns an exception
            Console.WriteLine($"\n\tSingle course with Id 1:\n {single.Name}");

            try
            {
                var singleError = context.Courses.Single(c => c.Level == 1); // this throws an exception, since there are more than one course
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\tSingle Error: {ex.Message}"); // catches the exception and prints the message
            }

            var singleOrDefault = context.Courses.SingleOrDefault(c => c.Level == 4);
            if (singleOrDefault != null)
            {
                Console.WriteLine($"\n\tSingleOrDefault course with Level 1:\n {singleOrDefault.Name}");
            }
            else
            {
                Console.WriteLine("\n\tNo course found with Level 1"); // if no course is found, it returns null
            }

            // Quantifying operators: All, Any, Contains

            // All: checks if all elements in a sequence satisfy a condition
            var coursesFullPrice = context.Courses.All(c => c.FullPrice > 10); // returns true if all courses have a full price greater than 10, false otherwise
            Console.WriteLine($"\n\tAll courses have a full price greater than 10?: {coursesFullPrice}");

            // Any: checks if any element in a sequence satisfies a condition
            var coursesAny = context.Courses.Any(c => c.Level == 3); // is there any course of level 3?
            Console.WriteLine($"\n\tAny course with Level 3?: {coursesAny}");

            // contains: checks if a sequence contains a specific element
            var ids = new List<int> { 1, 2, 3 };
            var coursesContains = context.Courses.Where(c => ids.Contains(c.Id)); // checks if the courses collection contains any course with Id in the ids list
            Console.WriteLine("The Courses table contains any of the 'ids' list elements:");
            foreach (var id in ids)
            {
                Console.Write(id + " ");
            }
            Console.WriteLine("\n");
            foreach (var course in coursesContains)
            {
                Console.WriteLine($"id:{course.Id} - {course.Name}\n");
            }
            // Aggregating operators: Count, Sum, Average, Min, Max

            // count: counts the number of elements in a sequence
            var coursesCount = context.Courses.Count(); // counts the number of courses in the collection
            Console.WriteLine($"\n\tTotal number of courses: {coursesCount}");

            // max: returns the maximum value in a sequence
            var maxPrice = context.Courses.Max(c => c.FullPrice); // returns the maximum full price of all courses
            Console.WriteLine($"\n\tMaximum full price of all courses: ${maxPrice}");

            // min: returns the monimum value in a sequence
            var minPrice = context.Courses.Min(c => c.FullPrice); // returns the minimum full price of all courses
            Console.WriteLine($"\n\tMinimum full price of all courses: ${minPrice}");

            //average: returns the average value in a sequence
            var averagePrice = context.Courses.Average(c => c.FullPrice); // returns the average full price of all courses
            Console.WriteLine($"\n\tAverage full price of all courses: ${Math.Round(averagePrice, 2)}");

            // chaining aggregating methods
            var coursesCountChained = context.Courses.Where(c => c.Level == 1).Count(); // counts the number of courses of level 1
            Console.WriteLine($"\n\tTotal number of courses of Level 1: {coursesCountChained}");

            // chaining aggregating even simplified
            var coursesCountChainedSimplified = context.Courses.Count(c => c.Level == 1); // counts the number of courses of level 1
            Console.WriteLine($"\n\tTotal number of courses of Level 1 (simplified): {coursesCountChainedSimplified}");
        }
        
        static void DeferredExecution(PlutoContext context)
        {
            // Deferred Execution: this means that the query is not executed when it is defined, but when we need the data

            // when we execute something like the following line, no SQL query has been executed yet (in EF), not even filters have been
            // applied. The only thing you've done is to build an object that knows how to get the data when you need it:
            var exampleQuery = context.Courses.Where(c => c.Level == 1);

            // Real execution:  the query gets executed when you do something that materializes the results, such as ToList(), ToArray(),
            // ToDictionary(), First(), FirstOrDefault(), Single(), SingleOrDefault(), Count(), etc. Even a for each loop will do it.

            var exampleQuery2 = context.Courses.Where(c => c.Level == 1); // SQL is not executed yet, just a query object is created

            var list = exampleQuery2.ToList(); // here the SQL query is executed, and the results are materialized into a list

            // Example/Exercise of deferred execution
            var courses = context.Courses; // all of the courses, no SQL executed yet

            foreach (var course in courses)
            {
                Console.WriteLine(course.Name);
            }

            // Queries are executed when:
            // 1. You iterate over the results (foreach loop)
            // 2. Calling methods that materialize the results (ToList(), ToArray(), ToDictionary(), etc.)
            // 3. Any methods that return a singleton value (First(), FirstOrDefault(), Last(), Single(), SingleOrDefault(), Count(), etc.)

            // This is what we call deferred execution, queries are not executed immediately when they are defined, but when we need the data

            // Which are the benefits of deferred execution?

            // Deferred execution enables queries to be extended: We can extend this query and filter only courses in level 1:
            var coursesTwo = context.Courses; // is not executed yet against the database
            var coursesTwoFiltered = coursesTwo.Where(c => c.Level == 1); // this is also not executed yet
            // and even sort them by name:
            var coursesTwoFilteredSorted = coursesTwoFiltered.OrderBy(c => c.Name); // neither this is executed, so we're extending the query

            // later the query gets exectuted inside the foreach loop
            foreach (var course in coursesTwoFilteredSorted)
            {
                Console.WriteLine($"{course.Id} - {course.Name}");
            }

            // the same quey but chained together
            var coursesChained = context.Courses.Where(c => c.Level == 1).OrderBy(c => c.Name); // this is also not executed yet
            foreach (var course in coursesChained) // here the query gets executed
            {
                Console.WriteLine($"{course.Id} - {course.Name}");
            }

            // Immediate Execution: this means that the query is executed immediately, and the results are materialized into a collection

            // - There are times that you may want to immediately execute a query, because your quieres cannot be translated into SQL

            // - For example, if you have a calculated property in your entity, like IsBeginnerCourse in Course class:
            // it cannot be translated into SQL, so we need to execute the query immediately

            //var coursesImmediate = context.Courses.Where(c => c.IsBeginnerCourse == true); // thwrows an exception

            //- so, if we want to use the IsBeginnerCourse property, we need to execute the query immediately this means, we'll load the
            // courses from the db first, and then filter them in memory, so this is where we use immediate execution

            var coursesImmediate = context.Courses
                .ToList() // this will load all courses from the db into memory 
                .Where(c => c.IsBeginnerCourse == true); // and then filter them in memory

            // WARNING: this is not efficient, since it loads all courses from the db into memory, and then filters them in memory
            //          so, the performance of the application may be affected if you have a lot of data in the db

            // RECOMENDATION: So, if you want to go for an optimized solution, you cannot use calculated properties in your queries, well
            // actually there's a workaround for this, but it's not recommended, since it can lead to performance issues

            // You can use this approach if you have an application that handles a small amount of data, and you don't care about performance
        }

        static void IEnumerableVsIQueryable(PlutoContext context)
        {
            // IQueryable vs IEnumerable

            // - IQueryable is a queryable interface that allows you to build queries that can be executed against a database
            // - IEnumerable is a collection interface that allows you to iterate over a collection of objects in memory

            // since DBSet implements IQueryable, we can instantiate it like this, instead of var courses = context.Courses.ToList();
            IQueryable<Course> coursesIQueryable = context.Courses;
            var filteredCoursesIQ = coursesIQueryable.Where(c => c.Level == 1);

            // checking the previous query on sql profiler, it generates the following SQL query:
            // SELECT 
            // [Extent1].[Id] AS[Id], 
            // [Extent1].[Name] AS[Name], 
            // [Extent1].[Description] AS[Description], 
            // [Extent1].[Level] AS[Level], 
            // [Extent1].[FullPrice] AS[FullPrice], 
            // [Extent1].[AuthorId] AS[AuthorId]
            // FROM[dbo].[Courses] AS[Extent1]
            // WHERE 1 = [Extent1].[Level] -----> note that the filter is part of the SQL query, so it is executed in the database
            //                                    that's because we are using IQueryable, the orginal query is extended to the filter

            foreach (var course in filteredCoursesIQ)
            {
                Console.WriteLine($"{course.Id} - {course.Name} - Is for begginers?: {course.IsBeginnerCourse}");
            }

            IEnumerable<Course> coursesIEnumerable = context.Courses;
            var filteredCoursesIE = coursesIEnumerable.Where(c => c.Level == 1);

            // checking the previous query on sql profiler, it generates the following SQL query:
            // SELECT
            // [Extent1].[Id] AS[Id], 
            // [Extent1].[Name] AS[Name], 
            // [Extent1].[Description] AS[Description], 
            // [Extent1].[Level] AS[Level], 
            // [Extent1].[FullPrice] AS[FullPrice], 
            // [Extent1].[AuthorId] AS[AuthorId]
            // FROM[dbo].[Courses] AS[Extent1]

            // Notice the Where clause is not part of the SQL query, so it is executed in memory

            foreach (var course in filteredCoursesIE)
            {
                Console.WriteLine($"{course.Id} - {course.Name} - Is for begginers?: {course.IsBeginnerCourse}");
            }

            // What does this mean?

            // It means that you're gonna get all courses from the database, even if there are a BILLION of those courses, load them
            // in memory and then apply the filter, which is NOT efficient

            // The reason fot that is when you use IEnumerable, you cannot extend the query, since it is already executed and when it comes
            // to the foearch loop, the filter is applied in memory, so it was not translated to SQL.

            // ------------------------------------------------------------------------------------------
            // How IQueryable allows you to extend the query?
            // ------------------------------------------------------------------------------------------

            IEnumerable<Course> IEnumerable = context.Courses;

            // LINQ extension methods on IEnumerable expects a func or a delegate as a parameter, so it can be executed in memory
            var resultIEnum = IEnumerable.Where(ie => ie.Level == 1); // this is executed in memory first, so it cannot be translated to SQL

            IQueryable<Course> IQueryable = context.Courses;

            // LINQ extension methods on IQueryable expects an expression tree as a parameter (lambda expression), so it can be translated to SQL
            IQueryable.Where(iq => iq.Level == 1); // this is translated to SQL, so it can be executed in the database, so it can be extended
                                                   // it's simply stored in an expression object.
                                                   // Later, when this query variable is executed, all these expressions will be compiled
                                                   // and translated into SQL code, and run altogether against the database
        }

        static void LazyLoading(PlutoContext context)
        {
            // Lazy loading: this is a feature of Entity Framework that allows you to load related entities on demand

            // when this line is executed, EF is gonna send a SQL query to the database to select the course with Id 2
            // that's because we're using the single method, any of these singleton methods will execute the query, first,
            // firstOrDefault, single, singleOrDefault, count, max, min, average, etc. These methods cause immediate execution
            var course = context.Courses.Single(c => c.Id == 2); // checking sql profiler

            // however, course doesn't have its tags initialized yet, later when we access the course.Tags, EF is gonna send
            // another SQL query to the DB to get the tags for this course

            // SO, this is called lazy loading, because the related entities (tags in this case) are not loaded immediately,
            // they are loaded on demand, when we access them for the first time
            foreach (var tag in course.Tags) // checking sql profiler
                Console.WriteLine(tag.Name);

            // BEST PRACTICES:

            // 1. Use lazy loading when loading an object graph is costly and you don't need all the related entities immediately
            //    so, we use it load only the main objects and load the related entities on demand
            // 2. Use it desktop applications, where you have a lot of memory available, and you don't care about the performance
            // 3. Avoid lazy loading in web applications, since it can cause performance issues, can cause unnecesary SQL queries

            // In web applications, we can disable lazy loading by not declaring the properties as virtual, but this means that
            // you need to make sure to follow this rule for every navigation property.

            // There's a better way to guarantee that lazy loading is disabled in the entire context, and that is using a
            // configuration --> Go to PlutoContext.cs and check the constructor where we disable lazy loading.
        }

        static void NPlusOneQueries(PlutoContext context)
        {
            // If lazy loading is used innapropriately, it can lead to the N+1 problem, which is a performance issue

            // The N+1 problem occurs when you load a collection of entities, and then for each entity, you load a related entity

            // For example: Courses an Tags, if we load all courses, and then for each course, we load its tags, this can lead
            // to N+1 queries:

            // if we do:
            var courses = context.Courses.ToList(); // EF sends a query to load all courses, such as: SELECT * FROM Courses;

            // and then, if we iterate over the courses and access their tags:
            foreach (var course in courses)
            {
                Console.WriteLine($"Course: {course.Name}");
                foreach (var tag in course.Tags) // <-- if lazy loading is enabled, EF sends another query to bring the tags
                {                                // for each course, so if we have N courses
                    Console.WriteLine($"  Tag: {tag.Name}");
                }
            }

            // So we have a query such as:
            // SELECT* FROM Tags
            // INNER JOIN CourseTags ON Tags.Id = CourseTags.TagId
            // WHERE CourseTags.CourseId = 1;

            // and then, that's for the second course, the third, fourth, until the last course

            // So, the result is:
            // 1 query to load all courses
            // + N additional queries to load the tags for each course = N+1 queries
            // So, if we have 1000 courses, we end up with 1001 queries:
            // - 1 for the first query to load all courses
            // - N, in this exmaple a 1000 for each course to load its tags, summarizing, we have 1001 queries

            // Another example:
            foreach (var course in courses)
            {
                Console.WriteLine("{0} by {1}", course.Name, course.Author.Name); // EF sends a query per course to load the author
            }                                                                      // this is our N queries + 1 Initial query

            // NOTE: if lazy loading is disabled, the above foreach loop will throw an exception, since the Author property is not loaded
        }

        static void EagerLoading(PlutoContext context)
        {
            //Eager Loading: this is a feature of Entity Framework that allows you to load related entities immediately, this
            //               prevents additional queries to the database when accessing related entities

            // Taking the n+1 problem into account, we can use eager loading to load related entities immediately by
            // uing the Include method, which is an extension method of IQueryable:

            // This is one way to eager load related entities, using the Include method and a "Magic String" (string name of the
            // navigation property), however, this is a bad practice, imagine we change the name of the navigation property xd

            //var courses = context.Courses.Include("Author").ToList();

            // So that's why instead of using a magic string, we can use a lambda expression to specify the navigation property
            // we want to include, we need to include System.Data.Entity namespace to use the Include method:

            var courses = context.Courses.Include(c => c.Author).ToList(); // this will load all courses and their authors in a single query

            foreach (var course in courses)
            {
                Console.WriteLine("{0} by {1}", course.Name, course.Author.Name); // EF sends a query per course to load the author
            }

            /// Multiple levels

            // For single properties, we can use the Include method to load related entities, like this:
            context.Courses.Include(c => c.Author.Name);

            // For collection properties, we need to select the collection property, we can't access collection
            // properties directly:
            context.Courses.Include(c => c.Tags).Select(t => t.Name);

            // So, the usage of one or another depends on the type of property we want to include, as an ingenier, you should
            // decide which properties you want to include based on the requirements of your application

            // Also, there's a third way to load related objects, which is called Explicit Loading, this can be useful in
            // situations where you still to eager load a lot of objects but your queries are getting too complex.
        }

        static void ExplicitLoading(PlutoContext context)
        {
            // Explicit loading: this is a feature similar to eager loading, we tell EF exactly what should be loaded ahead
            // of time.

            // The difference between eager loading and explicit loading is that in eager loading EF will join the related
            // tables and query all the data in one roundtrip, whereas with eager loading, there will be multiple queries
            // depending on the number of explicit loads you have in your code

            // Summary
            // Eager loading:                       Explicit Loading:
            // - Uses JOINs                         - Separate queries
            // - One round-trip to the database     - Multiple round-trips to the database

            // Eager loading:
            var author = context.Authors.Include(a => a.Courses).Single(a => a.Id == 1); // this will eager load the courses for the author with Id 1

            foreach (var course in author.Courses)
            {
                Console.WriteLine("{0}", course.Name);
            }

            // In situations where you have too many includes, your queries end up being really complex, in these cases, if
            // you still need to load all these objects ahead of time, you can use explicit loading, which is

            //Explicit loading:

            // getting rid of the Include is going to simplify the query, because EF is not going to join the Authors table with
            // the courses table, 
            var authorExplicit = context.Authors.Single(a => a.Id == 1);

            // now we need to explicitly load the courses for this author, and this is why we call it explicit loading:
            // There are 2 ways to do this:

            // 1. MSDN way: using the Load method, which instructor doesn't recommend, since you need to remember a lot of
            // methods to call, entry, collection, load, etc. But, there's another reason... this only works for SINGLE ENTRIES.
            // If my query returns a list of authors, we cannot use this approach:
            context.Entry(authorExplicit).Collection(a => a.Courses).Load();

            // 2. The instructor's (MOSH) way:
            context.Courses.Where(c => c.AuthorId == authorExplicit.Id).Load();

            // Another benefit of explicit loading is that you apply filters to the related entities that you want to load:

            // First, MDSN way: too noisy as you can see
            context.Entry(authorExplicit).Collection(a => a.Courses).Query().Where(c => c.FullPrice == 0).Load();

            // Mosh's way:
            context.Courses.Where(c => c.AuthorId == authorExplicit.Id && c.FullPrice == 0).Load();

            // Just remember the Load() method, you can make your queries as normal as usual, just for explicit loading you
            // just need to add the Load() method at the end :D


            // Another example:

            // Imagine we wanna get all authors and their free courses
            var authors = context.Authors.ToList();

            // now to get the free courses for each author, we cannot use the MSDN way, because if I call context.Entry() I
            // cannot pass the authors variable collection, cause' the entry method is used to reference only a single object
            // so we need to use Mosh's way:

            var authorIds = authors.Select(a => a.Id); // this returns an IEnumerable<int> with all the author Ids

            context.Courses.Where(c => authorIds.Contains(c.AuthorId) && c.FullPrice == 0).Load(); // Sql will use the IN operator to select multiple courses
        }

        static void AddinObjectsToTheDB(PlutoContext context)
        {
            //LinqSintax(context);
            //ExtensionMethods(context);
            //AdditionalExtensionMethods(context);
            //DeferredExecution(context);
            //IEnumerableVsIQueryable(context);
            //LazyLoading(context);
            //NPlusOneQueries(context);    
            //EagerLoading(context);
            //ExplicitLoading(context);

            // Adding objects to the database

            var course = new Course // new course object to be added to the db
            {
                Name = "New Course",
                Description = "New Course Description",
                FullPrice = 19.95f,
                Level = 1,
                Author = new Author { Id = 1, Name = "Edgar Vargas" } // this create a new author object
            };

            //context.Courses.Add(course); // adding the course to the context, this doesn't save it to the database yet!!!

            //context.SaveChanges(); // this saves the changes to the database, so now the course is added to the db

            // The above creates a new Author object, this could lead to duplicated authors in the database, so we have two approaches to solve this:

            // ----------------- 1. Bringin all of the authors from the database, and then check if the author already exists, if it does, we use that one:

            var authors = context.Authors.ToList(); // bringing all authors from the database

            var author = context.Authors.Local.Single(a => a.Id == 1); // getting the author with Id 1, this is the author we want to use which is in memory thanks
                                                                       // to the previous query

            var course2 = new Course // new course object to be added to the db
            {
                Name = "New Course",
                Description = "New Course Description",
                FullPrice = 19.95f,
                Level = 1,
                Author = author // this create a new author object
            };

            //context.Courses.Add(course2); // adding the course to the context, this doesn't save it to the database yet!!!
            //context.SaveChanges(); // this saves the changes to the database, so now the course is added to the db

            // This approach is more useful in WPF applications, where you have a lot of objects in memory, and you can bring all authors

            // ----------------- 2. Using foreign key properties

            // with this approach we don't need the "authors" or "author" objects we used before:

            var course3 = new Course // new course object to be added to the db
            {
                Name = "New Course",
                Description = "New Course Description",
                FullPrice = 19.95f,
                Level = 1,
                AuthorId = 1 // more simple, we just set the AuthorId property to the Id of the author we want to use
            };

            // This second approach works well in web applications, because in web apps our context is short-lived, so we don't have objects hanging in the memory

            // ----------------- 3. Using the Attach method

            // There's a third approach, which is not used very often, but it's worth mentioning for some odd circumstances:

            // Imagine we have an object that's not in our context, something like this:

            var authorNew = new Author() { Id = 1, Name = "Mosh Hamedani" }; // this is an author object that is not in the context

            context.Authors.Attach(authorNew);

            var course4 = new Course
            {
                Name = "New Course",
                Description = "New Course Description",
                FullPrice = 19.95f,
                Level = 1,
                Author = authorNew // we use the navigation property to set the author
            };

            // Mosh doesn't recommend this approach, the problem with Attach() method or similar methods is that they make you too close to EF inner workings

            // Final recommendations:

            // - Use the two first approaches, depending on the type of application you're building:
            //   - If you have an existing author in the context, simply get that, but you need to make sure the author (in this case) is in the context
            //   - Otherwise, use the foreign key property to create the association between the course and the author (simpler and cleaner)
        }

        static void Main(string[] args)
        {
            var context = new PlutoContext(); 

            
        }
    }
}
