using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Queries
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new PlutoContext();

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
            foreach(var group in coursesGrouped)
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
            foreach(var row in coursesJoinGroup)
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
            foreach(var row in coursesCrossJoin)
            {
                Console.WriteLine($"{row.AuthorName} - {row.CourseName}");
            }
        }
    }
}
