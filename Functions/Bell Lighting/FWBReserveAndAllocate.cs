
/* No longer required, here because I wrote and teted the code so may serve as an example */

var fileName = "FWBReservation";
StringBuilder log = new StringBuilder();
int allocDays = 0;
var dateFrom = DateTime.Today;
var dateTo = DateTime.Today;
bool allocate = false;

log.AppendLine($"Fulfillment Calculation {DateTime.Now}");

var allocDaysCode = Db.UDCodes.Where(r => r.CodeTypeID.ToUpper() == "AUTOALLOC").Where(r => r.CodeID.ToUpper() == "ALLOCDAYS").FirstOrDefault();
var reserveCode = Db.UDCodes.Where(r => r.CodeTypeID.ToUpper() == "AUTOALLOC").Where(r => r.CodeID.ToUpper() == "RES_ALL").FirstOrDefault();

if (allocDaysCode != null) 
{
  Int32.TryParse(allocDaysCode.CodeDesc, out allocDays);
  dateTo = DateTime.Today.AddDays(allocDays + 1);
}

allocate = reserveCode.CodeDesc.ToUpper() == "ALLOCATE";

log.AppendLine($"Window is from {dateFrom} to {dateTo}");

CallService<Ice.Contracts.DynamicQuerySvcContract>(svc =>
{
    using (DataSet queryResults = svc.ExecuteByID("GHA-FWBReserveAndAllocate", new QueryExecutionTableset()))
    {
        try
        {
            var jobCount = queryResults.Tables["Results"].Rows.Count;
            
            log.AppendLine($"{jobCount} lines found");
      
            foreach (var row in queryResults.Tables["Results"].AsEnumerable())
            {
              var orderNum = Convert.ToInt32(row["OrderRel_OrderNum"]);
              var orderLine = Convert.ToInt32(row["OrderRel_OrderLine"]);
              var orderRelNum = Convert.ToInt32(row["OrderRel_OrderRelNum"]);
              var jobNum = row["Calculated_JobNum"].ToString();
              var assemblySeq = Convert.ToInt32(row["Calculated_AssemblySeq"]);
              var mtlSeq = Convert.ToInt32(row["Calculated_MtlSeq"]);


              var shipOrderComplete = Convert.ToBoolean(queryResults.Tables["Results"].Rows[0]["OrderHed_ShipOrderComplete"]);
              DateTime? reqDate = row["OrderRel_ReqDate"] == DBNull.Value ? null : Convert.ToDateTime(row["OrderRel_ReqDate"]);
              
              log.AppendLine($"Sales Order {orderNum}/{orderLine}/{orderRelNum}, Job {jobNum}/{assemblySeq}/{mtlSeq}, {reqDate}");
              
              if (reqDate is null || reqDate < dateFrom || reqDate > dateTo) continue;
              
              log.AppendLine($"Reserve...");
              
              if (string.IsNullOrEmpty(jobNum))
              {
                log.AppendLine(ThisLib.FWBReserveOrder(orderNum, orderLine, orderRelNum));
                if (allocate)
                {
                  log.AppendLine($"Allocate...");
                  log.AppendLine(ThisLib.FWBAllocateOrder(orderNum, orderLine, orderRelNum));
                }
              }
              else
              {
                log.AppendLine(ThisLib.FWBReserveJob(jobNum, assemblySeq, mtlSeq));
                if (allocate)
                {
                  log.AppendLine($"Allocate...");
                  log.AppendLine(ThisLib.FWBAllocateJob(jobNum, assemblySeq, mtlSeq));
                }
              }
              
            }
        }
        catch (Exception ex)
        {
            log.AppendLine($"Query Error {ex.ToString()}");
            ThisLib.Logger($"Query Error {ex.ToString()}", fileName);
        }
    }
});






using (var logger = Ice.Logging.ApplicationLoggerBuilder.CreateDefaultBuilder(this.Session, fileName)    
  .ConfigureFile(options => {options.TruncateFile = true; options.MessageOptions.TimestampFormat = TimestampFormat.DateTime;})
  .Build())
{
    logger.LogInformation(log.ToString());
}