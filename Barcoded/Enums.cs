using System.ComponentModel;

namespace Barcoded
{
    public enum Symbology
    {
        [Description("Code 128 Start A")]
        Code128ABC,
        [Description("Code 128 Start B")]
        Code128BAC,
        [Description("Code 128 Start A Supress C")]
        Code128AB,
        [Description("Code 128 Start B Supress C")]
        Code128BA,
        [Description("GS1-128")]
        GS1128,
        [Description("Code 39")]
        Code39,
        [Description("Code 39 Full ASCII")]
        Code39Full,
        [Description("Code 39 With Check Digit")]
        Code39C,
        [Description("Code 39 Full ASCII With Check Digit")]
        Code39FullC,
        [Description("Interleaved 2 of 5")]
        I2of5,
        [Description("Interleaved 2 of 5 With Check Digit")]
        I2of5C,
        [Description("EAN-13")]
        Ean13,
        [Description("UPC-A")]
        UpcA,
        [Description("EAN-8")]
        Ean8
    };

    public enum HumanReadablePosition
    {
        Hidden,
        Above,
        Below,
        Embedded
    };

    public enum Code128Subset
    {
        A,
        B,
        C,
        Null
    }

    public enum ModuleType
    {
        Bar,
        Space
    };
}
