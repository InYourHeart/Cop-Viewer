using CoP_Viewer.Source.Controller;
using CoP_Viewer.Source.UI;
using CoP_Viewer.Source.Util;
using Microsoft.VisualBasic.FileIO;
using System.Collections;
using System.Configuration;

namespace CoP_Viewer.Source
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            MapView mapView = new();
            InfoView infoView = new();

            MapController mapController = new(mapView, infoView);

            Logger.logInfo("Application started");
            Application.Run(new MainForm(mapView, infoView));
        }
    }
}