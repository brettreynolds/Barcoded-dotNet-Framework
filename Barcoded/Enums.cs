using System;
using System.Collections.Generic;
using System.Text;

namespace Barcoded
{
    public enum Symbology
    {
        Code128ABC,
        Code128BAC,
        Code128AB,
        Code128BA,
        Code39,
        Code39Full,
        Code39C,
        Code39FullC
    };

    public enum HumanReadablePosition
    {
        Hidden,
        Above,
        Below,
        Embeded
    };

    public enum Code128Subset
    {
        A,
        B,
        C,
        Null
    }

    internal enum ModuleType
    {
        Bar,
        Space
    };
}
