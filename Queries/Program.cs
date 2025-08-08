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

            // LINQ syntax
            Console.WriteLine("\tUsing LINQ Syntax:");
            var coursesLINQ = from c in context.Courses
                        where c.Name.Contains("C#")
                        orderby c.Name
                        select c;
            // iterate through the results
            foreach(var course in coursesLINQ)
            {
                Console.WriteLine(course.Name);
            }

            // Extension method syntax
            Console.WriteLine("\tUsing Method Sintax:");
            var coursesExt = context.Courses
                .Where(c => c.Name.Contains("C#"))
                .OrderBy(c => c.Name);
            // iterate through the results
            foreach(var course in coursesExt)
            {
                Console.WriteLine(course.Name);
            }
        }
    }
}
