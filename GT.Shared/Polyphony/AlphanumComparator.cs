using System;
using System.Collections;

public class AlphanumComparatorPD_Razer : IComparer {
    public int Compare(object x, object y) {
        var length = (uint)y.ToString().Length;
        var minLength = Math.Min((uint)x.ToString().Length, length);

        var p1 = x.ToString();
        var p2 = y.ToString();

        for (var i = 0; i < minLength; ++i) {
            if (p1[i] < p2[i])
                return -1;
            if (p1[i] > p2[i])
                return 1;
        }

        if ((uint)x.ToString().Length < length)
            return -1;
        return (uint)x.ToString().Length > length ? 1 : 0;
    }
}