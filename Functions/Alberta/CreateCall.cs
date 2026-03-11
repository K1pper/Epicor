//Ice.Diagnostics.Log.WriteEntry($"Creating call from quote {QuoteNum}...");

QuoteTableset quoteTs = new QuoteTableset();
string CallType = string.Empty;
Dictionary<int, string> TopicList = new Dictionary<int, string>();
Dictionary<int, string> JobList = new Dictionary<int, string>();

Guid ServiceCallSysRowId = Guid.Empty;
bool EmailSent = false;

Message = string.Empty;
Success = false;

/* Assumptions */
const string QuoteReasonCode = "CRW01";

/***********************************************************************************************************************************************************************
*
*  Retreive the quote
*
***********************************************************************************************************************************************************************/

bool isFixedCost = false;
decimal fixedCost = 0m;

this.CallService<Erp.Contracts.QuoteSvcContract>(quoteSvc => {
  try
  {
    quoteTs = quoteSvc.GetByID(QuoteNum);
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Create service call): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

if (quoteTs.QuoteDtl.Count == 0) 
{
  Message = $"Quote does not have any lines therefore a service call cannot be created";
  return;
}


/***********************************************************************************************************************************************************************
*
*  Get quote assembly
*
***********************************************************************************************************************************************************************/

var quoteAsmTs = new QuoteAsmTableset(); 

this.CallService<Erp.Contracts.QuoteAsmSvcContract>(quoteAsmSvc => {
  try
  {
    quoteAsmTs = quoteAsmSvc.GetRows($"QuoteNum = {QuoteNum}", "", "", "", "", "", "", "", "", "", "", "", "", 0, 1, out bool success);    
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Get quote assembly): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

if (quoteAsmTs.QuoteOpr.Count > 0)
{
  isFixedCost = quoteAsmTs.QuoteOpr[0].UDField<bool>("GHA_FixedCost_c");
  fixedCost = quoteAsmTs.QuoteOpr[0].UDField<decimal>("GHA_FixedCostPrice_c");
}

/***********************************************************************************************************************************************************************
*
*  Check the sales category for Call Type and Topic
*
***********************************************************************************************************************************************************************/

this.CallService<Erp.Contracts.SalesCatSvcContract>(salesCatSvc => {
  try
  {
    var catTs = new SalesCatTableset();
    foreach (var quote in quoteTs.QuoteDtl)
    {
      catTs = salesCatSvc.GetByID(quote.SalesCatID);
      if (catTs.SalesCat.Count == 0) 
      {
        Message = $"Sales category does not exist";
        return;
      }
      if (string.IsNullOrEmpty(catTs.SalesCat[0].UDField<string>("GHA_SalesCat_CallType_c"))) 
      {
        Message = $"Sales category on line {quote.QuoteLine} does not have a valid call type";
        throw new Ice.Common.BusinessObjectException(Message);
        return;
      }
      CallType = catTs.SalesCat[0].UDField<string>("GHA_SalesCat_CallType_c");
      if (string.IsNullOrEmpty(catTs.SalesCat[0].UDField<string>("GHA_SalesCat_Topic_c"))) 
      {
        Message = $"Sales category on line {quote.QuoteLine} does not have a valid topic";
        throw new Ice.Common.BusinessObjectException(Message);
        return;
      }
      TopicList.Add(quote.QuoteLine, catTs.SalesCat[0].UDField<string>("GHA_SalesCat_Topic_c"));
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Create service call): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

/***********************************************************************************************************************************************************************
*
*  Get email and deposit details
*
***********************************************************************************************************************************************************************/

string quoteCreator = string.Empty;
string domesticProjectManager = string.Empty;
string creditControlManager = string.Empty;
string reportStyle = string.Empty;
decimal depositPercentage = 0m;
int depositRounding = 0;
decimal depositAmount = 0m;

this.CallService<Ice.Contracts.UserFileSvcContract>(userSvc => {
  try
  {
    var createdBy = quoteTs.QuoteHed[0].UDField<string>("GHA_AQS_CreatedBy_c");
    if (!string.IsNullOrEmpty(createdBy)) 
    {
      var user = userSvc.GetByID(createdBy);
       
      if (user.UserFile.Count > 0)
      {
        quoteCreator = user.UserFile[0].EMailAddress;
      }
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Get quote creator): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc => {
  try
  {
      var udCodes = userCodesSvc.GetByID("GHA_QuCl");
      var domManCode = udCodes.UDCodes.Where(r => r.CodeID == "DomMan").FirstOrDefault();
      domesticProjectManager = domManCode == null ? "" : domManCode.LongDesc;
      var credManCode = udCodes.UDCodes.Where(r => r.CodeID == "CredMan").FirstOrDefault();
      creditControlManager = credManCode == null ? "" : credManCode.LongDesc;
      var reportStyleCode = udCodes.UDCodes.Where(r => r.CodeID == "QuoteStyle").FirstOrDefault();
      reportStyle = reportStyleCode == null ? "" : reportStyleCode.CodeDesc;
      var depositPercentageCode = udCodes.UDCodes.Where(r => r.CodeID == "DepPerc").FirstOrDefault();
      if (depositPercentageCode != null)
      {
        decimal.TryParse(depositPercentageCode.CodeDesc, out depositPercentage);
      }
      var depositRoundingCode = udCodes.UDCodes.Where(r => r.CodeID == "Round").FirstOrDefault();
      if (depositRoundingCode != null)
      {
        int.TryParse(depositRoundingCode.CodeDesc, out depositRounding);
      }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Get domestic project manager and credit control manager): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

/***********************************************************************************************************************************************************************
*
*  Mark the quote as won
*
***********************************************************************************************************************************************************************/

this.CallService<Erp.Contracts.TaskSvcContract>(taskSvc => {
  try
  {
    if (quoteTs.QuoteHed[0]["PONum"] != "818697")
    {
  
      string message;
  
      var filter = $"RelatedToFile = 'QuoteHed' and Key1 = '{QuoteNum}' and TaskID = 'TASK01' and TaskSetID = 'TSKSET01' and TypeCode = 'TSKTYP01'";
  
      var checkTask = taskSvc.GetList(filter, 0, 1, out bool result);

      if (checkTask.TaskList.Count() == 0) return;

      var taskSeqNum = checkTask.TaskList[0].TaskSeqNum;

      var task = taskSvc.GetByID("QuoteHed", QuoteNum.ToString(), string.Empty, string.Empty, taskSeqNum);

      if (task.Task.Count == 0) return;
  
      task.Task[0].Complete = true;
      task.Task[0].Conclusion = "NEXT";
      task.Task[0].RowMod = "U";
      taskSvc.ChangeComplete(ref task, true, out message);

      task.Task[0].Conclusion = "WIN";
      task.Task[0].ReasonCode = QuoteReasonCode;
      task.Task[0].RowMod = "U";
      taskSvc.Update(ref task);
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Mark quote as won): {ex})";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

/***********************************************************************************************************************************************************************
*
*  Calculate Deposit Amount
*
***********************************************************************************************************************************************************************/

depositAmount = quoteTs.QuoteHed[0].DocTotalQuote - quoteTs.QuoteHed[0].DocTax;

depositAmount = depositAmount * depositPercentage / 100;

switch (depositRounding)
{
  case 5:
  case 10:
  case 50:
  case 100:
    depositAmount = Math.Ceiling(depositAmount / depositRounding) * depositRounding;
    break;
}

/***********************************************************************************************************************************************************************
*
*  Create service call
*
***********************************************************************************************************************************************************************/

this.CallService<Erp.Contracts.ServiceCallCenterSvcContract>(callSvc => {
  try
  {
    var callTs = new ServiceCallCenterTableset();
    callSvc.GetNewFSCallhd(ref callTs);
    
    var call = callTs.FSCallhd[0];
    var quote = quoteTs.QuoteHed[0];
    
    callSvc.ChangeHdrCustID(quote.CustomerCustID, ref callTs);
    
    call.CustNum = quote.CustNum;
    call.BTCustNum = quote.BTCustNum;  
    call.ShipToCustNum = quote.ShipToCustNum;
    call.PrcConNum = quote.PrcConNum;
    call.ShipToNum = quote.ShipToNum;
    call.ShpConNum = quote.ShpConNum;
    call.UseOTS = quote.UseOTS;
    call.OTSName = quote.OTSName;
    call.OTSAddress1 = quote.OTSAddress1;
    call.OTSAddress2 = quote.OTSAddress2;
    call.OTSAddress3 = quote.OTSAddress3;
    call.OTSCity = quote.OTSCity;
    call.OTSState = quote.OTSState;
    call.OTSZIP = quote.OTSZIP;
    call.OTSContact = quote.OTSContact;
    call.OTSPhoneNum = quote.OTSPhoneNum;
    call.OTSCountryNum = quote.OTSCountryNum;
    call.CallCode = CallType;
    call.SetUDField<int>("GHA_MFS_QuoteNum_c", quoteTs.QuoteHed[0].QuoteNum); 
    call.SetUDField<string>("GHA_MFS_CallSource_c", quoteTs.QuoteHed[0].UDField<bool>("GHA_AQS_Quote_c") ? "AQS" : "QUOTE"); 
    call.SetUDField<decimal>("GHA_MFS_Deposit_Amount_c", depositAmount); 
    call.SetUDField<bool>("GHA_MFS_DepositRequired_c", depositAmount > 0);
    
    call.RowMod = "A";
    
    //string output = JsonConvert.SerializeObject(call);
    //Ice.Diagnostics.Log.WriteEntry(output);
    
    callSvc.Update(ref callTs);
    
    ServiceCall = callTs.FSCallhd[0].CallNum;
    ServiceCallSysRowId = callTs.FSCallhd[0].SysRowID;
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Create service call): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});

/***********************************************************************************************************************************************************************
*
*  Create service call lines
*
***********************************************************************************************************************************************************************/

this.CallService<Erp.Contracts.ServiceCallCenterSvcContract>(callSvc => {
  try
  {
    foreach (var quote in quoteTs.QuoteDtl)
    {
      var callTs = new ServiceCallCenterTableset();
      callSvc.GetNewFSCallDt(ref callTs, ServiceCall);
    
      var call = callTs.FSCallDt.Where(r => r.Added()).FirstOrDefault();
      
      call.PartNum = quote.PartNum;
      call.RowMod = "A";
      
      callSvc.ChangeDtlPartNum(ref callTs, quote.PartNum); 
      
      StringBuilder partDescription = new StringBuilder();
      
      partDescription.AppendLine(quote.LineDesc);

      if (!string.IsNullOrEmpty(quoteTs.QuoteHed[0].QuoteComment))
      {
        partDescription.AppendLine("");
        partDescription.AppendLine(quoteTs.QuoteHed[0].QuoteComment);
      }
      if (!string.IsNullOrEmpty(quote.QuoteComment))
      {
        partDescription.AppendLine("");
        partDescription.AppendLine(quote.QuoteComment);
      }
      call.PartDescription = partDescription.ToString();
      call.IUM = quote.SellingExpectedUM;
      call.CallQty =  quote.OrderQty;
      if (TopicList.TryGetValue(quote.QuoteLine, out string value))
      {
        call.IssueTopicID1 = value;
      }
      else
      {
        Message = $"Topic not found for sales category on quoteline {quote.QuoteLine}";
      }
      if (isFixedCost)
      {
        call.SetUDField<bool>("GHA_MFS_FixedCost_c", true);
        call.SetUDField<string>("GHA_MFS_TimeEntryMethod_c", "FC");
        call.SetUDField<string>("GHA_MFS_LabourEntryMethod_c", "FC");
        call.SetUDField<decimal>("GHA_MFS_TravelFixedCost_c", 0);
        call.SetUDField<decimal>("GHA_MFS_FixedLabourRate_c", fixedCost);
      }
      call.RowMod = "A";
      callSvc.Update(ref callTs);
      
/***********************************************************************************************************************************************************************
*
*  Create service calls
*
***********************************************************************************************************************************************************************/

      callSvc.CreateServiceCallJob(ServiceCall, call.CallLine, out string createJobMessage, ref callTs); 
      
      var added = callTs.FSCallDt.Where(r => r.JobNum != "").FirstOrDefault();
      if (added != null)
      {
        JobList.Add(quote.QuoteLine, added.JobNum);
      }
           
      

/***********************************************************************************************************************************************************************
*
*  Send emails
*
***********************************************************************************************************************************************************************/

      if (!EmailSent)
      {

      EmailSent = true;

      StringBuilder internalComments = new StringBuilder();

      this.CallService<Erp.Contracts.CRMCallSvcContract>(crmCallSvc => {
      try
      {
          CRMCallTableset calls = crmCallSvc.GetRows($"relatedToFile = 'quotehed' and Key1 = '{QuoteNum}'", string.Empty, string.Empty, string.Empty, 0, 1, out bool crmCallSuccess);
          foreach (CRMCallRow crmCall in calls.CRMCall)
          {
            internalComments.AppendLine(string.Format(crmCall.OrigDate.ToString(), "dd/MM/yyyy"));
            internalComments.AppendLine(crmCall.CallDesc);
            internalComments.AppendLine(crmCall.CallText);
            internalComments.AppendLine("");
          }
      }
      catch(Exception ex)
      {
        Message = "Error in CreateCall() function (Get internal comments): {ex}";
        Ice.Diagnostics.Log.WriteEntry(Message);
        throw new Ice.Common.BusinessObjectException(Message);
        return;
      }
    });
      
      StringBuilder body = new StringBuilder();
      body.AppendLine($"Customer = {callTs.FSCallhd[0].CustNumCustID}");
      body.AppendLine($"Customer Name - {callTs.FSCallhd[0].CustNumName}");
      body.AppendLine($"Call Number - {callTs.FSCallhd[0].CallNum}");
      body.AppendLine($"Deposit Amount - {depositAmount}");
      body.AppendLine($"Call Total - {quoteTs.QuoteHed[0].DocTotalQuote}");
      body.AppendLine($"Sales Person - {quoteTs.QuoteHed[0].SalesRepName}");
      body.Append("");
      body.AppendLine($"Internal Comments -");
      body.Append(internalComments);
      body.Append("");
      body.AppendLine($"System Type - {call.PartNum}");
     
     
     var reportStyleCheck = Int32.TryParse(reportStyle, out int quoteReportStyle);
     
     string emailTo = string.Join(";", new[] { domesticProjectManager, creditControlManager, quoteCreator }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct());
    
     
     if (SendQuote && reportStyleCheck)
     {
      this.CallService<Erp.Contracts.QuotFormSvcContract>(quoteFormSvc => {
    
      try
      {      
        var quoteFormTs = new QuotFormTableset();
        QuoteFormParamRow row = new QuoteFormParamRow();
        row.QuoteNum = QuoteNum;
        row.AutoAction = "SSRSPrint";
        row.AgentID = "SystemTaskAgent";
        row.ReportStyleNum = quoteReportStyle;
        row.WorkstationID = this.Session.WorkstationID;
        row.SSRSRenderFormat = "PDF";
        row.FaxSubject = $"Service call {ServiceCall}, service job {call.JobNum} has been created from quote {QuoteNum}";
        row.EMailTo = emailTo; 
        row.EMailBody = body.ToString();
        row.RowMod = "A";
        quoteFormTs.QuoteFormParam.Add(row);
           
        quoteFormSvc.SubmitToAgent(quoteFormTs, "SystemTaskAgent", 0, 0, "Erp.UIRpt.QuotForm");      
      }
      catch(Exception ex)
      {
        Message = $"Error in CreateCall() function (Submit quote form): {ex}";
        Ice.Diagnostics.Log.WriteEntry(Message);
        throw new Ice.Common.BusinessObjectException(Message);
        return;
      }
          });
          
     }
     else
     {
        var mailer = GetMailer(async: false);
        var message = new Ice.Mail.SmtpMail();
        message.SetTo(emailTo);
        message.SetFrom("support@alberta.com.mt");
        message.SetSubject($"Service call {ServiceCall}, service job {call.JobNum} has been created from quote {quoteTs.QuoteHed[0].QuoteNum}");
        message.SetBody(body.ToString());
        mailer.Send(message);
      }
      
      }

    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Create service call lines): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});


/***********************************************************************************************************************************************************************
*
*  Copy attachments
*
***********************************************************************************************************************************************************************/

/* First find the attachments */

foreach (var quoteHedAttch in quoteTs.QuoteHedAttch)
{
  var job = Db.JobHead.Where(r => r.Company == Session.CompanyID).Where(r => r.CallNum == ServiceCall).FirstOrDefault();

  this.CallService<Ice.Contracts.AttachmentSvcContract>(attachSvc => {
    try
    {
      var attachTs = new AttachmentTableset();
      attachSvc.GetNewXFileAttch(ref attachTs, "Erp", "FsCallHd", ServiceCallSysRowId);
      
      var att = attachTs.XFileAttch.Where(r => r.Added()).FirstOrDefault();
      
      if (att != null)
      {
        att.RelatedToSchemaName = "Erp";
        att.RelatedToFile = "FSCallhd";
        att.Key1 = ServiceCall.ToString();
        att.ForeignSysRowID = ServiceCallSysRowId;
        att.XFileRefXFileName = quoteHedAttch.FileName;
        att.XFileRefXFileDesc = quoteHedAttch.DrawDesc;
        attachSvc.Update(ref attachTs);
        
        attachSvc.GetNewXFileAttch(ref attachTs, "Erp", "JobHead", ServiceCallSysRowId);
      }
      att = attachTs.XFileAttch.Where(r => r.Added()).FirstOrDefault();
      
      if (att != null)
      {
        att.RelatedToSchemaName = "Erp";
        att.RelatedToFile = "JobHead";
        att.Key1 = job.JobNum;
        att.ForeignSysRowID = job.SysRowID;
        att.XFileRefXFileName = quoteHedAttch.FileName;
        att.XFileRefXFileDesc = quoteHedAttch.DrawDesc;
        attachSvc.Update(ref attachTs);
      }
    }
    catch(Exception ex)
    {
      Message = $"Error in CreateCall() function (Copy attachments): {ex}";
      Ice.Diagnostics.Log.WriteEntry(Message);
      throw new Ice.Common.BusinessObjectException(Message);
      return;
    }
  });
};

foreach (var quoteDtlAttch in quoteTs.QuoteDtlAttch)
{
  var callDtl = Db.FSCallDt.Where(r => r.Company == Session.CompanyID).Where(r => r.CallNum == ServiceCall).Where(r => r.CallLine == quoteDtlAttch.QuoteLine).FirstOrDefault();
  var job = Db.JobHead.Where(r => r.Company == Session.CompanyID).Where(r => r.CallNum == ServiceCall).Where(r => r.CallLine == quoteDtlAttch.QuoteLine).FirstOrDefault();

  this.CallService<Ice.Contracts.AttachmentSvcContract>(attachSvc => {
    try
    {
      var attachTs = new AttachmentTableset();
      attachSvc.GetNewXFileAttch(ref attachTs, "Erp", "FsCallDt", ServiceCallSysRowId);
      
      var att = attachTs.XFileAttch.Where(r => r.Added()).FirstOrDefault();
      
      if (att != null)
      {
        att.RelatedToSchemaName = "Erp";
        att.RelatedToFile = "FSCallDt";
        att.Key1 = ServiceCall.ToString();
        att.Key2 = callDtl.CallLine.ToString();
        att.ForeignSysRowID = callDtl.SysRowID;
        att.XFileRefXFileName = quoteDtlAttch.FileName;
        att.XFileRefXFileDesc = quoteDtlAttch.DrawDesc;
        attachSvc.Update(ref attachTs);
      }
      
      attachSvc.GetNewXFileAttch(ref attachTs, "Erp", "JobHead", ServiceCallSysRowId);
      
      att = attachTs.XFileAttch.Where(r => r.Added()).FirstOrDefault();
      
      if (att != null)
      {
        att.RelatedToSchemaName = "Erp";
        att.RelatedToFile = "JobHead";
        att.Key1 = job.JobNum;
        att.ForeignSysRowID = job.SysRowID;
        att.XFileRefXFileName = quoteDtlAttch.FileName;
        att.XFileRefXFileDesc = quoteDtlAttch.DrawDesc;
        attachSvc.Update(ref attachTs);
      }
      
    }
    catch(Exception ex)
    {
      Message = $"Error in CreateCall() function (Copy attachments): {ex}";
      Ice.Diagnostics.Log.WriteEntry(Message);
      throw new Ice.Common.BusinessObjectException(Message);
      return;
    }
  });
};

/***********************************************************************************************************************************************************************
*
*  Copy Operations to Job
*
***********************************************************************************************************************************************************************/

this.CallService<Erp.Contracts.JobEntrySvcContract>(jobSvc => {
  try
  {
    foreach (var quoteOpr in quoteAsmTs.QuoteOpr.OrderBy(r => r.QuoteLine).OrderBy(r => r.OprSeq))
    {
      if (JobList.TryGetValue(quoteOpr.QuoteLine, out string jobNum))
      {
        var job = jobSvc.GetByID(jobNum);

        jobSvc.GetNewJobOper(ref job, jobNum, quoteOpr.AssemblySeq);
        
        var opr = job.JobOper.Where(r => r.Added()).FirstOrDefault();
        
        opr.OpCode = quoteOpr.OpCode;
        opr.OpDesc = quoteOpr.OpDesc;
        opr.OpStdID = quoteOpr.OpStdID;
        //opr.PrimaryProdOpDtl = quoteOpr.PrimaryProdOpDtl;
        //opr.PrimarySetupOpDtl = quoteOpr.PrimarySetupOpDtl;
        opr.LaborEntryMethod = "T";
        opr.QtyPer = quoteOpr.QtyPer;
        opr.ProdStandard = quoteOpr.StdFormat == "MP" ? quoteOpr.ProdStandard * quoteOpr.QtyPer / 60 : quoteOpr.ProdStandard;
        opr.StdFormat = quoteOpr.StdFormat == "MP" ? "HR" : quoteOpr.StdFormat;
        opr.Billable = quoteOpr.UDField<bool>("GHA_Billable_c");
        opr.EstLabHours = opr.StdFormat == "HR" ? opr.ProdStandard : 0;
        opr.LaborRate = quoteOpr.UDField<decimal>("GHA_LabourRate_c");
        opr.BillableLaborRate = opr.Billable ? quoteOpr.UDField<decimal>("GHA_LabourRate_c") : 0;
        opr.DocLaborRate = quoteOpr.UDField<decimal>("GHA_LabourRate_c");
        opr.DocBillableLaborRate = opr.Billable ? quoteOpr.UDField<decimal>("GHA_LabourRate_c") : 0;
        if (quoteOpr.UDField<bool>("GHA_FixedCost_c"))
        {
          opr.ProdStandard = 1;
          opr.EstLabHours = 1;
          opr.LaborRate = quoteOpr.UDField<decimal>("GHA_FixedCostPrice_c");
        }
        opr.RowMod = "A";
        
        
        jobSvc.Update(ref job);
      }
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Copy operations): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});   

/***********************************************************************************************************************************************************************
*
*  Copy Materials to Job
*
***********************************************************************************************************************************************************************/

//foreach(var job in JobList)
//{
//  Ice.Diagnostics.Log.WriteEntry("Dictionary: " + job.Key.ToString() + " " + job.Value);
//}

this.CallService<Erp.Contracts.JobEntrySvcContract>(jobSvc => {
  try
  {
    foreach (var quoteMtl in quoteAsmTs.QuoteMtl.OrderBy(r => r.QuoteLine).OrderBy(r => r.MtlSeq))
    {
      if (JobList.TryGetValue(quoteMtl.QuoteLine, out string jobNum))
      {
        var job = jobSvc.GetByID(jobNum);

        jobSvc.GetNewJobMtl(ref job, jobNum, quoteMtl.AssemblySeq);
        
        var mtl = job.JobMtl.Where(r => r.Added()).FirstOrDefault();
        
        var partNum = quoteMtl.PartNum;
        mtl.PartNum = partNum;
        
        jobSvc.ChangeJobMtlPartNum(
          ref job, 
          false, 
          ref partNum, 
          Guid.Empty, 
          "", 
          "", 
          out string vMsgText,
          out bool vSubAvail, 
          out string vMsgType, 
          out bool multipleMatch, 
          out bool opPartChgCompleted, 
          out string opMtlIssuedAction);
        
        mtl.Description = quoteMtl.Description;
        mtl.RelatedOperation = quoteMtl.RelatedOperation;
        mtl.QtyPer = quoteMtl.QtyPer;
        mtl.RequiredQty = quoteMtl.RequiredQty;
        mtl.IUM = quoteMtl.IUM;
        mtl.Billable = quoteMtl.UDField<bool>("GHA_Billable_c");
        mtl.UnitPrice = quoteMtl.UDField<decimal>("GHA_DiscountedPrice_c");
        mtl.BillableUnitPrice = quoteMtl.UDField<decimal>("GHA_DiscountedPrice_c");
        mtl.DocUnitPrice = quoteMtl.UDField<decimal>("GHA_DiscountedPrice_c");
        mtl.DocBillableUnitPrice = quoteMtl.UDField<decimal>("GHA_DiscountedPrice_c");

        mtl.RowMod = "A";
        
        
        jobSvc.Update(ref job);
      }
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Copy materials): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});   

/***********************************************************************************************************************************************************************
*
*  Engineer and release job
*
***********************************************************************************************************************************************************************/
/*
this.CallService<Erp.Contracts.JobEntrySvcContract>(jobSvc => {
  try
  {
    foreach(var j in JobList)
    {
      var job = jobSvc.GetByID(j.Value);
      Ice.Diagnostics.Log.WriteEntry(j.Value);
      Ice.Diagnostics.Log.WriteEntry(j.Key.ToString());
      
      var qd = quoteTs.QuoteDtl.Where(r => r.QuoteLine == j.Key).FirstOrDefault();
      if (qd == null)
      {
        Ice.Diagnostics.Log.WriteEntry("quote not found");
        continue;
      }
      
      //Ice.Diagnostics.Log.WriteEntry(qd.SalesCatID);
      
      //job.JobHead[0].UserChar1 = qd.SalesCatID;
      //job.JobHead[0].JobEngineered = true;
      //job.JobHead[0].RowMod = "U";
      //jobSvc.ChangeJobHeadJobEngineered(ref job);
      
      //Ice.Diagnostics.Log.WriteEntry("engineerred");
      
      //job.JobHead[0].UserChar1 = qd.SalesCatID;
      //job.JobHead[0].JobEngineered = true;
      //job.JobHead[0].JobReleased = true;
      //job.JobHead[0].EnableJobFirm = false;
      //job.JobHead[0].RowMod = "U";
      //jobSvc.ChangeJobHeadJobReleased(ref job);
      
      //Ice.Diagnostics.Log.WriteEntry("released");
      
      //job.JobHead[0].UserChar1 = qd.SalesCatID;
      job.JobHead[0].JobEngineered = true;
      job.JobHead[0].JobReleased = true;
      job.JobHead[0].EnableJobFirm = false;
      job.JobHead[0].OrigProdQty = job.JobHead[0].ProdQty;
      //job.JobHead[0].PartDescription = "Fire Detection Apollo - change";
      job.JobHead[0].RowMod = "U";
      
      Ice.Diagnostics.Log.WriteEntry(JsonConvert.SerializeObject(job.JobHead[0]));
      
      Ice.Diagnostics.Log.WriteEntry("updating.....");
      
      Ice.Diagnostics.Log.WriteEntry(JsonConvert.SerializeObject(job.JobHead[0]));
        
        
      jobSvc.Update(ref job);
      Ice.Diagnostics.Log.WriteEntry("updated");
    }
  }
  catch(Exception ex)
  {
    Message = $"Error in CreateCall() function (Release job): {ex}";
    Ice.Diagnostics.Log.WriteEntry(Message);
    if (ex.InnerException != null)
      Ice.Diagnostics.Log.WriteEntry(ex.InnerException.Message);
    throw new Ice.Common.BusinessObjectException(Message);
    return;
  }
});   
*/

Success = string.IsNullOrEmpty(Message);

//Ice.Diagnostics.Log.WriteEntry($"Call {ServiceCall} created...");