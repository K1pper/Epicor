var ttQuoteDtl_xRow = (from ttQuoteDtl_Row in ds.QuoteDtl
										where ttQuoteDtl_Row.RowMod == "A" || ttQuoteDtl_Row.RowMod == "U"
										select ttQuoteDtl_Row).FirstOrDefault();
if (ttQuoteDtl_xRow != null)
{
		var Part_xRow = (from Part_Row in Db.Part
										where Part_Row.Company == Session.CompanyID
										&&	Part_Row.PartNum == ttQuoteDtl_xRow.PartNum
										select Part_Row).FirstOrDefault();
		if (Part_xRow != null)
		{
				ttQuoteDtl_xRow["LbrPerc_c"] = Part_xRow["LabourPerc_c"];
		}
}