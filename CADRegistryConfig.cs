using System;
using System.IO;
using Microsoft.Win32;

namespace AVRoadLauncher
{
    public class CadAppDef
    {
        public string Name;
        public string Loader;
        public int LoadCtrls;
        public string Description;
        public int Managed;
        public string[] Commands;
        public string[] Groups;
    }

    public class DockToolDef
    {
        public string Guid;
        public string Name;
        public string Command;
        public string CommandClose;
    }

    public static class CADRegistryConfig
    {
        public static readonly DockToolDef[] DockingTools = new DockToolDef[]
        {
            new DockToolDef { Guid = "{0939E07C-FCEF-4C0D-8349-662031189B8A}", Name = "Lights in Model", Command = "LIGHTLIST", CommandClose = "LIGHTLISTCLOSE" },
            new DockToolDef { Guid = "{12F4DCC5-8FDB-4937-8387-E628D5C2ABBF}", Name = "Dashboard", Command = "DASHBOARD", CommandClose = "DASHBOARDCLOSE" },
            new DockToolDef { Guid = "{1A11B0B9-1AA0-4E71-95D1-C77737444340}", Name = "External References", Command = "EXTERNALREFERENCES", CommandClose = "EXTERNALREFERENCESCLOSE" },
            new DockToolDef { Guid = "{26C7CFE0-7187-4736-9B70-EF21C803A601}", Name = "Sun Properties", Command = "SUNPROPERTIES", CommandClose = "SUNPROPERTIESCLOSE" },
            new DockToolDef { Guid = "{3DE1200A-089D-4CDF-9B39-EC44F585FE73}", Name = "Materials", Command = "MATERIALS", CommandClose = "MATERIALSCLOSE" },
            new DockToolDef { Guid = "{60284BE8-5CB0-474B-BD85-9D3335F5E471}", Name = "Visual Styles Manager", Command = "VISUALSTYLES", CommandClose = "VISUALSTYLESCLOSE" },
            new DockToolDef { Guid = "{6D32A2D9-832E-11D2-A83C-0060B0872C0B}", Name = "Properties", Command = "PROPERTIES", CommandClose = "PROPERTIESCLOSE" },
            new DockToolDef { Guid = "{888F0E83-D9B4-48A5-AEA9-3867A6121478}", Name = "Advanced Settings Dialog", Command = "RPREF", CommandClose = "RPREFCLOSE" },
            new DockToolDef { Guid = "{9403002C-2B99-4281-B9DD-F708E0858542}", Name = "Markup Set Manager", Command = "MARKUP", CommandClose = "MARKUPCLOSE" },
            new DockToolDef { Guid = "{941D0ACA-4F95-45E6-AA36-EEA7773D4924}", Name = "Info Palette", Command = "ASSIST", CommandClose = "ASSISTCLOSE" },
            new DockToolDef { Guid = "{ACA19CF8-C061-4301-8E75-49DACFF072AD}", Name = "Quick Calc", Command = "QuickCalc", CommandClose = "QCCLOSE" },
            new DockToolDef { Guid = "{B17B40A6-2761-4BCC-BC48-2B8A465AA5EB}", Name = "Tool Palette", Command = "TOOLPALETTES", CommandClose = "TOOLPALETTESCLOSE" },
            new DockToolDef { Guid = "{B371F8AA-7FF7-11D2-BA58-0060B0B5E151}", Name = "Design Center", Command = "ADCENTER", CommandClose = "ADCCLOSE" },
            new DockToolDef { Guid = "{B6BCB9F4-84B2-11D2-A127-0060B0CE2DA0}", Name = "dbConnect Manager", Command = "DBCONNECT", CommandClose = "DBCCLOSE" },
            new DockToolDef { Guid = "{BEBBB318-2898-4C03-ADA2-360DC6B7AA69}", Name = "SheetSet Manager", Command = "SHEETSET", CommandClose = "SHEETSETHIDE" },
            new DockToolDef { Guid = "{D5CF410D-3E11-4D0C-AE69-A75C9B788C47}", Name = "Command Line", Command = "COMMANDLINE", CommandClose = "COMMANDLINEHIDE" },
        };

