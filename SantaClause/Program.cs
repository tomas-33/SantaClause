namespace SantaClause
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("North pole");
            var northPole = new NorthPole();

            await northPole.StartNorthPoleAsync();

            Console.ReadKey();
        }
    }
}
