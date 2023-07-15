using System;
using System.Linq;
using Godot;

public static class MechGameHelpers {
    public static int MaxEnumValue<TEnum>() where TEnum: Enum {
        return Enum.GetValues(typeof(TEnum)).Cast<int>().Max();
    }

    // https://stackoverflow.com/a/76545194/5679246
    public static Vector2 RandomPointInCircle(Random rand, Vector2 center, float radius) {
        var r = radius * Mathf.Sqrt((float)rand.NextDouble());
        var theta = (float)rand.NextDouble() * 2 * Mathf.Pi;

        return new Vector2(center.x + r * Mathf.Cos(theta), center.y + r * Mathf.Sin(theta));
    }
}