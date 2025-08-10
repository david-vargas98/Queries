using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics.Contracts;
using System.Diagnostics.PerformanceData;
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

        static void Main(string[] args)
        {
            var context = new PlutoContext();

            //LinqSintax(context);
            //ExtensionMethods(context);
            //AdditionalExtensionMethods(context);

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

            foreach(var course in courses)
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
            foreach(var course in coursesTwoFilteredSorted)
            {
                Console.WriteLine($"{course.Id} - {course.Name}");
            }

            // the same quey but chained together
            var coursesChained = context.Courses.Where(c => c.Level == 1).OrderBy(c => c.Name); // this is also not executed yet
            foreach(var course in coursesChained) // here the query gets executed
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
    }
}
