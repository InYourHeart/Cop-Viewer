using CoP_Viewer.Source.Model;
using CoP_Viewer.Source.UI;
using CoP_Viewer.Source.Util;
using System;
using System.Collections;
using Claim = CoP_Viewer.Source.Model.Claim;
using Region = CoP_Viewer.Source.Model.Region;

namespace CoP_Viewer.Source.Controller
{
    internal class MapController
    {
        private MapView mapView;
        private InfoView infoView;

        private Dictionary<int, Claim> claimList;
        private Dictionary<int, Terrain> terrainList;
        private Dictionary<int, City> cityList;
        private Dictionary<int, Region> regionList;

        public const string CLAIMS_KEY = "Resources\\data\\claims.csv";
        public const string CITIES_KEY = "Resources\\data\\cities.csv";
        public const string REGIONS_KEY = "Resources\\data\\regions.csv";
        public const string TERRAINS_KEY = "Resources\\data\\terrains.csv";

        public MapController(MapView mapView, InfoView infoView)
        {
            claimList = new Dictionary<int, Claim>();
            terrainList = new Dictionary<int, Terrain>();
            cityList = new Dictionary<int, City>();
            regionList = new Dictionary<int, Region>();

            this.mapView = mapView;
            this.infoView = infoView;

            this.mapView.setMapController(this);

            loadClaimList();
            loadTerrainList();
            loadCityList();
            loadRegionList();

            calculateClaimValues();
        }

        public void loadClaimList()
        {
            foreach (String[] claimData in CsvHandler.getValuesFromCSV(AppDomain.CurrentDomain.BaseDirectory + CLAIMS_KEY))
            {
                String claimName = claimData[0];
                String claimHex = claimData[1];

                claimList.Add(PixelHandler.hexToInt(claimHex), new Claim(claimName));
            }
        }

        public void loadTerrainList()
        {
            foreach (String[] terrainData in CsvHandler.getValuesFromCSV(AppDomain.CurrentDomain.BaseDirectory + TERRAINS_KEY))
            {
                String terrainName = terrainData[0];
                String terrainHex = terrainData[1];
                int terrainBaseTax = Int32.Parse(terrainData[2]);
                int terrainBaseManpower = Int32.Parse(terrainData[3]);

                terrainList.Add(PixelHandler.hexToInt(terrainHex), new Terrain(terrainName, terrainHex, terrainBaseTax, terrainBaseManpower));
            }
        }

        public void loadCityList()
        {
            int lineNum = 0;

            foreach (String[] cityData in CsvHandler.getValuesFromCSV(AppDomain.CurrentDomain.BaseDirectory + CITIES_KEY))
            {
                lineNum++;

                String cityName = cityData[0];
                String[] xCoords = cityData[1].Split(",");
                String[] yCoords = cityData[2].Split(",");

                if (xCoords[0].Equals(""))
                {
                    Logger.logError("City coordinate data is empty - Line " + lineNum);
                    continue;
                }

                //Add an entry for every set of coordinates
                for (int i = 0; i < xCoords.Length; i++)
                {
                    try
                    {
                        int index = PixelHandler.getIndex(int.Parse(xCoords[i]), int.Parse(yCoords[i]), mapView.getWidth());

                        cityList.Add(index, new City(cityName));
                    } catch (ArgumentNullException)
                    {
                        Logger.logError("City coordinate data is null - Line " + lineNum);
                    }
                    catch (FormatException) {
                        Logger.logError("City coordinate data is not a number - Line " + lineNum);
                    }
                }
            }
        }

        public void loadRegionList()
        {
            foreach (String[] regionData in CsvHandler.getValuesFromCSV(AppDomain.CurrentDomain.BaseDirectory + REGIONS_KEY))
            {
                String regionName = regionData[0];
                String regionHex = regionData[1];
                double regionTaxModifier = Double.Parse(regionData[2].Replace(".", ","));
                double regionManpowerModifier = Double.Parse(regionData[3].Replace(".", ","));

                regionList.Add(PixelHandler.hexToInt(regionHex), new Region(regionName, regionTaxModifier, regionManpowerModifier));
            }
        }

