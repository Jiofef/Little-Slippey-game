

public static class ArrayExtensions
{
    public static T TakeAndClear<T>(this T[] array, int index, T defaultValue = default)
    {
        T value = array[index];
        array[index] = defaultValue;
        return value;
    }
}
