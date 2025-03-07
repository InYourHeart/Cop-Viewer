using CoP_Viewer.Source.Model;
using CoP_Viewer.Source.UI;
using CoP_Viewer.Source.Util;
using System.Globalization;
using System.Security.Claims;
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
            foreach (String[] claimData in CsvHandler.getValuesFromCSV(Properties.Resources.ClaimData))
            {
                String claimName = claimData[0];
                String claimHex = claimData[1];

                claimList.Add(PixelHandler.hexToInt(claimHex), new Claim(claimName));
            }
        }

        public void loadTerrainList()
        {
            foreach (String[] terrainData in CsvHandler.getValuesFromCSV(Properties.Resources.TerrainData))
            {
                String terrainName = terrainData[0];
                String terrainHex = terrainData[1];
                int terrainBaseManpower = Int32.Parse(terrainData[2]);

                terrainList.Add(PixelHandler.hexToInt(terrainHex), new Terrain(terrainName, terrainHex, terrainBaseManpower));
            }
        }

        public void loadCityList()
        {
            foreach (String[] cityData in CsvHandler.getValuesFromCSV(Properties.Resources.CityData))
            {
                String cityName = cityData[0];
                String cityHex = cityData[2];

                cityList.Add(PixelHandler.hexToInt(cityHex), new City(cityName));
            }
        }

        public void loadRegionList()
        {
            foreach (String[] regionData in CsvHandler.getValuesFromCSV(Properties.Resources.RegionData))
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

                        claim.totalTax += terrain.baseManpower * taxModifier;
                        claim.totalManpower += terrain.baseManpower * manpowerModifier;
                    }
                }
            }
        }

        public void showInfoForPixel(int claimColor, int terrainColor, int regionColor, int occupationColor, int devastationColor)
        {
            Claim? claim; claimList.TryGetValue(claimColor, out claim);
            City? city; cityList.TryGetValue(terrainColor, out city);
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
                infoView.setPixelInfoViewEntryValue("Devastation", devastationPercentage.ToString("0.##"));
            }
        }
    }
}
