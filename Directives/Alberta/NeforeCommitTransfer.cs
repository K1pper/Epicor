int pos = 0;

foreach (var ttInvTrans_iterator in (from ttInvTrans_Row in ds.InvTrans
																		where (Convert.ToBoolean(" aAuU".IndexOf(ttInvTrans_Row.RowMod) > 0))
																		select ttInvTrans_Row))
{
	var ttInvTransRow = ttInvTrans_iterator;

	var UserFileRow = (from UserFile_Row in Db.UserFile
		where (UserFile_Row.DcdUserID == Session.UserID)
		select UserFile_Row).FirstOrDefault();
	if (UserFileRow != null)
	{
//
// Check authorisation is ok for the From Bin
//

		var WhseBinRow = (from WhseBin_Row in Db.WhseBin
			where (WhseBin_Row.Company == ttInvTransRow.Company && WhseBin_Row.WarehouseCode == ttInvTransRow.FromWarehouseCode && WhseBin_Row.BinNum == ttInvTransRow.FromBinNum)
			select WhseBin_Row).FirstOrDefault();
/*
		if (WhseBinRow != null)
		{
*/
//string msg = "FromWareHouseCode=" + ttInvTransRow.FromWarehouseCode.ToString() + "WhseBinRow.MainBin_c=" + WhseBinRow["MainBin_c"].ToString() + "UserFileRow.MainBin_c=" + UserFileRow["MainBin_c"].ToString();
//PublishInfoMessage(msg, Ice.Common.BusinessObjectMessageType.Information, Ice.Bpm.InfoMessageDisplayMode.Individual, "SalesOrder", "Update");
/*
			if (ttInvTransRow.FromWarehouseCode.ToString() == "MAIN")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Main") throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is not authorised for transactions in MAIN warehouse");
				if (UserFileRow["MainBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in MAIN warehouse");
			}
			if (ttInvTransRow.FromWarehouseCode.ToString() == "PROJ")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Projects") throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is not authorised for transactions in PROJ warehouse");
				if (UserFileRow["ProjBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in PROJ warehouse");
			}
			if (ttInvTransRow.FromWarehouseCode.ToString() == "SERV")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Service") throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is not authorised for transactions in SERV warehouse");
				if (UserFileRow["SerBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in SERV warehouse");
			}
			if (ttInvTransRow.FromWarehouseCode.ToString() == "RESLOC")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Reserved") throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is not authorised for transactions in RESLOC warehouse");
				if (UserFileRow["ResBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in RESLOC warehouse");
			}
*/
/*
			if (WhseBinRow["Whse_c"].ToString() == "Main" && UserFileRow["MainBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is in Virtual Warehouse MAIN and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Projects" && UserFileRow["ProjBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is in Virtual Warehouse PROJ and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Service" && UserFileRow["SerBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is in Virtual Warehouse SERV and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Reserved" && UserFileRow["ResBin_c"].ToString() == "False") 
			{
				throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is in Virtual Warehouse RESLOC and you are not authorised for transactions in that warehouse");
			}
*/
		if (WhseBinRow != null && (string)WhseBinRow["VirtWhse_c"] != "")
		{
			string[] VirtWhses = Convert.ToString(UserFileRow["VirtWhse_c"]).Split('~');
			pos = Array.IndexOf(VirtWhses,WhseBinRow["VirtWhse_c"]);
			if (pos == -1)
			{
				throw new Ice.BLException("From Bin " + ttInvTransRow.FromBinNum + " is in a Virtual Warehouse which you are not authorised to transact in");
			}
		}
//
// Check authorisation is ok for the To Bin
//
		WhseBinRow = (from WhseBin_Row in Db.WhseBin
			where (WhseBin_Row.Company == ttInvTransRow.Company && WhseBin_Row.WarehouseCode == ttInvTransRow.ToWarehouseCode && WhseBin_Row.BinNum == ttInvTransRow.ToBinNum)
			select WhseBin_Row).FirstOrDefault();
/*
		if (WhseBinRow != null)
		{
*/
/*
			if (ttInvTransRow.ToWarehouseCode.ToString() == "MAIN")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Main") throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is not authorised for transactions in MAIN warehouse");
				if (UserFileRow["MainBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in MAIN warehouse");
			}
			if (ttInvTransRow.ToWarehouseCode.ToString() == "PROJ")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Projects") throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is not authorised for transactions in PROJ warehouse");
				if (UserFileRow["ProjBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in PROJ warehouse");
			}
			if (ttInvTransRow.ToWarehouseCode.ToString() == "SERV")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Service") throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is not authorised for transactions in SERV warehouse");
				if (UserFileRow["SerBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in SERV warehouse");
			}
			if (ttInvTransRow.ToWarehouseCode.ToString() == "RESLOC")
			{
				if (WhseBinRow["Whse_c"].ToString() != "Reserved") throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is not authorised for transactions in RESLOC warehouse");
				if (UserFileRow["ResBin_c"].ToString() == "False") throw new Ice.BLException("You are not authorised for transactions in RESLOC warehouse");
			}
*/
/*
			if (WhseBinRow["Whse_c"].ToString() == "Main" && UserFileRow["MainBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is in Virtual Warehouse MAIN and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Projects" && UserFileRow["ProjBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is in Virtual Warehouse PROJ and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Service" && UserFileRow["SerBin_c"].ToString() == "False")
			{
				throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is in Virtual Warehouse SERV and you are not authorised for transactions in that Warehouse");
			}
			if (WhseBinRow["Whse_c"].ToString() == "Reserved" && UserFileRow["ResBin_c"].ToString() == "False") 
			{
				throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is in Virtual Warehouse RESLOC and you are not authorised for transactions in that warehouse");
			}
		}
*/
		if (WhseBinRow != null && (string)WhseBinRow["VirtWhse_c"] != "")
		{
			string[] VirtWhses = Convert.ToString(UserFileRow["VirtWhse_c"]).Split('~');
			pos = Array.IndexOf(VirtWhses,WhseBinRow["VirtWhse_c"]);
			if (pos == -1)
			{
				throw new Ice.BLException("To Bin " + ttInvTransRow.ToBinNum + " is in a Virtual Warehouse which you are not authorised to transact in");
			}
		}
	}
}