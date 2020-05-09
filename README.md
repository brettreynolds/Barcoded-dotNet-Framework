![Barcoded Icon](https://barcoded.online/Assets/Barcode%20Icon%201.svg)
# Barcoded
A C#/.NET library to generate barcode images.

[![Nuget](https://img.shields.io/nuget/v/Barcoded)](https://www.nuget.org/packages/Barcoded/)
[![Nuget](https://img.shields.io/nuget/dt/Barcoded)](https://www.nuget.org/packages/Barcoded/)
## Usage
```C#
LinearBarcode newBarcode = new LinearBarcode("SomeValue", Symbology.Code128BAC)
    {
    Encoder =
        {
            Dpi = 300,
            BarcodeHeight = 200
        }
    };
```

## Features

* **Supported Symbologies**
  - Code128 (Subsets A,B & C)
  - Code39 (Standard & Full ASCII)
  - GS1-128
  - EAN-13
  - EAN-8
  - UPC-A
  - Interleaved 2 of 5
  
* **Human Readable Label**
  - Discrete Text
  ```C#
  newBarcode.Encoder.HumanReadableValue = "S O M E V A L U E";
  ```
  - Placement
  ```C#
  newBarcode.Encoder.SetHumanReadablePosition("Above");
  ```
  - Font
  ```C#
  newBarcode.Encoder.SetHumanReadableFont("Arial", 8);
  ```
* **Match X Dimension (narrow bar) to desired width**
  ```C#
  newBarcode.TargetWidth = 400;
  ```

* **Show encoding characters**
  ```C#
  newBarcode.Encoder.ShowEncoding = true;
  ```

* **Output to ZPL string**
  ```C#
  string zplString = newBarcode.ZplEncode;
  ```

* **Include quietzone**
  ```C#
  newBarcode.Encoder.Quietzone = true;
  ```

## Demo

Website: https://barcoded.online/
provides a web UI that utilises the **Barcoded** library. Registration is required to support emailing of generated barcodes.
