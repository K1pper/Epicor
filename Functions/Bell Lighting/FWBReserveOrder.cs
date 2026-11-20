/* No longer required, here because I wrote and teted the code so may serve as an example */

Message = string.Empty;

try
{
  this.CallService<Erp.Contracts.OrderAllocSvcContract>(alloc =>
  {
      var sts = new Erp.Tablesets.SlimOrderAllocTableset();
      var ts = new Erp.Tablesets.OrderAllocTableset();
    
      sts.SlimOrderAlloc.Add(new Erp.Tablesets.SlimOrderAllocRow());
    
      sts.SlimOrderAlloc[0].Company = Session.CompanyID;
      sts.SlimOrderAlloc[0].JobNum = string.Empty;
      sts.SlimOrderAlloc[0].AssemblySeq = 0;
      sts.SlimOrderAlloc[0].DemandType = "Order";
      sts.SlimOrderAlloc[0].MtlSeq = 0;
      sts.SlimOrderAlloc[0].OrderLine = OrderLine;
      sts.SlimOrderAlloc[0].OrderNum = OrderNum;
      sts.SlimOrderAlloc[0].OrderRelNum = OrderRelNum;
      sts.SlimOrderAlloc[0].ReservePriorityOverride = 0;
      sts.SlimOrderAlloc[0].SelectedForAction = true;
      sts.SlimOrderAlloc[0].TFOrdLine = 0;
      sts.SlimOrderAlloc[0].TFOrdNum = "";
      sts.SlimOrderAlloc[0].RowMod = "A";
      
      alloc.AutoReserve(ref sts, "", "primaryonly", out Message);
  });
}
catch (Exception ex)
{
Message = ex.ToString();
}