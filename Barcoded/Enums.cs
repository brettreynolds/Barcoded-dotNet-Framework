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
