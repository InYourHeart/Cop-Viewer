namespace CoP_Viewer.Source.Util
{
    internal class Logger
    {
        private static String LOG_PATH = "logs/app.log";

        public static void logInfo(String msg)
        {
            String line = DateTime.Now.ToString() + " - Info - " + msg + "\n";

            if (!File.Exists(LOG_PATH))
            {
                FileStream fs = File.Create(LOG_PATH);
                fs.Close();
            }

            File.AppendAllText(LOG_PATH, line);
        }

        public static void logError(String msg)
        {
            String line = DateTime.Now.ToString() + " - Error - " + msg + "\n";

            if (!File.Exists(LOG_PATH))
            {
                FileStream fs = File.Create(LOG_PATH);
                fs.Close();
            }
                
            File.AppendAllText(LOG_PATH, line);
        }
    }
}
