// Get the first "added" or "updated" row from  the dataset.
var ttJobHead_iterator = (from ttJobHead_Row in ds.JobHead
                         where (string.Equals(ttJobHead_Row.RowMod, IceRow.ROWSTATE_ADDED, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(ttJobHead_Row.RowMod, IceRow.ROWSTATE_UPDATED, StringComparison.OrdinalIgnoreCase))
                         select ttJobHead_Row).FirstOrDefault();

// Check if a row exists.
if (ttJobHead_iterator != null)
{
  // Check if job is being engineered and is a manufactured job
   if ((ttJobHead_iterator.JobEngineered == true && ttJobHead_iterator.JobType == "MFG" ) ||
      ( ttJobHead_iterator.JobReleased == true && ttJobHead_iterator.JobType == "MFG"))
  { 
    // Check for sales Order on Hold via JobProd
		JobProd JobProd = (from row in Db.JobProd 
			where row.Company == Session.CompanyID && 
			row.JobNum == ttJobHead_iterator.JobNum &&
			row.OrderNum != 0
      select row).FirstOrDefault();
			if (JobProd != null)
			{
				OrderHed OrderHed = (from row in Db.OrderHed 
					where row.Company == Session.CompanyID &&
					row.OrderNum == JobProd.OrderNum &&
					row.OrderHeld == true
        	select row).FirstOrDefault();
					if (OrderHed != null)
					{
						var messageText = "Engineered not allowed - Sales Order is on hold";
    				this.PublishInfoMessage(messageText, Ice.Common.BusinessObjectMessageType.Information, Ice.Bpm.InfoMessageDisplayMode.Individual, "JobEntry", "ChangeJobHeadJobEngineered");
						ttJobHead_iterator.JobEngineered = false;
						ttJobHead_iterator.JobReleased = false;
  				}
			}
   }           
}
