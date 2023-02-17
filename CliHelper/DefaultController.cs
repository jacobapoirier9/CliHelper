namespace CliHelper;

internal class DefaultController : Controller
{
    private readonly List<Registration> _registrations;
    public DefaultController(List<Registration> registrations)
    {
        _registrations = registrations;
    }

    [Cli("help")]
    public void Help()
    {
        var groups = _registrations
            .Where(r => r.Type != typeof(DefaultController))
            .GroupBy(r => r.Type, r => r)
            .OrderBy(g => g.Key.Name);

        foreach (var group in groups)
        {
            var firstRegistration = _registrations.First(r => r.Type == group.Key);
            Console.WriteLine(firstRegistration?.TypeAttribute?.Alias ?? firstRegistration.Type.Name);

            foreach (var registration in group)
            {
                Console.Write("  ");
                Console.WriteLine(registration?.MethodAttribute?.Alias ?? registration.Method.Name);
            }

            Console.WriteLine();
        }

        var distinctTypes = _registrations.Select(r => r.Type).Distinct().OrderBy(t => t.Name).ToList();
        foreach (var type in distinctTypes)
        {
            var currentRegistrations = _registrations.Where(r => r.Type == type).OrderBy(r => r.Method.Name).ToList();
        }
    }
}