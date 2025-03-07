namespace CoP_Viewer.Source.Util
{
    internal class Logger
    {
        private static String LOG_PATH = "logs/app.log";

        public static void logError(String msg)
        {
            File.WriteAllText(LOG_PATH, DateTime.Now.ToString() + " - " + msg);
        }
    }
}
