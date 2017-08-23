namespace Policy_Tracker.Reports
{
    partial class PredictivePremiumReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
            this.MonthLbl = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrPivotGrid1 = new DevExpress.XtraReports.UI.XRPivotGrid();
            this.objectDataSource1 = new DevExpress.DataAccess.ObjectBinding.ObjectDataSource();
            this.fieldBranch = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldProductLine = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldUW = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldNBQuotedPremium = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldTotalNBQuoted = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldNBWrittenPremium = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldTotalWritten = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldPremiumAvailableForRenewal = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldTotalAvailable = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldRenewedPremium = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldTotalRenewed = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.fieldPredictedPremium = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPageInfo1 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.PageFooter = new DevExpress.XtraReports.UI.PageFooterBand();
            this.xrPivotGrid2 = new DevExpress.XtraReports.UI.XRPivotGrid();
            this.xrPivotGridField2 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField4 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField5 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField6 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField7 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField8 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField9 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField10 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField11 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            this.xrPivotGridField12 = new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField();
            ((System.ComponentModel.ISupportInitialize)(this.objectDataSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // Detail
            // 
            this.Detail.HeightF = 100F;
            this.Detail.Name = "Detail";
            this.Detail.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 0F;
            this.TopMargin.Name = "TopMargin";
            this.TopMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.TopMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 1.041667F;
            this.BottomMargin.Name = "BottomMargin";
            this.BottomMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.BottomMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPivotGrid2,
            this.xrLabel2,
            this.MonthLbl,
            this.xrLabel1,
            this.xrPivotGrid1});
            this.ReportHeader.HeightF = 255.2083F;
            this.ReportHeader.Name = "ReportHeader";
            // 
            // xrLabel2
            // 
            this.xrLabel2.Borders = DevExpress.XtraPrinting.BorderSide.Top;
            this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(411.4584F, 41.75F);
            this.xrLabel2.Name = "xrLabel2";
            this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel2.SizeF = new System.Drawing.SizeF(420.8333F, 23F);
            this.xrLabel2.StylePriority.UseBorders = false;
            this.xrLabel2.Text = "Report assumes a 20% hit Ratio when calculating the predicted premium";
            // 
            // MonthLbl
            // 
            this.MonthLbl.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold);
            this.MonthLbl.LocationFloat = new DevExpress.Utils.PointFloat(19.79167F, 54.12501F);
            this.MonthLbl.Name = "MonthLbl";
            this.MonthLbl.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.MonthLbl.SizeF = new System.Drawing.SizeF(147.9166F, 23F);
            this.MonthLbl.StylePriority.UseFont = false;
            this.MonthLbl.StylePriority.UseTextAlignment = false;
            this.MonthLbl.Text = "Month:";
            this.MonthLbl.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            // 
            // xrLabel1
            // 
            this.xrLabel1.Font = new System.Drawing.Font("Times New Roman", 20F);
            this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(411.4584F, 0F);
            this.xrLabel1.Name = "xrLabel1";
            this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel1.SizeF = new System.Drawing.SizeF(319.7917F, 41.75F);
            this.xrLabel1.StylePriority.UseFont = false;
            this.xrLabel1.Text = "Predictive Premium Report";
            // 
            // xrPivotGrid1
            // 
            this.xrPivotGrid1.DataSource = this.objectDataSource1;
            this.xrPivotGrid1.Fields.AddRange(new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField[] {
            this.fieldBranch,
            this.fieldProductLine,
            this.fieldUW,
            this.fieldNBQuotedPremium,
            this.fieldTotalNBQuoted,
            this.fieldNBWrittenPremium,
            this.fieldTotalWritten,
            this.fieldPremiumAvailableForRenewal,
            this.fieldTotalAvailable,
            this.fieldRenewedPremium,
            this.fieldTotalRenewed,
            this.fieldPredictedPremium});
            this.xrPivotGrid1.LocationFloat = new DevExpress.Utils.PointFloat(19.79167F, 119.7917F);
            this.xrPivotGrid1.Name = "xrPivotGrid1";
            this.xrPivotGrid1.OptionsPrint.FilterSeparatorBarPadding = 3;
            this.xrPivotGrid1.OptionsView.ShowColumnGrandTotalHeader = false;
            this.xrPivotGrid1.OptionsView.ShowColumnGrandTotals = false;
            this.xrPivotGrid1.OptionsView.ShowColumnHeaders = false;
            this.xrPivotGrid1.OptionsView.ShowColumnTotals = false;
            this.xrPivotGrid1.OptionsView.ShowDataHeaders = false;
            this.xrPivotGrid1.SizeF = new System.Drawing.SizeF(1069.708F, 49.99998F);
            // 
            // objectDataSource1
            // 
            this.objectDataSource1.DataSource = typeof(PolicyTracker.DomainModel.Reports.PredictivePremiumDataPoint);
            this.objectDataSource1.Name = "objectDataSource1";
            // 
            // fieldBranch
            // 
            this.fieldBranch.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.fieldBranch.AreaIndex = 0;
            this.fieldBranch.FieldName = "Branch";
            this.fieldBranch.Name = "fieldBranch";
            this.fieldBranch.Width = 50;
            // 
            // fieldProductLine
            // 
            this.fieldProductLine.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.fieldProductLine.AreaIndex = 1;
            this.fieldProductLine.FieldName = "ProductLine";
            this.fieldProductLine.Name = "fieldProductLine";
            // 
            // fieldUW
            // 
            this.fieldUW.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.fieldUW.AreaIndex = 2;
            this.fieldUW.FieldName = "UW";
            this.fieldUW.Name = "fieldUW";
            // 
            // fieldNBQuotedPremium
            // 
            this.fieldNBQuotedPremium.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldNBQuotedPremium.AreaIndex = 0;
            this.fieldNBQuotedPremium.Caption = "NB $ Quoted";
            this.fieldNBQuotedPremium.CellFormat.FormatString = "C0";
            this.fieldNBQuotedPremium.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fieldNBQuotedPremium.FieldName = "NBQuotedPremium";
            this.fieldNBQuotedPremium.Name = "fieldNBQuotedPremium";
            // 
            // fieldTotalNBQuoted
            // 
            this.fieldTotalNBQuoted.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldTotalNBQuoted.AreaIndex = 1;
            this.fieldTotalNBQuoted.Caption = "NB # Quo";
            this.fieldTotalNBQuoted.FieldName = "TotalNBQuoted";
            this.fieldTotalNBQuoted.Name = "fieldTotalNBQuoted";
            this.fieldTotalNBQuoted.Width = 60;
            // 
            // fieldNBWrittenPremium
            // 
            this.fieldNBWrittenPremium.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldNBWrittenPremium.AreaIndex = 2;
            this.fieldNBWrittenPremium.Caption = "NB $ Issued";
            this.fieldNBWrittenPremium.CellFormat.FormatString = "C0";
            this.fieldNBWrittenPremium.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fieldNBWrittenPremium.FieldName = "NBWrittenPremium";
            this.fieldNBWrittenPremium.Name = "fieldNBWrittenPremium";
            // 
            // fieldTotalWritten
            // 
            this.fieldTotalWritten.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldTotalWritten.AreaIndex = 3;
            this.fieldTotalWritten.Caption = "# Issued";
            this.fieldTotalWritten.FieldName = "TotalWritten";
            this.fieldTotalWritten.Name = "fieldTotalWritten";
            this.fieldTotalWritten.Width = 60;
            // 
            // fieldPremiumAvailableForRenewal
            // 
            this.fieldPremiumAvailableForRenewal.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldPremiumAvailableForRenewal.AreaIndex = 4;
            this.fieldPremiumAvailableForRenewal.Caption = "$ Avail. For Renewal";
            this.fieldPremiumAvailableForRenewal.CellFormat.FormatString = "C0";
            this.fieldPremiumAvailableForRenewal.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fieldPremiumAvailableForRenewal.FieldName = "PremiumAvailableForRenewal";
            this.fieldPremiumAvailableForRenewal.Name = "fieldPremiumAvailableForRenewal";
            // 
            // fieldTotalAvailable
            // 
            this.fieldTotalAvailable.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldTotalAvailable.AreaIndex = 5;
            this.fieldTotalAvailable.Caption = "# Avail.";
            this.fieldTotalAvailable.FieldName = "TotalAvailable";
            this.fieldTotalAvailable.Name = "fieldTotalAvailable";
            this.fieldTotalAvailable.Width = 80;
            // 
            // fieldRenewedPremium
            // 
            this.fieldRenewedPremium.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldRenewedPremium.AreaIndex = 6;
            this.fieldRenewedPremium.Caption = "$ Renewed";
            this.fieldRenewedPremium.CellFormat.FormatString = "C0";
            this.fieldRenewedPremium.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fieldRenewedPremium.FieldName = "RenewedPremium";
            this.fieldRenewedPremium.Name = "fieldRenewedPremium";
            // 
            // fieldTotalRenewed
            // 
            this.fieldTotalRenewed.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldTotalRenewed.AreaIndex = 7;
            this.fieldTotalRenewed.Caption = "# Renewed";
            this.fieldTotalRenewed.FieldName = "TotalRenewed";
            this.fieldTotalRenewed.Name = "fieldTotalRenewed";
            this.fieldTotalRenewed.Width = 70;
            // 
            // fieldPredictedPremium
            // 
            this.fieldPredictedPremium.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldPredictedPremium.AreaIndex = 8;
            this.fieldPredictedPremium.Caption = "Predicted Prem.";
            this.fieldPredictedPremium.CellFormat.FormatString = "C0";
            this.fieldPredictedPremium.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fieldPredictedPremium.FieldName = "PredictedPremium";
            this.fieldPredictedPremium.Name = "fieldPredictedPremium";
            // 
            // xrPageInfo1
            // 
            this.xrPageInfo1.LocationFloat = new DevExpress.Utils.PointFloat(989.4999F, 76.99998F);
            this.xrPageInfo1.Name = "xrPageInfo1";
            this.xrPageInfo1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrPageInfo1.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrPageInfo1.StylePriority.UseTextAlignment = false;
            this.xrPageInfo1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            // 
            // PageFooter
            // 
            this.PageFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPageInfo1});
            this.PageFooter.HeightF = 100F;
            this.PageFooter.Name = "PageFooter";
            // 
            // xrPivotGrid2
            // 
            this.xrPivotGrid2.DataSource = this.objectDataSource1;
            this.xrPivotGrid2.Fields.AddRange(new DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField[] {
            this.xrPivotGridField2,
            this.xrPivotGridField4,
            this.xrPivotGridField5,
            this.xrPivotGridField6,
            this.xrPivotGridField7,
            this.xrPivotGridField8,
            this.xrPivotGridField9,
            this.xrPivotGridField10,
            this.xrPivotGridField11,
            this.xrPivotGridField12});
            this.xrPivotGrid2.LocationFloat = new DevExpress.Utils.PointFloat(19.79167F, 184.375F);
            this.xrPivotGrid2.Name = "xrPivotGrid2";
            this.xrPivotGrid2.OptionsPrint.FilterSeparatorBarPadding = 3;
            this.xrPivotGrid2.OptionsView.ShowColumnGrandTotalHeader = false;
            this.xrPivotGrid2.OptionsView.ShowColumnGrandTotals = false;
            this.xrPivotGrid2.OptionsView.ShowColumnHeaders = false;
            this.xrPivotGrid2.OptionsView.ShowColumnTotals = false;
            this.xrPivotGrid2.OptionsView.ShowDataHeaders = false;
            this.xrPivotGrid2.OptionsView.ShowFilterHeaders = false;
            this.xrPivotGrid2.OptionsView.ShowFilterSeparatorBar = false;
            this.xrPivotGrid2.SizeF = new System.Drawing.SizeF(1069.708F, 49.99998F);
            // 
            // xrPivotGridField2
            // 
            this.xrPivotGridField2.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.xrPivotGridField2.AreaIndex = 0;
            this.xrPivotGridField2.FieldName = "ProductLine";
            this.xrPivotGridField2.Name = "xrPivotGridField2";
            this.xrPivotGridField2.Width = 250;
            // 
            // xrPivotGridField4
            // 
            this.xrPivotGridField4.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField4.AreaIndex = 0;
            this.xrPivotGridField4.Caption = "NB $ Quoted";
            this.xrPivotGridField4.CellFormat.FormatString = "C0";
            this.xrPivotGridField4.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.xrPivotGridField4.FieldName = "NBQuotedPremium";
            this.xrPivotGridField4.Name = "xrPivotGridField4";
            // 
            // xrPivotGridField5
            // 
            this.xrPivotGridField5.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField5.AreaIndex = 1;
            this.xrPivotGridField5.Caption = "NB # Quo";
            this.xrPivotGridField5.FieldName = "TotalNBQuoted";
            this.xrPivotGridField5.Name = "xrPivotGridField5";
            this.xrPivotGridField5.Width = 60;
            // 
            // xrPivotGridField6
            // 
            this.xrPivotGridField6.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField6.AreaIndex = 2;
            this.xrPivotGridField6.Caption = "NB $ Issued";
            this.xrPivotGridField6.CellFormat.FormatString = "C0";
            this.xrPivotGridField6.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.xrPivotGridField6.FieldName = "NBWrittenPremium";
            this.xrPivotGridField6.Name = "xrPivotGridField6";
            // 
            // xrPivotGridField7
            // 
            this.xrPivotGridField7.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField7.AreaIndex = 3;
            this.xrPivotGridField7.Caption = "# Issued";
            this.xrPivotGridField7.FieldName = "TotalWritten";
            this.xrPivotGridField7.Name = "xrPivotGridField7";
            this.xrPivotGridField7.Width = 60;
            // 
            // xrPivotGridField8
            // 
            this.xrPivotGridField8.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField8.AreaIndex = 4;
            this.xrPivotGridField8.Caption = "$ Avail. For Renewal";
            this.xrPivotGridField8.CellFormat.FormatString = "C0";
            this.xrPivotGridField8.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.xrPivotGridField8.FieldName = "PremiumAvailableForRenewal";
            this.xrPivotGridField8.Name = "xrPivotGridField8";
            // 
            // xrPivotGridField9
            // 
            this.xrPivotGridField9.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField9.AreaIndex = 5;
            this.xrPivotGridField9.Caption = "# Avail.";
            this.xrPivotGridField9.FieldName = "TotalAvailable";
            this.xrPivotGridField9.Name = "xrPivotGridField9";
            this.xrPivotGridField9.Width = 80;
            // 
            // xrPivotGridField10
            // 
            this.xrPivotGridField10.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField10.AreaIndex = 6;
            this.xrPivotGridField10.Caption = "$ Renewed";
            this.xrPivotGridField10.CellFormat.FormatString = "C0";
            this.xrPivotGridField10.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.xrPivotGridField10.FieldName = "RenewedPremium";
            this.xrPivotGridField10.Name = "xrPivotGridField10";
            // 
            // xrPivotGridField11
            // 
            this.xrPivotGridField11.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField11.AreaIndex = 7;
            this.xrPivotGridField11.Caption = "# Renewed";
            this.xrPivotGridField11.FieldName = "TotalRenewed";
            this.xrPivotGridField11.Name = "xrPivotGridField11";
            this.xrPivotGridField11.Width = 70;
            // 
            // xrPivotGridField12
            // 
            this.xrPivotGridField12.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.xrPivotGridField12.AreaIndex = 8;
            this.xrPivotGridField12.Caption = "Predicted Prem.";
            this.xrPivotGridField12.CellFormat.FormatString = "C0";
            this.xrPivotGridField12.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.xrPivotGridField12.FieldName = "PredictedPremium";
            this.xrPivotGridField12.Name = "xrPivotGridField12";
            // 
            // PredictivePremiumReport
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.Detail,
            this.TopMargin,
            this.BottomMargin,
            this.ReportHeader,
            this.PageFooter});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.objectDataSource1});
            this.DataSource = this.objectDataSource1;
            this.Landscape = true;
            this.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 1);
            this.PageHeight = 850;
            this.PageWidth = 1100;
            this.PaperKind = System.Drawing.Printing.PaperKind.Custom;
            this.Version = "14.2";
            ((System.ComponentModel.ISupportInitialize)(this.objectDataSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRPivotGrid xrPivotGrid1;
        private DevExpress.DataAccess.ObjectBinding.ObjectDataSource objectDataSource1;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldBranch;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldProductLine;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldUW;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldNBQuotedPremium;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldTotalNBQuoted;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldNBWrittenPremium;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldTotalWritten;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldPremiumAvailableForRenewal;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldTotalAvailable;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldRenewedPremium;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldTotalRenewed;
        private DevExpress.XtraReports.UI.XRLabel MonthLbl;
        private DevExpress.XtraReports.UI.XRLabel xrLabel1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField fieldPredictedPremium;
        private DevExpress.XtraReports.UI.XRPageInfo xrPageInfo1;
        private DevExpress.XtraReports.UI.PageFooterBand PageFooter;
        private DevExpress.XtraReports.UI.XRPivotGrid xrPivotGrid2;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField2;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField4;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField5;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField6;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField7;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField8;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField9;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField10;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField11;
        private DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField xrPivotGridField12;
    }
}
