# Coalition of Powers Map Viewer

A C# desktop application that calculates and provides visualization of the relevant statistics and maps for Coalition of Powers.

## Pre-Requisites

Built for Windows. Definitely works of Windows 10, I have not tested outside of that.

## Installation

1. Download copviewer.zip from the latest release here: https://github.com/InYourHeart/Cop-Viewer/releases
2. Unzip into your preferred folder

## Running

1. Double click the "CoP Viewer.exe" file

## Configuration

The application makes use of 5 color coded maps, found in the /maps folder in .png format. The .csv files in the /data folder give information in relation to those colors.

These are loaded on start up, so if you make alterations you will have to restart the application before the changes take effect.

### Political map (political.png)

Represents what pixels are owned by what Claims. 

The claims.csv file links the hex color values to the Claim names, as seen below. 

```
France;A7AAD7
Spain;C19B4A
Portugal;9BCC8B
```

Pixels with colors not found in the claims.csv are considered not owned by anyone.

### Terrain map (terrain.png)

Defines the base economic value of each pixel, primarily by a representation of its terrain.

The terrain.csv links each hex color to a name and base value of tax and manpower. Pixels with colors not found in this .csv are considered to have base values of 0.

```
Grassland;359950;10;10
River;354ca0;10;10
Dryland;B29950;6;6
```

The cities.csv file provides additional information for City colored pixels. A City pixel is identified as belonging to a specific City by finding a path composed of City pixels to one of the coordinates found in the .csv file.

```
Rome;2377,2379;3735,3735
Milan;2074;3253
Naples;2553;3872
Turin;1938;3295
```

### Occupations map (occupations.png)

Represents what pixels are occupied by who. 

Uses the claims.csv file for occupations made by Claims. The 0x3F3F3F hex code is used for rebel occupations.

### Regions map (regions.png)

Can be used to represent Regions within Claims that provide a different ratio of resources.

The regions.csv file specifies the properties of the Region. It holds a name, hex, and modifiers applied to tax and manpower.

```
Lombardy;F8B972;1;0.75
```

### Devastation map (devastation.png)

Represents how devastated each pixel is. 

The value is a percentange from 0 to 100, and is represented by the pixel's saturation value in the HSV color format. 

Only pixels with the maximum red value of 255 in the RGB color format are counted.
