using Shared;

namespace AgentClient;

public static class FakePart
{
    private static int _counter = 0;

    public static Part Get()
    {
        _counter++;
        return new Part() {Id = "Id" + _counter, Name = "Part" + _counter};
    }
}