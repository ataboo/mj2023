using System;
using System.Linq;

public static class MechGameHelpers {
    public static int MaxEnumValue<TEnum>() where TEnum: Enum {
        return Enum.GetValues(typeof(TEnum)).Cast<int>().Max();
    }
}