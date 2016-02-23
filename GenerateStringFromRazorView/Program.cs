using HashCode.Common.AspNet;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerateStringFromRazorView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var viewName = "MyView";
            var razorViewGenerator = RazorViewGenerator.Create(PlatformServices.Default.Application.ApplicationBasePath);
            var result = razorViewGenerator.RenderViewToString(viewName, new MyModel("hello world")).GetAwaiter().GetResult();

            Console.WriteLine($"Contenu du fichier {viewName} :\n{result}");
            Console.ReadKey();
        }
    }
}
