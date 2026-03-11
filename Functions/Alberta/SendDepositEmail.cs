int depositRounding = 0;
decimal depositAmount = 0m;
decimal depositPercentage = 0m;
    
ServiceCallCenterTableset callTs = new ServiceCallCenterTableset();

this.CallService<Erp.Contracts.ServiceCallCenterSvcContract>(callSvc => {
  try
  {
    callTs = callSvc.GetByID(CallNum);
  }
  catch(Exception ex)
  {
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Get service call): {ex}");
    return;
  }
});

QuoteTableset quoteTs = new QuoteTableset();

this.CallService<Erp.Contracts.QuoteSvcContract>(quoteSvc => {
  try
  {
    quoteTs = quoteSvc.GetByID(QuoteNum);
  }
  catch(Exception ex)
  {
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Create service call): {ex}");
    return;
  }
});


this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc => {
  try
  {
      var udCodes = userCodesSvc.GetByID("GHA_QuCl");
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
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Get domestic project manager and credit control manager): {ex}");
    throw new Ice.Common.BusinessObjectException($"Error in CreateCall() function (Get domestic project manager and credit control manager): {ex}");
    return;
  }
});

depositAmount = callTs.FSCallhd[0].UDField<decimal>("GHA_MFS_Deposit_Amount_c");

/***********************************************************************************************************************************************************************
*
*  Get emails
*
***********************************************************************************************************************************************************************/

string callCreator = string.Empty;
string quoteCreator = string.Empty;
string domesticProjectManager = string.Empty;

this.CallService<Ice.Contracts.UserFileSvcContract>(userSvc => {
  try
  {
    var createdBy = callTs.FSCallhd[0].EntryPerson;
    if (!string.IsNullOrEmpty(createdBy)) 
    {
      var user = userSvc.GetByID(createdBy);
       
      if (user.UserFile.Count > 0)
      {
        callCreator = user.UserFile[0].EMailAddress;
      }
    }
  }
  catch(Exception ex)
  {
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Get quote creator): {ex}");
    return;
  }
});


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
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Get quote creator): {ex}");
    return;
  }
});

this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc => {
  try
  {
      var udCodes = userCodesSvc.GetByID("GHA_QuCl");
      var domManCode = udCodes.UDCodes.Where(r => r.CodeID == "DomMan").FirstOrDefault();
      domesticProjectManager = domManCode == null ? "" : domManCode.LongDesc;
  }
  catch(Exception ex)
  {
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Get domestic project manager and credit control manager): {ex}");
    return;
  }
});


/***********************************************************************************************************************************************************************
*
*  Send emails
*
***********************************************************************************************************************************************************************/
  try {
      StringBuilder body = new StringBuilder();
      body.AppendLine($"Customer = {callTs.FSCallhd[0].CustNumCustID}");
      body.AppendLine($"Customer Name - {callTs.FSCallhd[0].CustNumName}");
      body.AppendLine($"Call Number - {callTs.FSCallhd[0].CallNum}");
      body.AppendLine($"Deposit Amount - {depositAmount}");
      body.AppendLine($"Call Total - {quoteTs.QuoteHed[0].DocTotalQuote}");
      body.AppendLine($"Sales Person - {quoteTs.QuoteHed[0].SalesRepName}");
      body.Append("");
     
      var received = Received ? "unreceived" : "received"; //backwards because thats how the dashboard works
     
      var mailer = GetMailer(async: false);
      var message = new Ice.Mail.SmtpMail();
      message.To.Add(domesticProjectManager);
      message.To.Add(callCreator);
      message.To.Add(quoteCreator);
      message.SetFrom("support@alberta.com.mt");
      message.SetSubject($"Deposit has been {received} for service call {CallNum}");
      message.SetBody(body.ToString());
      mailer.Send(message);
  }
  catch(Exception ex)
  {
    Ice.Diagnostics.Log.WriteEntry($"Error in CreateCall() function (Send mail): {ex}");
    return;
  }
