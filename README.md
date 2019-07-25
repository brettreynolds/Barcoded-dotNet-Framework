![Barcoded Icon](https://barcoded.online/Assets/Barcode%20Icon%201.svg)
# Barcoded

A C#/.NET library to generate barcode images for eith Code128 or Code 39 symbologies.

## Usage
```
LinearBarcode newBarcode = new LinearBarcode("SomeValue", Symbology.Code128BAC)
    {
    Encoder =
        {
        Dpi = 300,
        BarcodeHeight = 200,
        TargetWidth = 400,
        }
    }
```