        public static readonly CadAppDef[] Applications = new CadAppDef[]
        {
            new CadAppDef { Name = "AcadAcLaunchNFW", Loader = "AcLaunchNFW.arx", LoadCtrls = 10, Description = "Launch NFW", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadApp", Loader = "AcApp.arx", LoadCtrls = 2, Description = "AcApp Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadAppload", Loader = "Appload.arx", LoadCtrls = 13, Description = "AcadAppload Command Module", Managed = 0, Commands = new string[] { "APPLOAD" }, Groups = new string[] { "ACAD_APPLOAD" } },
            new CadAppDef { Name = "AcadArray", Loader = "AcArray.arx", LoadCtrls = 13, Description = "AcadArray Command Module", Managed = 0, Commands = new string[] { "ARRAY" }, Groups = new string[] { "ACAD_ARRAY" } },
            new CadAppDef { Name = "AcadAssist", Loader = "AcAssist.arx", LoadCtrls = 12, Description = "Acad Dynamic Assistance Command Module", Managed = 0, Commands = new string[] { "ASSIST", "ASSISTCLOSE" }, Groups = new string[] { "AcadAssist" } },
            new CadAppDef { Name = "AcadAttEdit", Loader = "Attedit.arx", LoadCtrls = 13, Description = "AcadAttEdit Command Module", Managed = 0, Commands = new string[] { "ATTEDIT", "DDATTE" }, Groups = new string[] { "ATTRIBUTEEDIT" } },
            new CadAppDef { Name = "AcadAuthEnviron", Loader = "AcAuthEnviron.arx", LoadCtrls = 12, Description = "AcadAuthEnviron Command Module", Managed = 0, Commands = new string[] { "BEDIT", "-BEDIT", "BCLOSE", "BSAVE", "BSAVEAS", "BPARAMETER", "BACTION", "BACTIONTOOL", "BATTORDER", "BASSOCIATE", "BACTIONSET", "BGRIPSET", "BVSTATE", "-BVSTATE", "BVHIDE", "-BVHIDE", "BVSHOW", "-BVSHOW", "BCYCLEORDER" }, Groups = new string[] { "ACAD_ACAE" } },
            new CadAppDef { Name = "AcadBattman", Loader = "Battman.arx", LoadCtrls = 12, Description = "AcadBattman Command Module", Managed = 0, Commands = new string[] { "ATTSYNC", "BATTMAN" }, Groups = new string[] { "AcBattman" } },
            new CadAppDef { Name = "AcadBGPlot", Loader = "AcBGPlot.arx", LoadCtrls = 12, Description = "AcadBGPlot Command Module", Managed = 0, Commands = new string[] { "BGPLOTRECEIVE" }, Groups = new string[] { "ACAD_BGPLOT" } },
            new CadAppDef { Name = "AcadBlock", Loader = "AcBlock.arx", LoadCtrls = 13, Description = "AcadBlock Command Module", Managed = 0, Commands = new string[] { "ACADBLOCKDIALOG", "ACADWBLOCKDIALOG", "BLOCKICON", "BMAKE", "BMOD", "DDINSERT", "RESETBLOCK" }, Groups = new string[] { "ACAD_BLOCK" } },
            new CadAppDef { Name = "AcadBrowser", Loader = "AcBrowse.arx", LoadCtrls = 13, Description = "AcadBrowser Command Module", Managed = 0, Commands = new string[] { "BROWSER", "BROWSER2" }, Groups = new string[] { "ACAD_BROWSER" } },
            new CadAppDef { Name = "AcadDashboard", Loader = "AcDashboard.arx", LoadCtrls = 12, Description = "Dashboard Command Module", Managed = 0, Commands = new string[] { "DASHBOARD", "DASHBOARDCLOSE" }, Groups = new string[] { "AcadAcDashboard" } },
            new CadAppDef { Name = "AcadDBConnect", Loader = "ConDlg.arx", LoadCtrls = 13, Description = "AcadDbConnect", Managed = 0, Commands = new string[] { "DBCCONFIGURE", "DBCCONNECT", "DBCDEFINELLT", "DBCDEFINELT", "DBCDELETELABELS", "DBCDELETELINKS", "DBCDELETELLT", "DBCDELETELT", "DBCDELETEQRY", "DBCDISCONNECT", "DBCEDITLINKEDTABLE", "DBCEDITLLT", "DBCEDITLT", "DBCEDITQRY", "DBCEDITTABLE", "DBCEXECUTEQRY", "DBCEXPORTLINKS", "DBCEXPORTQS", "DBCEXPORTTS", "DBCHIDELABELS", "DBCIMPORTQRY", "DBCIMPORTQS", "DBCIMPORTTS", "DBCLINKCONVERSION", "DBCLINKMANAGER", "DBCNEWQRYLT", "DBCNEWQRYTABLE", "DBCONNECT", "DBCPROPSLBLT", "DBCPROPSLT", "DBCRELOADLABELS", "DBCSELECTLINKS", "DBCSHOWLABELS", "DBCSYNC", "DBCVIEWLINKEDTABLE", "DBCVIEWTABLE" }, Groups = new string[] { "CONDLG_COMMANDS" } },
            new CadAppDef { Name = "AcadDblClkEdit", Loader = "AcDblClkEdit.arx", LoadCtrls = 15, Description = "AcadDblClkEdit Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadDC", Loader = "acdcframe.arx", LoadCtrls = 13, Description = "AutoCAD Design Center", Managed = 0, Commands = new string[] { "adcenter", "adcclose", "adc", "content", "adcnavigate", "adccustomnavigate" }, Groups = new string[] { "ACAD_DC" } },
            new CadAppDef { Name = "AcadDim", Loader = "acdim.arx", LoadCtrls = 15, Description = "AcadDim Command Module", Managed = 0, Commands = new string[] { "AIDIMPREC", "AIDIMSTYLE", "AIDIMTEXTMOVE", "DDIM", "DIM", "DIM1", "DIMALIGNED", "DIMANGULAR", "DIMARC", "DIMBASELINE", "DIMCENTER", "DIMCONTINUE", "DIMDIAMETER", "DIMDISASSOCIATE", "DIMEDIT", "DIMHORIZONTAL", "DIMLINEAR", "DIMORDINATE", "DIMOVERRIDE", "DIMRADIUS", "DIMREASSOCIATE", "DIMREGEN", "DIMROTATED", "DIMSTYLE", "-DIMSTYLE", "DIMTEDIT", "DIMVERTICAL", "LEADER", "QDIM", "QLEADER", "TOLERANCE", "DIMJOGGED", "AIDIMFLIPARROW" }, Groups = new string[] { "ACAD_DIM" } },
            new CadAppDef { Name = "AcadDOrder", Loader = "AcDorder.arx", LoadCtrls = 13, Description = "AcadDOrder Command Module", Managed = 0, Commands = new string[] { "DRAWORDER", "TEXTTOFRONT" }, Groups = new string[] { "ACAD_DRAWORDER" } },
            new CadAppDef { Name = "AcadDWGAids", Loader = "dwgaids.arx", LoadCtrls = 45, Description = "AcadDwgaids Command Module", Managed = 0, Commands = new string[] { "+DSETTINGS", "DSETTINGS" }, Groups = new string[] { "ACAD_DWGAIDS" } },
            new CadAppDef { Name = "AcadDynInput", Loader = "AcDynInput.arx", LoadCtrls = 8, Description = "Dynamic Input Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadEPlotExt", Loader = "aceplotx.arx", LoadCtrls = 15, Description = "AcadEPlotExt Command Module", Managed = 0, Commands = new string[] { "EPLOTEXT" }, Groups = new string[] { "ACAD_EPLOTEXT" } },
            new CadAppDef { Name = "AcadETransmitUi", Loader = "acETransmitui.arx", LoadCtrls = 45, Description = "AcadETransmitUi Command Module", Managed = 0, Commands = new string[] { "ETRANSMIT", "-ETRANSMIT", "ARCHIVE", "-ARCHIVE" }, Groups = new string[] { "ACAD_ETRANSMIT" } },
            new CadAppDef { Name = "AcadEvaluator", Loader = "AcFdEval.arx", LoadCtrls = 12, Description = "Field Evaluator Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadField", Loader = "AcField.arx", LoadCtrls = 12, Description = "Field Command Module", Managed = 0, Commands = new string[] { "-UPDATEFIELD", "UPDATEFIELD", "-FIELD", "FIELD" }, Groups = new string[] { "ACAD_FIELD" } },
            new CadAppDef { Name = "AcadFind", Loader = "TextFind.arx", LoadCtrls = 13, Description = "AcadFind Command Module", Managed = 0, Commands = new string[] { "FIND" }, Groups = new string[] { "ACAD_FIND" } },
            new CadAppDef { Name = "AcadGeom", Loader = "geom3d.arx", LoadCtrls = 13, Description = "AcadGeom Command Module", Managed = 0, Commands = new string[] { "3DALIGN", "MIRROR3D", "ROTATE3D", "HELIX", "ALIGN" }, Groups = new string[] { "ACAD_GEOM" } },
            new CadAppDef { Name = "AcadGsConfig", Loader = "acgsconfig.arx", LoadCtrls = 44, Description = "AcadGsConfig Command Module", Managed = 0, Commands = new string[] { "3DCONFIG", "-3DCONFIG" }, Groups = new string[] { "ACAD_GSCONFIG" } },
            new CadAppDef { Name = "AcadHyperLink", Loader = "achlnkui.arx", LoadCtrls = 14, Description = "AcadHyperLink Command Module", Managed = 0, Commands = new string[] { "ATTACHURL", "DETACHURL", "HYPERLINK", "-HYPERLINK", "HYPERLINKBACK", "HYPERLINKFWD", "HYPERLINKOPEN", "HYPERLINKOPTIONS", "HYPERLINKSTOP", "PASTEASHYPERLINK", "SELECTURL" }, Groups = new string[] { "ACAD_HYPERLINK" } },
            new CadAppDef { Name = "AcadIDropMgr", Loader = "AcIDropMgr.arx", LoadCtrls = 15, Description = "AcadIDropMgr", Managed = 0, Commands = new string[] { "Setidrophandler" }, Groups = new string[] { "AcadIDropMgr" } },
            new CadAppDef { Name = "AcadLayerP", Loader = "AcLayerP.arx", LoadCtrls = 10, Description = "AcadLayerP Command Module", Managed = 0, Commands = new string[] { "LAYERP", "LAYERPMODE" }, Groups = new string[] { "ACAD_LayerP" } },
            new CadAppDef { Name = "AcadLaytrans", Loader = "Laytrans.arx", LoadCtrls = 12, Description = "AcadLaytrans Command Module", Managed = 0, Commands = new string[] { "LAYTRANS" }, Groups = new string[] { "ACAD_Laytrans" } },
            new CadAppDef { Name = "AcadMatch", Loader = "AcMatch.arx", LoadCtrls = 45, Description = "AcadMatch Command Module", Managed = 0, Commands = new string[] { "MATCHPROP", "PAINTER", "PAINTPROP" }, Groups = new string[] { "ACAD_MATCHPROP" } },
            new CadAppDef { Name = "AcadMText", Loader = "acmted.arx", LoadCtrls = 13, Description = "AcadMText Command Module", Managed = 0, Commands = new string[] { "MTEDIT", "MTEXT", "-MTEXT", "MTPROP" }, Groups = new string[] { "ACAD_MTEXT" } },
            new CadAppDef { Name = "AcadOLEScale", Loader = "AcOscale.arx", LoadCtrls = 13, Description = "AcadOLEScale Command Module", Managed = 0, Commands = new string[] { "OLESCALE", "OLEOPEN", "OLERESET", "OLECONVERT" }, Groups = new string[] { "ACAD_OLESCALE" } },
            new CadAppDef { Name = "AcadOPM", Loader = "Acopm.arx", LoadCtrls = 13, Description = "AcadOPM Command Module", Managed = 0, Commands = new string[] { "PPGETACADINPUT", "PROPERTIES", "PROPERTIESCLOSE", "QSELECT" }, Groups = new string[] { "ACAD_OPM" } },
            new CadAppDef { Name = "AcadPlDetails", Loader = "AcPlDetails.arx", LoadCtrls = 12, Description = "AcadPlDetails Command Module", Managed = 0, Commands = new string[] { "VIEWPLOTDETAILS" }, Groups = new string[] { "ACAD_PLDETAILS" } },
            new CadAppDef { Name = "AcadPlotStamp", Loader = "AcPltStamp.arx", LoadCtrls = 45, Description = "AcadPlotStamp Command Module", Managed = 0, Commands = new string[] { "DDPLOTSTAMP", "PLOTSTAMP", "-PLOTSTAMP" }, Groups = new string[] { "ACAD_PLOTSTAMP" } },
            new CadAppDef { Name = "AcadPublish", Loader = "AcPublish.arx", LoadCtrls = 12, Description = "AcadPublish Command Module", Managed = 0, Commands = new string[] { "PUBLISH", "+PUBLISH", "-PUBLISH", "3DDWFPUBLISH", "3DDWF" }, Groups = new string[] { "ACAD_PUBLISH" } },
            new CadAppDef { Name = "AcadPublishToWeb", Loader = "AcWebPublish.arx", LoadCtrls = 12, Description = "AcadPublishToWeb Command Module", Managed = 0, Commands = new string[] { "PUBLISHTOWEB" }, Groups = new string[] { "ACAD_PUBLISHTOWEB" } },
            new CadAppDef { Name = "AcadPurge", Loader = "AcPurge.arx", LoadCtrls = 13, Description = "AcadPurge Command Module", Managed = 0, Commands = new string[] { "PURGE" }, Groups = new string[] { "ACAD_PURGE" } },
            new CadAppDef { Name = "AcadQCal", Loader = "AcCalcEngine.arx", LoadCtrls = 45, Description = "AcadGeomCal Command Module (QCALC)", Managed = 0, Commands = new string[] { "cal" }, Groups = new string[] { "ACAD_GEOMCAL" } },
            new CadAppDef { Name = "AcadRefEdit", Loader = "AcRefEd.arx", LoadCtrls = 13, Description = "AcadRefEdit Command Module", Managed = 0, Commands = new string[] { "REFCLOSE", "REFEDIT", "-REFEDIT", "REFSET", "REFSAVE" }, Groups = new string[] { "ACAD_REFEDIT" } },
            new CadAppDef { Name = "AcadRender", Loader = "AcRender.arx", LoadCtrls = 13, Description = "AcadRender Command Module", Managed = 0, Commands = new string[] { "CONVERTOLDMATERIALS", "3DSIN" }, Groups = new string[] { "ACAD_RENDER" } },
            new CadAppDef { Name = "AcadRevCloud", Loader = "AcRevcloud.arx", LoadCtrls = 12, Description = "AcadRevCloud Command Module", Managed = 0, Commands = new string[] { "REVCLOUD" }, Groups = new string[] { "ACAD_REVCLOUD" } },
            new CadAppDef { Name = "AcadSection", Loader = "AcSection.arx", LoadCtrls = 12, Description = "SectionPlane Command Module", Managed = 0, Commands = new string[] { "SECTIONPLANE", "FLATSHOT", "JOGSECTION", "LIVESECTION" }, Groups = new string[] { "ACAD_SECTION_PLANE" } },
            new CadAppDef { Name = "AcadSign", Loader = "AcSign.arx", LoadCtrls = 10, Description = "AcadSign Command Module", Managed = 0, Commands = new string[] { "SIGVALIDATE" }, Groups = new string[] { "ACAD_SIGN" } },
            new CadAppDef { Name = "AcadSolids", Loader = "AcSolids.arx", LoadCtrls = 13, Description = "AcadSolids Command Module", Managed = 0, Commands = new string[] { "SOLDRAW", "SOLPROF", "SOLVIEW" }, Groups = new string[] { "ACAD_SOLIDS" } },
            new CadAppDef { Name = "AcadSpaceTrans", Loader = "AcSpaceTrans.arx", LoadCtrls = 14, Description = "AcadSpaceTrans Command Module", Managed = 0, Commands = new string[] { "SPACETRANS" }, Groups = new string[] { "ACAD_SPACETRANS" } },
            new CadAppDef { Name = "AcadStandards", Loader = "AcStd.arx", LoadCtrls = 15, Description = "AcadStandards Command Module", Managed = 0, Commands = new string[] { "CHECKSTANDARDS", "STANDARDS" }, Groups = new string[] { "ACSTD_GROUP" } },
            new CadAppDef { Name = "AcadStar", Loader = "AcStar.arx", LoadCtrls = 8, Description = "AcStar", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcadSummaryInfoUI", Loader = "acsiui.arx", LoadCtrls = 13, Description = "AcadSummaryInfoUI Command Module", Managed = 0, Commands = new string[] { "DWGPROPS" }, Groups = new string[] { "ACAD_DWGPROPS" } },
            new CadAppDef { Name = "AcadTable", Loader = "AcTable.arx", LoadCtrls = 12, Description = "Table Command Module", Managed = 0, Commands = new string[] { "-TABLE", "TABLE", "TABLEDIT", "TABLESTYLE", "TINSERT", "TABLEEXPORT", "MATCHCELL", "ACAD_INTERNAL_CELLSELECT", "ACAD_INTERNAL_REPEATCELLEDIT", "INTERNALTABLEDIT" }, Groups = new string[] { "AcadAcTable" } },
            new CadAppDef { Name = "AcadTaskBar", Loader = "AcTaskBar.arx", LoadCtrls = 14, Description = "AcadTaskBar Command Module", Managed = 0, Commands = new string[] { "TaskBar" }, Groups = new string[] { "ACAD_TaskBar" } },
            new CadAppDef { Name = "AcadToolPalette", Loader = "AcTp.arx", LoadCtrls = 14, Description = "Tool Palette Module", Managed = 0, Commands = new string[] { "TOOLPALETTESCLOSE", "TOOLPALETTES", "BAUTHORPALETTE", "BAUTHORPALETTECLOSE" }, Groups = new string[] { "AcadAcTp" } },
            new CadAppDef { Name = "AcadTscale", Loader = "AcTscale.arx", LoadCtrls = 12, Description = "AcadTscale Command Module", Managed = 0, Commands = new string[] { "JUSTIFYTEXT", "SCALETEXT" }, Groups = new string[] { "ACAD_TSCALE" } },
            new CadAppDef { Name = "AcadUnits", Loader = "units.arx", LoadCtrls = 45, Description = "AcadUnits Command Module", Managed = 0, Commands = new string[] { "DDUNITS", "UNITS", "-UNITS" }, Groups = new string[] { "ACAD_UNITS" } },
            new CadAppDef { Name = "AcadVBA", Loader = "AcVBA.arx", LoadCtrls = 13, Description = "AcVBA Command Module", Managed = 0, Commands = new string[] { "VBAIDE", "VBALOAD", "-VBALOAD", "VBAMAN", "VBANEW", "VBAPREF", "VBARUN", "-VBARUN", "VBASTMT", "VBAUNLOAD" }, Groups = new string[] { "ACAD_VBA" } },
            new CadAppDef { Name = "AcadVisualStylesManager", Loader = "AcVisualStylesManager.arx", LoadCtrls = 13, Description = "AcadVisualStylesManager Command Module", Managed = 0, Commands = new string[] { "VISUALSTYLES", "VISUALSTYLESCLOSE" }, Groups = new string[] { "ACAD_AcVisualStylesManager" } },
            new CadAppDef { Name = "AcadVmTools", Loader = "acvmtools.arx", LoadCtrls = 44, Description = "AcadVmTools Command Module", Managed = 0, Commands = new string[] { "3DCLIP", "3DCORBIT", "3DFORBIT", "3DDISTANCE", "3DORBIT", "3DORBITCTR", "3DPAN", "3DPANTRANSPARENT", "3DSWIVEL", "3DZOOM", "3DZOOMTRANSPARENT", "HIDE", "SHADE", "SHADEMODE", "-SHADEMODE", "3DWALK", "3DFLY", "WALKFLYSETTINGS", "3DORBITTRANSPARENT", "3DSWIVELTRANSPARENT" }, Groups = new string[] { "ACAD_VMTOOLS" } },
            new CadAppDef { Name = "AcadVPMax", Loader = "acvpmaxmin.arx", LoadCtrls = 44, Description = "AcadVPMax Command Module", Managed = 0, Commands = new string[] { "VPMAX", "VPMIN", "UPDATETHUMBSNOW" }, Groups = new string[] { "ACAD_VPMAX" } },
            new CadAppDef { Name = "AcadWhoHas", Loader = "whohas.arx", LoadCtrls = 15, Description = "AcadWhoHas Command Module", Managed = 0, Commands = new string[] { "WHOHAS" }, Groups = new string[] { "ACAD_WHOHAS" } },
            new CadAppDef { Name = "AcadXopen", Loader = "AcXopen.arx", LoadCtrls = 12, Description = "AcadXopen Command Module", Managed = 0, Commands = new string[] { "XOPEN" }, Groups = new string[] { "AcXopen" } },
            new CadAppDef { Name = "AcadXref", Loader = "AcXrefEsw.arx", LoadCtrls = 12, Description = "AcadXref Command Module", Managed = 0, Commands = new string[] { "ExternalReferences", "ExternalReferencesClose", "ExacReload" }, Groups = new string[] { "AcXref" } },
            new CadAppDef { Name = "AcAeNet", Loader = "AcAeNet.dll", LoadCtrls = 13, Description = "AutoCAD Authoring Environment", Managed = 1, Commands = new string[] { "BlookupTable" }, Groups = new string[] { "ACAD_ACAE" } },
            new CadAppDef { Name = "AcCamera", Loader = "AcCamera.arx", LoadCtrls = 2, Description = "Camera Module", Managed = 0, Commands = new string[] { "CAMERA" }, Groups = new string[] { "AcCameraCommands" } },
            new CadAppDef { Name = "AcCustomize", Loader = "AcCustomize.dll", LoadCtrls = 12, Description = "AutoCAD Customization Dialog", Managed = 1, Commands = new string[] { "cui", "cuiexport", "cuiimport" }, Groups = new string[] { "CUSTOMIZATION_CMDS" } },
            new CadAppDef { Name = "AcCvPs", Loader = "AcCvps.arx", LoadCtrls = 12, Description = "AcCvPs Command Module", Managed = 0, Commands = new string[] { "CONVERTCTB", "CONVERTPSTYLES" }, Groups = new string[] { "ACLT_ACCVPS" } },
            new CadAppDef { Name = "AcDbDimAssoc", Loader = "acdim.arx", LoadCtrls = 15, Description = "AcDbDimAssoc Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "ACDDPTYPE", Loader = "acddptype.arx", LoadCtrls = 44, Description = "acddptype Command Module", Managed = 0, Commands = new string[] { "DDPTYPE" }, Groups = new string[] { "ACDDPTYPE_COMMANDS" } },
            new CadAppDef { Name = "ACDDVPOINT", Loader = "acddvpoint.arx", LoadCtrls = 12, Description = "acddvpoint Command Module", Managed = 0, Commands = new string[] { "DDVPOINT" }, Groups = new string[] { "ACDDVPOINT_COMMANDS" } },
            new CadAppDef { Name = "AcDwfMarkupUi", Loader = "AcDwfMarkupUi.arx", LoadCtrls = 12, Description = "Dwf Markup Ui Command Module", Managed = 0, Commands = new string[] { "MARKUP", "MARKUPCLOSE", "OPENDWFMARKUP" }, Groups = new string[] { "ACAD_DWFMarkupUi" } },
            new CadAppDef { Name = "AcDwgRecovery", Loader = "AcDwgRecovery.arx", LoadCtrls = 14, Description = "AutoCAD Drawing Recovery UI", Managed = 0, Commands = new string[] { "DRAWINGRECOVERY", "DRAWINGRECOVERYHIDE" }, Groups = new string[] { "AcDwgRecovery" } },
            new CadAppDef { Name = "AcDxNotify", Loader = "AcDxNotify.arx", LoadCtrls = 12, Description = "Attribute Extraction Notification Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcDxUi", Loader = "AcDxUi.dll", LoadCtrls = 14, Description = "AutoCAD Data Extraction", Managed = 1, Commands = null, Groups = null },
            new CadAppDef { Name = "AcDxWizard", Loader = "acdxwizard.dll", LoadCtrls = 12, Description = "AutoCAD Data Extraction Wizard", Managed = 1, Commands = new string[] { "EATTEXT", "-EATTEXT", "EATTEXTUPDATE", "EATTEXTEDIT" }, Groups = new string[] { "ACDXWIXARD_CMDS" } },
            new CadAppDef { Name = "AcEAttEdit", Loader = "AcEAttedit.arx", LoadCtrls = 12, Description = "AcEAttEdit Command Module", Managed = 0, Commands = new string[] { "EATTEDIT" }, Groups = new string[] { "ACAD_EAttedit" } },
            new CadAppDef { Name = "AcetDimsio", Loader = "Express\\dimsio.arx", LoadCtrls = 12, Description = "AcetDimsio", Managed = 0, Commands = new string[] { "DIMEX", "DIMIM" }, Groups = new string[] { "ACET_DIMSIO" } },
            new CadAppDef { Name = "AcetLeaderTools", Loader = "Express\\leaderex.arx", LoadCtrls = 12, Description = "AcetLeaderTools", Managed = 0, Commands = new string[] { "QLATTACH", "QLDETACHSET", "QLATTACHSET", "TXT2MTXT" }, Groups = new string[] { "ACET_LEADERTOOLS" } },
            new CadAppDef { Name = "AcetLodr", Loader = "acetlodr.arx", LoadCtrls = 10, Description = "AcetLodr Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcetMocoro", Loader = "Express\\Mocoro.arx", LoadCtrls = 12, Description = "AcetMocoro", Managed = 0, Commands = new string[] { "MOCORO" }, Groups = new string[] { "ACET_MOCORO" } },
            new CadAppDef { Name = "AcetMoveBak", Loader = "Express\\MoveBak.arx", LoadCtrls = 10, Description = "AcetMoveBak", Managed = 0, Commands = new string[] { "MOVEBAK" }, Groups = new string[] { "ACET_MOVEBAK" } },
            new CadAppDef { Name = "AcetPropulate", Loader = "Express\\propulate.arx", LoadCtrls = 12, Description = "AcetPropulate", Managed = 0, Commands = new string[] { "PROPULATE" }, Groups = new string[] { "ACET_PROPULATE" } },
            new CadAppDef { Name = "AcetSysvdlg", Loader = "Express\\Sysvdlg.arx", LoadCtrls = 12, Description = "RText", Managed = 0, Commands = new string[] { "SYSVDLG" }, Groups = new string[] { "ACET_SYSVDLG" } },
            new CadAppDef { Name = "Acetutil", Loader = "Express\\Acetutil.arx", LoadCtrls = 10, Description = "Acetutil", Managed = 0, Commands = new string[] { "" }, Groups = new string[] { "ACET_UTIL" } },
            new CadAppDef { Name = "ACFILTER", Loader = "acfilter.arx", LoadCtrls = 44, Description = "acfilter Command Module", Managed = 0, Commands = new string[] { "FILTER" }, Groups = new string[] { "ACFILTER_COMMANDS" } },
            new CadAppDef { Name = "AcInfoSvc", Loader = "AcInfoSvc.arx", LoadCtrls = 8, Description = "Information Services Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcInterfere", Loader = "AcInterfere.dll", LoadCtrls = 12, Description = "Interfere Command", Managed = 1, Commands = new string[] { "INTERFERE", "-INTERFERE" }, Groups = new string[] { "InterfereCommand" } },
            new CadAppDef { Name = "AcLayer", Loader = "aclayer.dll", LoadCtrls = 14, Description = "AutoCAD Layer Manager", Managed = 1, Commands = new string[] { "LAYER" }, Groups = new string[] { "ACLAYER_CMDS" } },
            new CadAppDef { Name = "AcLayerTools", Loader = "AcLayerTools.dll", LoadCtrls = 12, Description = "AutoCAD Layer Tools", Managed = 1, Commands = new string[] { "LAYISO", "LAYUNISO", "LAYON", "LAYOFF", "LAYCUR", "LAYFRZ", "LAYTHW", "LAYLCK", "LAYULK", "LAYVPI", "LAYMCH", "-LAYMCH", "LAYDEL", "-LAYDEL", "LAYMRG", "-LAYMRG", "COPYTOLAYER", "-COPYTOLAYER", "CHSPACE" }, Groups = new string[] { "LayerToolsCommands" } },
            new CadAppDef { Name = "AcLayWalk", Loader = "Laywalk.arx", LoadCtrls = 12, Description = "AcLayWalk", Managed = 0, Commands = new string[] { "LAYWALK" }, Groups = new string[] { "ACLAYWALK" } },
            new CadAppDef { Name = "AcMgd", Loader = "acmgd.dll", LoadCtrls = 14, Description = ".NET Managed Layer", Managed = 1, Commands = new string[] { "NETLOAD" }, Groups = new string[] { "ACMGD_CMDS" } },
            new CadAppDef { Name = "AcMPolygonMGD", Loader = "AcMPolygonObj17.dbx", LoadCtrls = 1, Description = "AcDbMPolygonMGD", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcMPolygonObj15", Loader = "AcMPolygonObj17.dbx", LoadCtrls = 1, Description = "AcDbMPolygon", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcMPolygonObj17", Loader = "AcMPolygonObj17.dbx", LoadCtrls = 1, Description = "AcDbMPolygon", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcMultiLineUi", Loader = "AcMultiLineUi.dll", LoadCtrls = 12, Description = "Multiline User Interface", Managed = 1, Commands = new string[] { "MLSTYLE", "MLEDIT" }, Groups = new string[] { "MlinesUICommands" } },
            new CadAppDef { Name = "AcObjClassImp", Loader = "AcObjClassImp.arx", LoadCtrls = 12, Description = "Object Classification Main API Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcPrevInput", Loader = "AcPrevInput.arx", LoadCtrls = 14, Description = "Previous Input Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcScaleList", Loader = "AcScaleList.dll", LoadCtrls = 44, Description = "Edit AutoCAD Scale List", Managed = 1, Commands = new string[] { "ScaleListEdit" }, Groups = new string[] { "ScaleListCommands" } },
            new CadAppDef { Name = "AcSceneUI", Loader = "AcSceneUI.arx", LoadCtrls = 13, Description = "SceneUI Command Module", Managed = 0, Commands = new string[] { "MATERIALS", "MATERIALATTACH", "MATERIALMAP", "MATERIALASSIGN", "MATERIALSCLOSE", "SPOTLIGHT", "POINTLIGHT", "DISTANTLIGHT", "GEOGRAPHICLOCATION", "SUNPROPERTIES", "SUNPROPERTIESCLOSE", "LIGHT", "LIGHTLIST", "LIGHTLISTCLOSE", "CONVERTOLDLIGHTS", "RENDER", "RENDERCROP", "RENDERWIN", "RENDERPRESETS", "RPREF", "RPREFCLOSE", "RENDERENVIRONMENT", "SAVEIMG", "ANIPATH" }, Groups = new string[] { "ACAD_SCENEUI" } },
            new CadAppDef { Name = "AcSecOpt", Loader = "AcSecOpt.arx", LoadCtrls = 12, Description = "AcSecOpt Command Module", Managed = 0, Commands = new string[] { "SECURITYOPTIONS" }, Groups = new string[] { "ACAD_SecOpt" } },
            new CadAppDef { Name = "AcSmDwgMine", Loader = "AcSmDwgMine.arx", LoadCtrls = 12, Description = "Sheet Set Manager Publish Utilities", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcSmNav", Loader = "AcSmNav.arx", LoadCtrls = 12, Description = "AutoCAD Sheet Set Manager UI", Managed = 0, Commands = new string[] { "SHEETSET", "OPENSHEETSET", "-OPENSHEETSET", "SHEETSETHIDE" }, Groups = new string[] { "ACSM_GROUP" } },
            new CadAppDef { Name = "AcSmPublishUtils", Loader = "AcSmPublishUtils.arx", LoadCtrls = 12, Description = "Sheet Set Manager Publish Utilities", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcSmSheetList", Loader = "AcSmSheetList.arx", LoadCtrls = 9, Description = "Table of Contents Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcStETransmit", Loader = "AcStETransmit.arx", LoadCtrls = 8, Description = "AcStETransmit", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcUnderlay", Loader = "AcUnderlay.DLL", LoadCtrls = 12, Description = "AutoCAD dwf/dgn in", Managed = 1, Commands = new string[] { "DWFATTACH", "-DWFATTACH", "DWFCLIP", "DWFADJUST", "-DWFADJUST" }, Groups = new string[] { "AcUnderlayCommands" } },
            new CadAppDef { Name = "AcViewMgr", Loader = "AcViewMgr.arx", LoadCtrls = 12, Description = "AcViewMgr Module", Managed = 0, Commands = new string[] { "VIEW", "NEWVIEW" }, Groups = new string[] { "ACAD_AcViewMgr" } },
            new CadAppDef { Name = "AcViewTransitionsUi", Loader = "AcViewTransitionsUi.dll", LoadCtrls = 12, Description = "View Transitions User Interface", Managed = 1, Commands = new string[] { "VTOPTIONS" }, Groups = new string[] { "AcViewTransitionsCommands" } },
            new CadAppDef { Name = "AcVpPlace", Loader = "AcVpPlace.arx", LoadCtrls = 12, Description = "AcVpPlace Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AcWipeoutUI", Loader = "AcWipeout.arx", LoadCtrls = 13, Description = "AcWipeout Command Module", Managed = 0, Commands = new string[] { "WIPEOUT" }, Groups = new string[] { "ACAD_WIPEOUT" } },
            new CadAppDef { Name = "ACXPLODE", Loader = "acxplode.arx", LoadCtrls = 44, Description = "AcXPlode Command Module", Managed = 0, Commands = new string[] { "XPLODE", "XP" }, Groups = new string[] { "ACXPLODE_COMMANDS" } },
            new CadAppDef { Name = "AecArchBase", Loader = "AecArchBase50.dbx", LoadCtrls = 9, Description = "AecArchBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecArchBase40", Loader = "AecArchBase50.dbx", LoadCtrls = 9, Description = "AecArchBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecArchBase50", Loader = "AecArchBase50.dbx", LoadCtrls = 9, Description = "AecArchBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecArchDACHBase", Loader = "AecArchDACHBase50.dbx", LoadCtrls = 13, Description = "AecArchDACHBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecArchDACHBase40", Loader = "AecArchDACHBase50.dbx", LoadCtrls = 13, Description = "AecArchDACHBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecArchDACHBase50", Loader = "AecArchDACHBase50.dbx", LoadCtrls = 13, Description = "AecArchDACHBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecAreaCalculationBase", Loader = "AecAreaCalculationBase50.dbx", LoadCtrls = 13, Description = "AecAreaCalculationBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecAreaCalculationBase40", Loader = "AecAreaCalculationBase50.dbx", LoadCtrls = 13, Description = "AecAreaCalculationBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecAreaCalculationBase50", Loader = "AecAreaCalculationBase50.dbx", LoadCtrls = 13, Description = "AecAreaCalculationBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecBase", Loader = "AecBase50.dbx", LoadCtrls = 9, Description = "AecBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecBase40", Loader = "AecBase50.dbx", LoadCtrls = 9, Description = "AecBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecBase50", Loader = "AecBase50.dbx", LoadCtrls = 9, Description = "AecBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecBaseEx50", Loader = "AecBaseEx50.dbx", LoadCtrls = 9, Description = "AecBaseEx50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecModeler50", Loader = "AecModeler50.dbx", LoadCtrls = 8, Description = "AecModeler50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecProjectBase50", Loader = "AecProjectBase50.dbx", LoadCtrls = 9, Description = "AecProjectBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecSchedule", Loader = "AecSchedule50.dbx", LoadCtrls = 9, Description = "AecSchedule50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecSchedule40", Loader = "AecSchedule50.dbx", LoadCtrls = 9, Description = "AecSchedule50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecSchedule50", Loader = "AecSchedule50.dbx", LoadCtrls = 9, Description = "AecSchedule50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecScheduleData", Loader = "AecScheduleData50.dbx", LoadCtrls = 9, Description = "AecScheduleData50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecScheduleData40", Loader = "AecScheduleData50.dbx", LoadCtrls = 9, Description = "AecScheduleData50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecScheduleData50", Loader = "AecScheduleData50.dbx", LoadCtrls = 9, Description = "AecScheduleData50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecStructureBase", Loader = "AecStructureBase50.dbx", LoadCtrls = 9, Description = "AecStructureBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecStructureBase40", Loader = "AecStructureBase50.dbx", LoadCtrls = 9, Description = "AecStructureBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecStructureBase50", Loader = "AecStructureBase50.dbx", LoadCtrls = 9, Description = "AecStructureBase50", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AecUiBase50", Loader = "AecUiBase50.arx", LoadCtrls = 13, Description = "AecUiBase50", Managed = 0, Commands = new string[] { "-AecExportToAutoCAD", "-AecDwgUnits", "AecFileSaveMessage", "AecObjectCopyMessage", "AecVersion", "AecFileOpenMessage", "AecSetXrefConfig", "AecPostDxfinFix", "AecObjRelUpdate", "AecObjRelShow", "AecObjRelDump", "AecDisplayManagerConfigsSelection" }, Groups = null },
            new CadAppDef { Name = "ASE-LPNTableRecord", Loader = "Ase.arx", LoadCtrls = 13, Description = "ASE-LPNTableRecord", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "AText", Loader = "Express\\ctextapp.arx", LoadCtrls = 5, Description = "AText", Managed = 0, Commands = new string[] { "ARCTEXT", "ATEXT" }, Groups = new string[] { "ACET_ATEXT" } },
            new CadAppDef { Name = "CalculatorUi", Loader = "AcCalcUi.dll", LoadCtrls = 44, Description = "Calculator UI", Managed = 1, Commands = new string[] { "quickcalc", "qcclose" }, Groups = new string[] { "AcCalcUiCommands" } },
            new CadAppDef { Name = "ISM", Loader = "acismobj17.dbx", LoadCtrls = 9, Description = "ISM", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "ISMUI", Loader = "acismui.arx", LoadCtrls = 13, Description = "ISM Command Module", Managed = 0, Commands = new string[] { "CLASSICIMAGE", "IMAGE", "-IMAGE", "IMAGEADJUST", "-IMAGEADJUST", "IMAGEATTACH", "IMAGECLIP", "IMAGEFRAME", "IMAGEQUALITY", "TRANSPARENCY" }, Groups = new string[] { "ISM_GROUP" } },
            new CadAppDef { Name = "LsObj", Loader = "AcLsObj.arx", LoadCtrls = 13, Description = "LsObj Command Module", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "Rectang", Loader = "rectang.arx", LoadCtrls = 13, Description = "Rectang Command Module", Managed = 0, Commands = new string[] { "RECTANG", "RECTANGLE" }, Groups = new string[] { "RECTANG_GROUP" } },
            new CadAppDef { Name = "RText", Loader = "Express\\RText.arx", LoadCtrls = 13, Description = "RText", Managed = 0, Commands = new string[] { "RTEXT" }, Groups = new string[] { "ACET_RTEXT" } },
            new CadAppDef { Name = "SceneOE", Loader = "AcSceneOE.dbx", LoadCtrls = 13, Description = "SceneOE", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "SheetSetManagerWizard", Loader = "AcSmWizard.arx", LoadCtrls = 12, Description = "Sheet Set Manager Wizard Command Module", Managed = 0, Commands = new string[] { "NEWSHEETSET" }, Groups = new string[] { "SheetSetManagerWizard" } },
            new CadAppDef { Name = "WipeOut", Loader = "AcWipeoutObj17.dbx", LoadCtrls = 13, Description = "AcWipeoutObj", Managed = 0, Commands = null, Groups = null },
            new CadAppDef { Name = "WSCommCntrAcCon", Loader = "WSCommCntrAcCon.arx", LoadCtrls = 2, Description = "WSCommCntrAcCon", Managed = 0, Commands = null, Groups = null },
        };

        public static void RegisterAll(string appDir)
        {
            try
            {
                // 1. Register in HKCU
                using (RegistryKey appRoot = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\Applications"))
                {
                    if (appRoot != null)
                    {
                        foreach (var def in Applications)
                        {
                            using (RegistryKey k = appRoot.CreateSubKey(def.Name))
                            {
                                if (k == null) continue;
                                string fullLoader = def.Loader;
                                if (!string.IsNullOrEmpty(fullLoader))
                                {
                                    if (fullLoader.StartsWith(@"C:\Program Files (x86)\AutoCAD 2007", StringComparison.OrdinalIgnoreCase))
                                        fullLoader = fullLoader.Substring(@"C:\Program Files (x86)\AutoCAD 2007".Length);
                                    if (!Path.IsPathRooted(fullLoader))
                                        fullLoader = Path.Combine(appDir, fullLoader);
                                }
                                k.SetValue("LOADER", fullLoader, RegistryValueKind.String);
                                k.SetValue("LOADCTRLS", def.LoadCtrls, RegistryValueKind.DWord);
                                if (!string.IsNullOrEmpty(def.Description))
                                    k.SetValue("DESCRIPTION", def.Description, RegistryValueKind.String);
                                if (def.Managed == 1)
                                    k.SetValue("MANAGED", 1, RegistryValueKind.DWord);
                                if (def.Commands != null && def.Commands.Length > 0)
                                {
                                    using (RegistryKey ck = k.CreateSubKey("Commands"))
                                    {
                                        if (ck != null) { foreach (var c in def.Commands) ck.SetValue(c, c, RegistryValueKind.String); }
                                    }
                                }
                                if (def.Groups != null && def.Groups.Length > 0)
                                {
                                    using (RegistryKey gk = k.CreateSubKey("Groups"))
                                    {
                                        if (gk != null) { foreach (var g in def.Groups) gk.SetValue(g, g, RegistryValueKind.String); }
                                    }
                                }
                            }
                        }
                        // TDTObjects demand-load registration
                        using (RegistryKey tdtKey = appRoot.CreateSubKey("TDTObjects"))
                        {
                            if (tdtKey != null)
                            {
                                tdtKey.SetValue("LOADER", Path.Combine(appDir, "TDTObjects17.dbx"), RegistryValueKind.String);
                                tdtKey.SetValue("LOADCTRLS", 13, RegistryValueKind.DWord);
                                tdtKey.SetValue("DESCRIPTION", "TDT Objects Module", RegistryValueKind.String);
                            }
                        }
                    }
                }

                // 2. Register DockingTools in HKCU
                using (RegistryKey dtRoot = Registry.CurrentUser.CreateSubKey(@"Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409\DockingTools"))
                {
                    if (dtRoot != null)
                    {
                        foreach (var dt in DockingTools)
                        {
                            using (RegistryKey k = dtRoot.CreateSubKey(dt.Guid))
                            {
                                if (k == null) continue;
                                k.SetValue("Name", dt.Name, RegistryValueKind.String);
                                k.SetValue("Command", dt.Command, RegistryValueKind.String);
                                k.SetValue("CommandClose", dt.CommandClose, RegistryValueKind.String);
                            }
                        }
                    }
                }

                // 3. Register in HKLM ONLY if AutoCAD is not installed elsewhere
                try
                {
                    bool canWriteHklm = false;
                    using (RegistryKey checkKey = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                    {
                        if (checkKey == null || checkKey.GetValue("AcadLocation") == null)
                        {
                            canWriteHklm = true;
                        }
                    }
                    if (canWriteHklm)
                    {
                        using (RegistryKey hklmRoot = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\Autodesk\AutoCAD\R17.0\ACAD-5001:409"))
                        {
                            if (hklmRoot != null)
                            {
                                hklmRoot.SetValue("AcadLocation", appDir, RegistryValueKind.String);
                                hklmRoot.SetValue("Location", appDir, RegistryValueKind.String);
                                using (RegistryKey dtKey = hklmRoot.CreateSubKey("DockingTools"))
                                {
                                    if (dtKey != null)
                                    {
                                        foreach (var dt in DockingTools)
                                        {
                                            using (RegistryKey k = dtKey.CreateSubKey(dt.Guid))
                                            {
                                                if (k != null) { k.SetValue("Name", dt.Name); k.SetValue("Command", dt.Command); k.SetValue("CommandClose", dt.CommandClose); }
                                            }
                                        }
                                    }
                                }
                                using (RegistryKey appKey = hklmRoot.CreateSubKey("Applications"))
                                {
                                    if (appKey != null)
                                    {
                                        foreach (var def in Applications)
                                        {
                                            using (RegistryKey k = appKey.CreateSubKey(def.Name))
                                            {
                                                if (k == null) continue;
                                                string fullLoader = def.Loader;
                                                if (!string.IsNullOrEmpty(fullLoader) && !Path.IsPathRooted(fullLoader))
                                                    fullLoader = Path.Combine(appDir, fullLoader);
                                                k.SetValue("LOADER", fullLoader, RegistryValueKind.String);
                                                k.SetValue("LOADCTRLS", def.LoadCtrls, RegistryValueKind.DWord);
                                                if (!string.IsNullOrEmpty(def.Description)) k.SetValue("DESCRIPTION", def.Description, RegistryValueKind.String);
                                                if (def.Managed == 1) k.SetValue("MANAGED", 1, RegistryValueKind.DWord);
                                                if (def.Commands != null && def.Commands.Length > 0)
                                                {
                                                    using (RegistryKey ck = k.CreateSubKey("Commands"))
                                                    {
                                                        if (ck != null) { foreach (var c in def.Commands) ck.SetValue(c, c, RegistryValueKind.String); }
                                                    }
                                                }
                                                if (def.Groups != null && def.Groups.Length > 0)
                                                {
                                                    using (RegistryKey gk = k.CreateSubKey("Groups"))
                                                    {
                                                        if (gk != null) { foreach (var g in def.Groups) gk.SetValue(g, g, RegistryValueKind.String); }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }
        }
    }
}