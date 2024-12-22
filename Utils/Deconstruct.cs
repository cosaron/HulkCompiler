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