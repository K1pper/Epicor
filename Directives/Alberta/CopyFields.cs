var sourceQuoteDs = new Erp.Tablesets.QuoteTableset();
var destinationQuoteDs = new Erp.Tablesets.QuoteTableset();

using (var quoteSvc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.QuoteSvcContract>(Db))
{
  destinationQuoteDs = quoteSvc.GetByID(result);
  
  if (destinationQuoteDs== null)
    return;
    
  if (!destinationQuoteDs.QuoteHed.Any() || !destinationQuoteDs.QuoteDtl.Any())
    return;
  
  sourceQuoteDs = quoteSvc.GetByID(ipSourceQuoteNum);
  
  if (sourceQuoteDs == null)
    return;
  
  if (!sourceQuoteDs.QuoteHed.Any() || !sourceQuoteDs.QuoteDtl.Any())
    return;
  
  destinationQuoteDs.QuoteDtl[0].SellingExpectedQty = sourceQuoteDs.QuoteDtl[0].SellingExpectedQty;
  destinationQuoteDs.QuoteDtl[0].OrderQty = sourceQuoteDs.QuoteDtl[0].OrderQty;
  
  destinationQuoteDs.QuoteDtl[0].RowMod = "U";
  
  quoteSvc.ChangeSellingExpQty(ref destinationQuoteDs);
  quoteSvc.Update(ref destinationQuoteDs);
  
  // the following BAQ is no longer in use
  /*using (var dynamicQuerySvc = Ice.Assemblies.ServiceRenderer.GetService<Ice.Contracts.DynamicQuerySvcContract>(Db))  
  {
    // Calculate the Total Price of the new Quote
    var quotePriceBaqExecutionParams = dynamicQuerySvc.GetQueryExecutionParametersByID("GHA_AQS_QuotePrice");
    
    var quotePriceQuoteNumParam = quotePriceBaqExecutionParams.ExecutionParameter.FirstOrDefault(x => x.ParameterID == "QuoteNum");
    
    if (quotePriceQuoteNumParam == null)
      return;
      
    quotePriceQuoteNumParam.ParameterValue = sourceQuoteDs.QuoteHed[0].QuoteNum.ToString();
    
    var quotePriceExecuteResult = dynamicQuerySvc.ExecuteByID("GHA_AQS_QuotePrice", quotePriceBaqExecutionParams);
    
    if (quotePriceExecuteResult == null)
      return;
      
    var totalPrice = quotePriceExecuteResult.Tables["Results"].Rows[0]["Calculated_TotalCost"];
    
    // Update QuoteDtl.DocDspExtUnitPrice via Updatable BAQ
    var baqExecutionParams = dynamicQuerySvc.GetQueryExecutionParametersByID("GHA_AQS_QuoteDtlUnitPrice");
    
    var quoteNumParam = baqExecutionParams.ExecutionParameter.FirstOrDefault(x => x.ParameterID == "QuoteNum");  
    
    if (quoteNumParam == null)  
      return;  
      
    quoteNumParam.ParameterValue = destinationQuoteDs.QuoteHed[0].QuoteNum.ToString();  
    
    var quoteLineParam = baqExecutionParams.ExecutionParameter.FirstOrDefault(x => x.ParameterID == "QuoteLine");  
    
    if (quoteLineParam == null)  
      return;  
      
    quoteLineParam.ParameterValue = destinationQuoteDs.QuoteDtl[0].QuoteLine.ToString();
    
    var executeResult = dynamicQuerySvc.ExecuteByID("GHA_AQS_QuoteDtlUnitPrice", baqExecutionParams);
    executeResult.Tables["Results"].Rows[0]["QuoteDtl_DocExpUnitPrice"] = totalPrice;
  
    var updateResult = dynamicQuerySvc.UpdateByID("GHA_AQS_QuoteDtlUnitPrice", executeResult);
  }*/
}

using (var quoteAsmSvc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.QuoteAsmSvcContract>(Db))
{
  var destinationQuoteAsmDs = quoteAsmSvc.GetByID(destinationQuoteDs.QuoteDtl[0].QuoteNum, destinationQuoteDs.QuoteDtl[0].QuoteLine, 0);
  
  if (destinationQuoteAsmDs == null)
    return;
    
  if (!destinationQuoteAsmDs.QuoteMtl.Any())
    return;

  var sourceQuoteAsmDs = quoteAsmSvc.GetByID(sourceQuoteDs.QuoteDtl[0].QuoteNum, sourceQuoteDs.QuoteDtl[0].QuoteLine, 0);
  
  if (sourceQuoteAsmDs == null)
    return;
    
  if (!sourceQuoteAsmDs.QuoteMtl.Any())
    return;
  
  foreach (var quoteMtl in destinationQuoteAsmDs.QuoteMtl)
  { 
    var line = sourceQuoteAsmDs.QuoteMtl
      .FirstOrDefault(x => x.QuoteLine == quoteMtl.QuoteLine &&
                           x.AssemblySeq == quoteMtl.AssemblySeq &&
                           x.MtlSeq == quoteMtl.MtlSeq);
     
    if (line == null)
      continue;
    
    quoteMtl.EstUnitCost = line.EstUnitCost;
    quoteMtl.RowMod = "U";
  }
  
  quoteAsmSvc.Update(ref destinationQuoteAsmDs);
}