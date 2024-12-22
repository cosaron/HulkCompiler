namespace HulkCompiler.Utils;
public static class Extensions
{
    public static void Deconstruct<T>(this T[] array, out T a)
    {
        if (array.Length < 1)
            throw new ArgumentException("Deconstruct Exception. Array must have at least 1 element");

        a = array[0];
    }
    public static void Deconstruct<T>(this T[] array, out T a, out T b)
    {
        if (array.Length < 2)
            throw new ArgumentException("Deconstruct Exception. Array must have at least 2 elements");

        a = array[0];
        b = array[1];
    }
    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c)
    {
        if (array.Length < 3)
            throw new ArgumentException("Array must have at least 3 elements");

        a = array[0];
        b = array[1];
        c = array[2];
    }
    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c, out T d)
    {
        if (array.Length < 4)
            throw new ArgumentException("Array must have at least 4 elements");

        a = array[0];
        b = array[1];
        c = array[2];
        d = array[3];
    }

    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c, out T d, out T e)
    {
        if (array.Length < 5)
            throw new ArgumentException("Array must have at least 5 elements");

        a = array[0];
        b = array[1];
        c = array[2];
        d = array[3];
        e = array[4];
    }

    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c, out T d, out T e, out T f)
    {
        if (array.Length < 6)
            throw new ArgumentException("Array must have at least 6 elements");

        a = array[0];
        b = array[1];
        c = array[2];
        d = array[3];
        e = array[4];
        f = array[5];
    }



}

public static class Functions
{
    public static T[]? TopologicalSort<T>(Dictionary<T, List<T>> diGraph) where T : notnull
    {
        T[] sorted = new T[diGraph.Count];
        Dictionary<T, int> inDegree = diGraph.Keys.ToDictionary(key => key, _ => 0);

        foreach (var (node, edges) in diGraph)
            foreach (var edge in edges)
                inDegree[edge]++;

        Queue<T> pendings = new();

        foreach (var (node, degree) in inDegree)
            if (degree == 0)
                pendings.Enqueue(node);

        int i = 0;

        while (pendings.Count > 0)
        {
            T actualNode = pendings.Dequeue();
            sorted[i++] = actualNode;
            foreach (var node in diGraph[actualNode])
            {
                inDegree[node]--;
                if (inDegree[node] == 0)
                    pendings.Enqueue(node);
            }
        }

        return i == diGraph.Count ? sorted : null;
    }



}
