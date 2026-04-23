var debug = false;

if (debug)
{
    string fileName = "MandataLog";

    using (var logger = Ice.Logging.ApplicationLoggerBuilder.CreateDefaultBuilder(this.Session, fileName).Build())
    {
        logger.LogInformation(Message);
    }
}