        private void calculateClaimValues()
        {
            int[] politicalImagePixels = mapView.getClaimImagePixels();
            int[] terrainImagePixels = mapView.getTerrainImagePixels();
            int[] regionsImagePixels = mapView.getRegionsImagePixels();
            int[] occupationsImagePixels = mapView.getOccupationsImagePixels();
            int[] devastationImagePixels = mapView.getDevastationImagePixels();

            for (int i = 0; i < politicalImagePixels.Length; i++)
            {
                int politicalPixel = politicalImagePixels[i] & 0xffffff;
                int occupationPixel = occupationsImagePixels[i] & 0xffffff;

                //If pixel is occupied and the occupier exists, do not count it.
                if (claimList.ContainsKey(occupationPixel))
                {
                    continue;
                }

                if (!claimList.ContainsKey(politicalPixel))
                {
                    continue;
                }

                Claim? claim;
                claimList.TryGetValue(politicalPixel, out claim);
                if (claim != null)
                {
                    int terrainPixel = terrainImagePixels[i] & 0xffffff;

                    Terrain? terrain;
                    terrainList.TryGetValue(terrainPixel, out terrain);

                    if (terrain != null)
                    {
                        double taxModifier = 1;
                        double manpowerModifier = 1;

                        Region? region;
                        regionList.TryGetValue(regionsImagePixels[i] & 0xffffff, out region);
                        if (region != null)
                        {
                            taxModifier *= region.taxModifier;
                            manpowerModifier *= region.manpowerModifier;
                        }

                        double devastationPercentage = PixelHandler.getDevastation(devastationImagePixels[i] & 0xffffff);

                        if (devastationPercentage != -1)
                        {
                            taxModifier *= devastationPercentage;
                            manpowerModifier *= devastationPercentage;
                        }

                        claim.totalTax += terrain.baseTax * taxModifier;
                        claim.totalManpower += terrain.baseManpower * manpowerModifier;
                    } else
                    {
                        Point coords = PixelHandler.getCoords(i,mapView.getWidth());

                        Logger.logError("Unrecognized terrain color at index " + i + ", coords (" + coords.X + "," + coords.Y + "). Color: " + terrainPixel);
                    }
                }
            }
        }

        private int findCity(int index, int limit)
        {
            ArrayList searchedIndexes = new ArrayList();

            return findCity(index, limit, 0, ref searchedIndexes);
        }

        private int findCity(int index, int limit, int distance, ref ArrayList searchedIndexes)
        {
            if (cityList.ContainsKey(index))
            {
                return index;
            }

            if (distance + 1 > limit)
            {
                return -1;
            }

            int width = mapView.getWidth();
            ArrayList nextIndexes = new ArrayList();

            checkIndexForCityTerrain(index - 1, ref nextIndexes, ref searchedIndexes);
            checkIndexForCityTerrain(index + 1, ref nextIndexes, ref searchedIndexes);
            checkIndexForCityTerrain(index - width, ref nextIndexes, ref searchedIndexes);
            checkIndexForCityTerrain(index + width, ref nextIndexes, ref searchedIndexes);

            foreach(int nextIndex in nextIndexes)
            {
                int result = findCity(nextIndex, limit, distance + 1, ref searchedIndexes);

                if (result != -1)
                {
                    return result;
                }
            }

            return -1;
        }

        private void checkIndexForCityTerrain(int nextIndex, ref ArrayList nextIndexes, ref ArrayList searchedIndexes)
        {
            Terrain? t;
            if (!searchedIndexes.Contains(nextIndex))
            {
                terrainList.TryGetValue(mapView.getTerrainAt(nextIndex), out t);
                if (t != null && t.name.Equals("City"))
                {
                    nextIndexes.Add(nextIndex);
                }
            }

            searchedIndexes.Add(nextIndex);
        }

        public void showInfoForPixel(int index, int claimColor, int terrainColor, int regionColor, int occupationColor, int devastationColor)
        {
            Claim? claim; claimList.TryGetValue(claimColor, out claim);
            Terrain? terrain; terrainList.TryGetValue(terrainColor, out terrain);
            City? city = null; 
            
            if (terrain != null && terrain.name.Equals("City")) 
            {
                cityList.TryGetValue(findCity(index, 50), out city);
            };

            Region? region; regionList.TryGetValue(regionColor, out region);
            Claim? occupier; claimList.TryGetValue(occupationColor, out occupier);
            double devastationPercentage = PixelHandler.getDevastation(devastationColor);

            if (claim == null)
            {
                infoView.setPixelInfoViewEntryValue("Claim", "None selected");
                infoView.setPixelInfoViewEntryValue("Tax", null);
                infoView.setPixelInfoViewEntryValue("Manpower", null);
            }
            else
            {
                infoView.setPixelInfoViewEntryValue("Claim", claim.name);
                infoView.setPixelInfoViewEntryValue("Tax", claim.totalTax);
                infoView.setPixelInfoViewEntryValue("Manpower", claim.totalManpower);
            }

            if (city == null)
            {
                infoView.setPixelInfoViewEntryValue("City", null);
            }
            else
            {
                infoView.setPixelInfoViewEntryValue("City", city.name);
            }

            if (region == null)
            {
                infoView.setPixelInfoViewEntryValue("Region", null);
            }
            else
            {
                infoView.setPixelInfoViewEntryValue("Region", region.name);
            }

            if (occupier == null)
            {
                infoView.setPixelInfoViewEntryValue("Occupied by", null);
            }
            else
            {
                infoView.setPixelInfoViewEntryValue("Occupied by", occupier.name);
            }

            if (devastationPercentage == -1)
            {
                infoView.setPixelInfoViewEntryValue("Devastation", null);
            }
            else
            {
                infoView.setPixelInfoViewEntryValue("Devastation", (devastationPercentage * 100).ToString("0.##"));
            }
        }
    }
}
