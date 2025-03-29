using CoP_Viewer.Source.UI;

namespace CoP_Viewer
{
    public partial class MainForm : Form
    {
        //Constants
        private const int MIN_HEIGHT = 720;
        private const int MIN_WIDTH = 1280;

        //UI elements
        private MapView mapView;
        private InfoView infoView;

        Button politicalMapButton;
        Button terrainMapButton;
        Button regionsMapButton;
        Button occupationsMapButton; 
        Button devastationMapButton;

        public MainForm(MapView mapView, InfoView infoView)
        {
            InitializeComponent();

            this.mapView = mapView;
            this.infoView = infoView;
        }

        private void MapView_Load(object sender, EventArgs e)
        {
            this.Controls.Add(mapView);
            this.Controls.Add(infoView);

            politicalMapButton = new Button();
            politicalMapButton.Text = "Political";
            politicalMapButton.Click += Change_Map_To_Political;
            terrainMapButton = new Button();
            terrainMapButton.Text = "Terrain";
            terrainMapButton.Click += Change_Map_To_Terrain;
            regionsMapButton = new Button();
            regionsMapButton.Text = "Regions";
            regionsMapButton.Click += Change_Map_To_Regions;
            occupationsMapButton = new Button();
            occupationsMapButton.Text = "Occupations";
            occupationsMapButton.Click += Change_Map_To_Occupations;
            devastationMapButton = new Button();
            devastationMapButton.Text = "Devastation";
            devastationMapButton.Click += Change_Map_To_Devastation;

            this.Controls.Add(politicalMapButton);
            this.Controls.Add(terrainMapButton);
            this.Controls.Add(regionsMapButton);
            this.Controls.Add(occupationsMapButton);
            this.Controls.Add(devastationMapButton);

            this.Height = (int)(Screen.FromControl(this).Bounds.Height * 0.9);
            this.Width = (int)(Screen.FromControl(this).Bounds.Width * 0.9);
            this.Location = new Point((int)(Screen.FromControl(this).Bounds.Width * 0.05),
                                        (int)(Screen.FromControl(this).Bounds.Height * 0.05));

            ResizeMapView();
            ResizeInfoView();
            ResizeMapButtons();
        }

        private void Change_Map_To_Political(object? sender, EventArgs e)
        {
            mapView.setMapImage(MapView.POLITICAL_KEY);
        }

        private void Change_Map_To_Terrain(object? sender, EventArgs e)
        {
            mapView.setMapImage(MapView.TERRAIN_KEY);
        }

        private void Change_Map_To_Regions(object? sender, EventArgs e)
        {
            mapView.setMapImage(MapView.REGIONS_KEY);
        }

        private void Change_Map_To_Occupations(object? sender, EventArgs e)
        {
            mapView.setMapImage(MapView.OCCUPATIONS_KEY);
        }

        private void Change_Map_To_Devastation(object? sender, EventArgs e)
        {
            mapView.setMapImage(MapView.DEVASTATION_KEY);
        }

        private void Views_Resize(object sender, EventArgs e)
        {
            ResizeMapView();
            ResizeInfoView();
            ResizeMapButtons();
        }

        private void ResizeMapButtons()
        {
            int pad = 10;
            int h = 30;
            int w = (int)(this.ClientRectangle.Width * 0.25 / 5);
            int newPosX = (int)(this.Width - (int)(this.ClientRectangle.Width * 0.25) - this.Height * 0.05);
            int newPosY = (int)(this.Height * 0.025) + (int)(this.ClientRectangle.Height * 0.45) + pad;

            politicalMapButton.Height = h;
            politicalMapButton.Width = w;
            politicalMapButton.Location = new Point(newPosX, newPosY);
            terrainMapButton.Height = h;
            terrainMapButton.Width = w;
            terrainMapButton.Location = new Point(newPosX + w, newPosY);
            regionsMapButton.Height = h;
            regionsMapButton.Width = w;
            regionsMapButton.Location = new Point(newPosX + w * 2, newPosY);
            occupationsMapButton.Height = h;
            occupationsMapButton.Width = w;
            occupationsMapButton.Location = new Point(newPosX + w * 3, newPosY);
            devastationMapButton.Height = h;
            devastationMapButton.Width = w;
            devastationMapButton.Location = new Point(newPosX + w * 4, newPosY);
        }

        private void ResizeMapView()
        {
            int newHeight = (int)(this.ClientRectangle.Height * 0.95);
            int newWidth = (int)(this.ClientRectangle.Width * 0.70);
            int newPosX = (int)(this.Height * 0.025);
            int newPostY = (int)(this.Height * 0.025);

            mapView.ResizeView(newHeight, newWidth, new Point(newPosX, newPostY));
        }

        private void ResizeInfoView()
        {
            int newHeight = (int)(this.ClientRectangle.Height * 0.45);
            int newWidth = (int)(this.ClientRectangle.Width * 0.25);
            int newPosX = (int)(this.Width - newWidth - this.Height * 0.05);
            int newPostY = (int)(this.Height * 0.025);

            infoView.ResizeView(newHeight, newWidth, new Point(newPosX, newPostY));
        }
    }
}
