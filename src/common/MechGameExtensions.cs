public static class MechGameExtensions
{
    public static T[] ToNativeArray<T>(this Godot.Collections.Array gArray) {
        var retArray = new T[gArray.Count];
        gArray.CopyTo(retArray, 0);
        return retArray;
    }
}