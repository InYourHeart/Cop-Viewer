using CoP_Viewer.Source.Controller;
using CoP_Viewer.Source.Model;
using CoP_Viewer.Source.Util;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace CoP_Viewer.Source.UI
{
    public class MapView : Control
    {
        //Map image data
        private int[] politicalImagePixels;
        private int[] terrainImagePixels;
        private int[] regionsImagePixels;
        private int[] occupationsImagePixels;
        private int[] devastationImagePixels;

        public const string POLITICAL_KEY = "political_map";
        public const string TERRAIN_KEY = "terrain_map";
        public const string REGIONS_KEY = "regions_map";
        public const string OCCUPATIONS_KEY = "occupations_map";
        public const string DEVASTATION_KEY = "devastation_map";

        private Bitmap politicalMap;
        private Bitmap terrainMap;
        private Bitmap regionsMap;
        private Bitmap occupationsMap;
        private Bitmap devastationMap;

        //Controller
        private MapController mapController;

        //--------------Image manipulation values---------------------//
        // Active map image
        private string backgroundMapKey = POLITICAL_KEY;
        private string overlayKey = "";
        private float mapImageInnerRatio;
        private float mapImageToViewRatio;

        private int initialHeight = -1;

        // Factor for zoom the image
        private const float MAX_ZOOM = 1000;
        private const float MIN_ZOOM = 0.9f;
        private const float ZOOM_SPEED = 0.5f;
        private float zoomFac = 1;

        //set on the mouse down to know from where moving starts
        private float transStartX;
        private float transStartY;

        //Mouse status
        private bool isClicking = false;
        private bool isDragging = false;

        //Current Image position after moving 
        private float curImageX = 0;
        private float curImageY = 0;
        //--------------------------------------------------------------//

        //Child UI elements
        private Point? selectedPixelIndex;

        public MapView()
        {
            this.BackColor = Color.White;

            politicalMap = new Bitmap(System.Configuration.ConfigurationManager.AppSettings[POLITICAL_KEY]);
            terrainMap = new Bitmap(System.Configuration.ConfigurationManager.AppSettings[TERRAIN_KEY]);
            regionsMap = new Bitmap(System.Configuration.ConfigurationManager.AppSettings[REGIONS_KEY]);
            occupationsMap = new Bitmap(System.Configuration.ConfigurationManager.AppSettings[OCCUPATIONS_KEY]);
            devastationMap = new Bitmap(System.Configuration.ConfigurationManager.AppSettings[DEVASTATION_KEY]);

            //Storing copies of the images' pixel arrays for later accesses
            politicalImagePixels = PixelHandler.getPixels(politicalMap);
            terrainImagePixels = PixelHandler.getPixels(terrainMap);
            regionsImagePixels = PixelHandler.getPixels(regionsMap);
            occupationsImagePixels = PixelHandler.getPixels(occupationsMap);
            devastationImagePixels = PixelHandler.getPixels(devastationMap);

            mapImageInnerRatio = (float)politicalMap.Width / (float)politicalMap.Height;

            this.DoubleBuffered = true;

            this.MouseUp += new MouseEventHandler(mouseUp);
            this.MouseMove += new MouseEventHandler(mouseMove);
            this.MouseWheel += new MouseEventHandler(mouseWheel);
            this.MouseDown += new MouseEventHandler(mouseDown);
        }

        public void setMapImage(string key)
        {
            switch (key)
            {
                case POLITICAL_KEY:
                case TERRAIN_KEY:
                    backgroundMapKey = key;
                    break;
                case REGIONS_KEY:
                case OCCUPATIONS_KEY:
                case DEVASTATION_KEY:
                    if (overlayKey.Equals(key))
                    {
                        overlayKey = "";
                        break;
                    }
                    overlayKey = key;
                    break;
                default:
                    break;
            }

            this.Invalidate();
        }

        public void ResizeView(int newHeight, int newWidth, Point newLocation)
        {
            if (this.initialHeight == -1)
            {
                this.initialHeight = newHeight;
            }

            this.Height = newHeight;
            this.Width = newWidth;
            this.Location = newLocation;

            curImageX = (float)this.Width / 4f - ((float)this.Height / (float)politicalMap.Height) * (float)this.Width;

            mapImageToViewRatio = (float)this.Height / (float)politicalMap.Height;

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Conditions to avoid to proceed further.
            if (backgroundMapKey == null) { return; }

            int curImageSizeX = (int)((float)this.initialHeight * mapImageInnerRatio * zoomFac);
            int curImageSizeY = (int)((float)this.initialHeight * zoomFac);

            Graphics g = e.Graphics;

            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            float pixelSize = curImageSizeX / politicalMap.Width;
            Pen outlinePen = new Pen(Color.Black);
            outlinePen.Width = 0.025f * zoomFac;

            //Map outline
            g.DrawRectangle(new Pen(Color.Black, pixelSize * 2),
                curImageX - pixelSize / 3,
                curImageY - pixelSize / 3,
                curImageSizeX,
                curImageSizeY);

            switch (backgroundMapKey)
            {
                case TERRAIN_KEY:
                    g.DrawImage(terrainMap,
                    curImageX,
                    curImageY,
                    curImageSizeX,
                    curImageSizeY);
                    break;
                default:
                    g.DrawImage(politicalMap,
                    curImageX,
                    curImageY,
                    curImageSizeX,
                    curImageSizeY);
                    break;
            }

            switch (overlayKey)
            {
                case REGIONS_KEY:
                    g.DrawImage(regionsMap,
                    curImageX,
                    curImageY,
                    curImageSizeX,
                    curImageSizeY);
                    break;
                case DEVASTATION_KEY:
                    g.DrawImage(devastationMap,
                    curImageX,
                    curImageY,
                    curImageSizeX,
                    curImageSizeY);
                    break;
                case OCCUPATIONS_KEY:
                    g.DrawImage(occupationsMap,
                    curImageX,
                    curImageY,
                    curImageSizeX,
                    curImageSizeY);
                    break;
                default:
                    break;
            }

            //Map view outline
            g.DrawRectangle(new Pen(Color.Black),
                            0,
                            0,
                            this.Width - 1,
                            this.Height - 1);

            if (selectedPixelIndex != null && pixelSize >= 1)
            {
                    Point onScreenIndex = getOnScreenIndex(selectedPixelIndex.Value);

                    Pen selectionPen = new Pen(Color.Red);
                    selectionPen.Width = 0.025f * zoomFac;

                    g.DrawRectangle(selectionPen,
                            onScreenIndex.X,
                            onScreenIndex.Y,
                            (int)Math.Round(pixelSize),
                            (int)Math.Round(pixelSize));
            }
        }

        private void mouseDown(object? sender, MouseEventArgs e)
        {
            isClicking = true;
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                selectedPixelIndex = getOnArrayIndex(e.X, e.Y);

                if (selectedPixelIndex.Value.X >= 0 && selectedPixelIndex.Value.X < politicalMap.Width
                    && selectedPixelIndex.Value.Y >= 0 && selectedPixelIndex.Value.Y < politicalMap.Height)
                {
                    int index = selectedPixelIndex.Value.X + selectedPixelIndex.Value.Y * politicalMap.Width;

                    int claimColor = politicalImagePixels[index] & 0x00FFFFFF;
                    int terrainColor = terrainImagePixels[index] & 0x00FFFFFF;
                    int regionColor = regionsImagePixels[index] & 0x00FFFFFF;
                    int occupationColor = occupationsImagePixels[index] & 0x00FFFFFF;
                    int devastationColor = devastationImagePixels[index];

                    mapController.showInfoForPixel(index, claimColor, terrainColor, regionColor, occupationColor, devastationColor);

                    this.Invalidate();
                }
            }
            else
            {
                isDragging = false;
            }

            isClicking = false;
        }

        public int getTerrainAt(int index)
        {
            return terrainImagePixels[index] & 0x00FFFFFF;
        }

        public int getWidth()
        {
            return politicalMap.Width;
        }

        private Point getOnArrayIndex(int x, int y)
        {
            float mouseX = (((float)x - curImageX) / mapImageToViewRatio) / zoomFac;
            float mouseY = (((float)y - curImageY) / mapImageToViewRatio) / zoomFac;

            int indexX;
            int indexY;

            if (mouseX > -1 && mouseX < 0)
            {
                indexX = -1;
            } else if (mouseX > politicalMap.Width && mouseX < politicalMap.Width + 1)
            {
                indexX = politicalMap.Width + 1;
            } else
            {
                indexX = (int)Math.Round(mouseX);
            }

            if (mouseY > -1 && mouseY < 0)
            {
                indexY = -1;
            }
            else if (mouseY > politicalMap.Height && mouseY < politicalMap.Height + 1)
            {
                indexY = politicalMap.Width + 1;
            } else
            {
                indexY = (int)Math.Round(mouseY);
            }

            return new Point(indexX, indexY);
        }

        private Point getOnScreenIndex(Point p)
        {
            int indexX = (int)((((float)p.X - 0.5) * mapImageToViewRatio) * zoomFac + curImageX + 1);
            int indexY = (int)((((float)p.Y - 0.5) * mapImageToViewRatio) * zoomFac + curImageY + 1);

            return new Point(indexX, indexY);
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging && isClicking)
            {
                transStartX = e.X;
                transStartY = e.Y;
                isDragging = true;
                return;
            }

            if (isDragging)
            {
                curImageX += (e.X - transStartX);
                curImageY += (e.Y - transStartY);

                transStartX = e.X;
                transStartY = e.Y;

                this.Invalidate();
            }
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            float previousZoom = zoomFac;
            if (e.Delta > 0)
            {
                zoomFac += zoomFac * ZOOM_SPEED;
            }
            else
            {
                zoomFac -= zoomFac * ZOOM_SPEED;
            }



            if (zoomFac > MAX_ZOOM)
            {
                zoomFac = MAX_ZOOM;
            }

            if (zoomFac < MIN_ZOOM)
            {
                zoomFac = MIN_ZOOM;
            }

            if (previousZoom == zoomFac)
            {
                return;
            }

            float zoomChange = zoomFac / previousZoom;

            curImageX = zoomChange * curImageX + (1 - zoomChange) * (float)e.X;
            curImageY = zoomChange * curImageY + (1 - zoomChange) * (float)e.Y;

            this.Invalidate();
        }

        internal void setMapController(MapController mapController)
        {
            this.mapController = mapController;
        }

        public int[] getClaimImagePixels()
        {
            return politicalImagePixels;
        }

        public int[] getTerrainImagePixels()
        {
            return terrainImagePixels;
        }

        public int[] getRegionsImagePixels()
        {
            return regionsImagePixels;
        }

        public int[] getOccupationsImagePixels()
        {
            return occupationsImagePixels;
        }

        public int[] getDevastationImagePixels()
        {
            return devastationImagePixels;
        }
    }
}
