////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

var debug = false;

if (debug)
{
    Ice.Diagnostics.Log.WriteEntry(Function + " : " + Message);

    string fileName = "GHALog" + Function;

    using (var logger = Ice.Logging.ApplicationLoggerBuilder.CreateDefaultBuilder(this.Session, fileName)
      .ConfigureFile(options => options.MessageOptions.TimestampFormat = TimestampFormat.DateTime)
      .Build())
    {
        logger.LogInformation(Message);
    }
}

