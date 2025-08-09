using System;
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
        static void Main(string[] args)
        {
            var context = new PlutoContext();
            //LinqSintax(context);

            // LINQ Extension Methods

            // Restriction: Filtering data
            var coursesLvlOne = context.Courses.Where(c => c.Level == 1); // filtering courses by level 1
            Console.WriteLine("\tRestriction/Filtering - Courses filtered by Level 1");
            foreach(var course in coursesLvlOne)
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
            foreach(var course in coursesOrderedDesc)
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
                foreach(var courseTag in courseListTag) // iterating over each tag in the list
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

            foreach(var tag in listTagsFlatten)
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

            foreach(var tag in listTagsFlattenDisct)
            {
                Console.WriteLine($"{tag.Name}");
            }

            // Grouping: Grouping courses by their level without using aggregation
            var coursesGroupedByLevel = context.Courses.GroupBy(c => c.Level); // grouping courses by their level

            Console.Write("\n\tGroupung courses by level");
            foreach(var coursesLevel in coursesGroupedByLevel)
            {
                Console.WriteLine($"\nLevel {coursesLevel.Key}");

                foreach(var course in coursesLevel)
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
    }
}
