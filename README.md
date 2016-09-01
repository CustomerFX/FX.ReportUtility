# FX.ReportUtility
A utility assembly for generating reports and attaching to records

# Usage
The utility assembly can be used to export a report as a PDF or attach a PDF to a record as an attachment.

```C#
// export a report to a PDF and save as an attachment
var report = new FX.ReportUtility.CRMReport("System:My Report");
var attachment = report.CreateAttachment("{ACCOUNT.ACCOUNTID} = '" + someAccountId + "'");
attachment.AccountId = someAccountId;
attachment.Save();
```

**Note:** for attaching to a record, it is the responsibility of the code using this assembly to set the references for the attachment record and then save it.

```C#
// export a report as a PDF 
var report = new FX.ReportUtility.CRMReport("System:My Report");
var exportedFile = report.SaveAsPDF("{ACCOUNT.ACCOUNTID} = '" + someAccountId + "'");
```

**RecordSelectionFormula** parameter must be a valid Crystal record selection formula. Examples:

**Example #1:**
"{OPPORTUNITY.OPPORTUNITYID} = '" + someOpportunityId + "'"
 
**Example #2:**
"{OPPORTUNITYQUOTE.OPPORTUNITYQUOTEID} = '" + someOpportunityQuoteId + "' AND {OPPORTUNITY_CONTACT.ISPRIMARY} = 'T' AND NOT ({PRODUCT.NAME} = 'Custom Door Panel' OR {PRODUCT.NAME} = 'Custom Drawer Panel')"
