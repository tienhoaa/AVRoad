using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using AVRoad.Security;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AVRoad.SinglePanel21.DesignCommands))]
[assembly: ExtensionApplication(typeof(AVRoad.SinglePanel21.DesignCommands))]

namespace AVRoad.SinglePanel21 {
public sealed class DesignCommands : IExtensionApplication {
    internal static readonly string Home=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    static XmlDocument mapping;
    static DesignPanel panel;
    internal static NativeDlbdHost host;
    static XmlElement pending;
    static readonly System.Windows.Forms.Timer attachTimer=new System.Windows.Forms.Timer();
    static bool queued;
    static bool pendingLogged;
    static int attempts;
    public void Initialize() {
        try { AcApp.SetSystemVariable("PICKFIRST", 1); } catch { }
        try {
            LicenseManager.Initialize();
            AcApp.DocumentManager.DocumentLockModeChanged += OnDocumentLockModeChanged;
            InitBlockedCommands();
        } catch { }
    }
    public void Terminate() {
        attachTimer.Stop();
        if(host!=null) host.Dispose();
        try {
            AcApp.DocumentManager.DocumentLockModeChanged -= OnDocumentLockModeChanged;
        } catch { }
    }
    static string SettingsBackup {
        get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AVRoad"),"UI21\\drafting-before.xml"); }
    }
    [CommandMethod("AVROADDEFAULTS21",CommandFlags.Session)] public void ApplyDraftingDefaults() {
        if(!String.Equals(Convert.ToString(AcApp.GetSystemVariable("CPROFILE")),"AVRoad",StringComparison.OrdinalIgnoreCase)) return;
        if(File.Exists(SettingsBackup)) return; // Preserve subsequent user choices.
        string[] names={"VTENABLE","VTDURATION","SELECTIONPREVIEW"}; int[] values={3,180,2};
        XmlDocument backup=new XmlDocument(); XmlElement root=backup.CreateElement("Drafting"); backup.AppendChild(root);
        foreach(string name in names) { XmlElement row=backup.CreateElement("Variable"); row.SetAttribute("name",name); row.SetAttribute("value",Convert.ToString(AcApp.GetSystemVariable(name),System.Globalization.CultureInfo.InvariantCulture)); root.AppendChild(row); }
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsBackup)); backup.Save(SettingsBackup);
        try {
            for(int i=0;i<names.Length;i++) AcApp.SetSystemVariable(names[i],values[i]);
            AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute("(command \"._-toolbar\" \"Workspaces\" \"_hide\")\n(command \"._-toolbar\" \"Layers\" \"_top\" \"0,1\")\n",true,false,false);
            DesignPanel.Log("DEFAULTS","AVRoad profile; smooth transitions retained; GPU driver unchanged; backup="+SettingsBackup);
        } catch { RestoreDraftingDefaults(); throw; }
    }
    [CommandMethod("AVROADRESTORE21",CommandFlags.Session)] public void RestoreDraftingDefaults() {
        if(!String.Equals(Convert.ToString(AcApp.GetSystemVariable("CPROFILE")),"AVRoad",StringComparison.OrdinalIgnoreCase) || !File.Exists(SettingsBackup)) return;
        XmlDocument backup=new XmlDocument(); backup.XmlResolver=null; backup.Load(SettingsBackup);
        foreach(XmlElement row in backup.DocumentElement.ChildNodes) {
            string name=row.GetAttribute("name");
            if(name=="VTENABLE" || name=="VTDURATION" || name=="SELECTIONPREVIEW") AcApp.SetSystemVariable(name,Int32.Parse(row.GetAttribute("value"),System.Globalization.CultureInfo.InvariantCulture));
        }
        DesignPanel.Log("DEFAULTS_RESTORED",SettingsBackup);
    }
    internal static XmlDocument Mapping { get {
        if(mapping==null) { mapping=new XmlDocument(); mapping.XmlResolver=null; mapping.Load(Path.Combine(Home,"commands.xml")); }
        return mapping;
    } }
    static XmlElement Page(string id) {
        if(!Regex.IsMatch(id,"^[A-Z0-9]+-PMU_[0-9]+$")) return null;
        return Mapping.SelectSingleNode("//group[@id='"+id+"']") as XmlElement;
    }
    [CommandMethod("AVROADUI21",CommandFlags.Session)] public void ShowDefault() {
        try { AcApp.SetSystemVariable("PICKFIRST", 1); } catch { }
        if (!LicenseManager.IsActiveLicenseValid()) {
            ShowActivationPrompt();
            if (!LicenseManager.IsActiveLicenseValid()) return;
        }
        try { AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute("DLBD\n",true,false,false); } catch { }
        string id="VNROAD-PMU_0002";
        try { string saved=File.ReadAllText(Path.Combine(Home,"last-page.txt")).Trim(); if(Page(saved)!=null) id=saved; } catch { }
        ShowPage(id);
    }
    [CommandMethod("AVROADUIPAGE")] public void ChoosePage() {
        Document doc=AcApp.DocumentManager.MdiActiveDocument; if(doc==null) return;
        PromptResult result=doc.Editor.GetString("\nToolpage: ");
        if(result.Status==PromptStatus.OK) ShowPage(result.StringResult);
    }
    static void ShowPage(string id) {
        pending=Page(id); if(pending==null) return;
        if(panel==null || panel.IsDisposed) panel=new DesignPanel();
        panel.ShowPage(pending);
        if(host!=null && host.IsAttached) {
            if(!host.IsVisible) AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute("DLBD\n",true,false,false);
            panel.SyncDlbdFont(host);
            host.RefreshLayout(); return;
        }
        attachTimer.Stop(); queued=false; pendingLogged=false; attempts=0;
        attachTimer.Tick-=Attach; attachTimer.Tick+=Attach; attachTimer.Interval=250;
        Attach(null,EventArgs.Empty);
        if(host==null || !host.IsAttached) attachTimer.Start();
    }
    static void Attach(object sender,EventArgs args) {
        attempts++;
        if(attempts>=24) {
            attachTimer.Interval=1000;
            if(!pendingLogged) { pendingLogged=true; DesignPanel.Log("ATTACH_WAIT","DLBD is not visible yet; continuing to watch for it"); }
        }
        if (!LicenseManager.IsActiveLicenseValid()) {
            attachTimer.Stop();
            return;
        }
        try {
            IntPtr tree=NativeDlbdHost.FindDataTree();
            if(tree!=IntPtr.Zero) {
                if(host!=null) host.Dispose();
                host=new NativeDlbdHost(tree,panel);
                if(panel!=null && !panel.IsDisposed) panel.SyncDlbdFont(host);
                attachTimer.Stop(); queued=false; pendingLogged=false;
                DesignPanel.Log("HOST_ATTACHED","One native DLBD panel; data above, design tools below");
            } else if((attempts % 4 == 0 || attempts == 1) && attempts <= 32) {
                Document doc=AcApp.DocumentManager.MdiActiveDocument;
                if(doc!=null) {
                    doc.SendStringToExecute("DLBD\n",true,false,false);
                    DesignPanel.Log("DLBD_REQUESTED","Attempt " + attempts + ": requesting DLBD window");
                }
            }
        } catch(System.Exception ex) {
            attachTimer.Interval=1000;
            if(!pendingLogged) { pendingLogged=true; DesignPanel.Log("ATTACH_RETRY",ex.ToString()); }
        }
    }
    [CommandMethod("AVROADUI21CLOSE",CommandFlags.Session)] public void Hide() {
        attachTimer.Stop(); if(host!=null) { host.Dispose(); host=null; }
    }

    #region Stealth Protection Interceptor & License Commands
    static Dictionary<string, bool> blockedCommands;

    static void InitBlockedCommands() {
        if (blockedCommands != null) return;
        blockedCommands = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        string[] coreTdt = new string[] {
            "TT", "TKAD", "DAODAP", "KMC", "HMC", "TN", "TD", "DCCT",
            "VBD", "CDTN", "CDTD", "KC", "GMC", "NDTN", "TDTN", "XLS", "CBSP",
            "LTN", "LTD", "KMT", "DTK", "TKT", "PLT", "MC", "LMC", "APG", "APMC",
            "HCT", "TBN", "TBD", "BTT", "DTD", "XTD", "DDTM", "NCD", "XD", "BTD",
            "ND", "CN", "CDT", "LDT", "TDT", "CNT", "DCT", "DVT", "DVDM2", "SLT",
            "BDD", "DTST", "BTST", "TTST", "TDBDT", "BKD", "CNMIA", "RMIA", "CONG",
            "TIM", "DMH", "XSLT", "XT", "MCDH", "TCD", "D2D", "TD3D", "TVBD", "CDD",
            "TAT", "KTT", "STT", "TLK", "TKD", "TKMD", "TKTN", "TKTD", "GMC2", "TCT",
            "TCL", "TPD", "PST", "GTC", "TMC", "TGT", "VMD", "VMC", "VTD", "VTN",
            "DDT", "DTN", "TNC", "TDC", "CDO", "KLT", "KLP", "KLG", "KLL", "KLD",
            "BKL", "XKL", "TKL", "LKL", "PKL", "TTKL", "KLTN", "KLTD", "KLMC"
        };
        for (int i = 0; i < coreTdt.Length; i++) {
            blockedCommands[coreTdt[i]] = true;
        }

        try {
            XmlDocument doc = Mapping;
            if (doc != null) {
                XmlNodeList list = doc.SelectNodes("//action/@command");
                if (list != null) {
                    foreach (XmlAttribute attr in list) {
                        string c = (attr.Value ?? string.Empty).Trim().ToUpperInvariant();
                        if (!string.IsNullOrEmpty(c) && !IsWhitelisted(c)) {
                            blockedCommands[c] = true;
                        }
                    }
                }
            }
        } catch { }
    }

    static bool IsWhitelisted(string cmd) {
        if (string.IsNullOrEmpty(cmd)) return true;
        string u = cmd.ToUpperInvariant();
        if (u.StartsWith("AV_") || u.StartsWith("AVROAD") || u.StartsWith("RELOADGUI") ||
            u.StartsWith("VSI_") || u.StartsWith("KHOIDONG") || u.StartsWith("QUIT") ||
            u.StartsWith("CLOSE") || u.StartsWith("QSAVE") || u.StartsWith("SAVE") ||
            u.StartsWith("OPTIONS") || u.StartsWith("UNDO") || u.StartsWith("REDO") ||
            u.StartsWith("ZOOM") || u.StartsWith("PAN") || u.StartsWith("HELP") ||
            u.StartsWith("ABOUT") || u.StartsWith("CANCEL") || u.StartsWith("_.NETLOAD") ||
            u.StartsWith("NETLOAD") || u.StartsWith("APPLOAD"))
            return true;
        return false;
    }

    static bool IsBlockedCommand(string cmd) {
        if (string.IsNullOrEmpty(cmd)) return false;
        if (IsWhitelisted(cmd)) return false;

        InitBlockedCommands();
        if (blockedCommands != null && blockedCommands.ContainsKey(cmd))
            return true;

        return false;
    }

    static bool _activationPromptShowing = false;
    static DateTime _lastPromptTime = DateTime.MinValue;

    static void OnDocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e) {
        try {
            string cmd = (e.GlobalCommandName ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(cmd)) return;

            if (IsBlockedCommand(cmd)) {
                if (LicenseManager.IsRevoked) {
                    e.Veto();
                    LicenseManager.NotifyOutdated();
                    return;
                }

                if (!LicenseManager.IsActiveLicenseValid()) {
                    e.Veto();

                    try {
                        Document doc = AcApp.DocumentManager.MdiActiveDocument;
                        if (doc != null) {
                            doc.Editor.WriteMessage(string.Format("\n[AVRoad] LỆNH '{0}' BỊ CHẶN: Bản quyền phần mềm chưa được kích hoạt!\n", cmd));
                        }
                    } catch { }

                    if (!_activationPromptShowing && (DateTime.Now - _lastPromptTime).TotalSeconds > 3) {
                        _activationPromptShowing = true;
                        _lastPromptTime = DateTime.Now;
                        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                        t.Interval = 200;
                        t.Tick += delegate {
                            t.Stop();
                            t.Dispose();
                            try {
                                ShowActivationPrompt();
                            } finally {
                                _activationPromptShowing = false;
                            }
                        };
                        t.Start();
                    }
                }
            }
        } catch { }
    }

    public static void ShowActivationPrompt() {
        try {
            using (LicenseDialog dlg = new LicenseDialog()) {
                AcApp.ShowModalDialog(dlg);
            }
        } catch {
            LicenseDialog.ShowActivationDialog();
        }
    }

    [CommandMethod("AV_LICENSE")]
    [CommandMethod("AV_BANQUYEN")]
    public void ShowLicenseDialog() {
        try {
            using (LicenseDialog dlg = new LicenseDialog()) {
                AcApp.ShowModalDialog(dlg);
            }
        } catch (System.Exception ex) {
            Document doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc != null) doc.Editor.WriteMessage("\n[AVRoad] Không thể mở hộp thoại bản quyền: " + ex.Message + "\n");
        }
    }

    [CommandMethod("AV_HWID")]
    public void ShowHwid() {
        try {
            string hwid = HardwareIdentifier.GetHardwareId();
            try { Clipboard.SetText(hwid); } catch { }
            Document doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc != null) {
                doc.Editor.WriteMessage("\n[AVRoad] Mã máy (HWID): " + hwid + " (Đã sao chép vào bộ nhớ tạm Clipboard)\n");
            }
        } catch (System.Exception ex) {
            Document doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc != null) doc.Editor.WriteMessage("\n[AVRoad] Lỗi: " + ex.Message + "\n");
        }
    }
    #endregion
}

internal sealed class ActionItem {
    internal string Title,Command,Description,Menu,Uid,MenuName;
    internal bool Available;
}

internal sealed class ModuleChoice {
    internal readonly string Id,Title,Icon;
    internal ModuleChoice(XmlElement root) {
        Id=root.GetAttribute("id"); Title=root.GetAttribute("title");
        string module=Id.Split('-')[0];
        Icon=module=="KSVN"?"survey":module=="VNROAD"?"road":module=="SANNEN"?"grading":module=="KVN"?"canal":module=="DCCT"?"geology":module=="RM"?"traffic":"tool";
    }
    public override string ToString() { return Title; }
}

public sealed class DesignPanel : UserControl {
    readonly TreeView tree=new TreeView();
    readonly Panel header=new Panel();
    readonly Label headingTitle=new Label();
    readonly ComboBox fontSelector=new ComboBox();
    readonly Label detail=new Label();
    readonly TextBox search=new TextBox();
    readonly Panel selector=new Panel();
    readonly Panel find=new Panel();
    readonly List<ActionItem> actions=new List<ActionItem>();
    readonly List<TreeNode> roots=new List<TreeNode>();
    readonly ContextMenuStrip context=new ContextMenuStrip();
    readonly Dictionary<string,XmlDocument> menuCache=new Dictionary<string,XmlDocument>(StringComparer.OrdinalIgnoreCase);
    readonly ComboBox moduleSelect=new ComboBox();
    bool selectingModule;
    readonly ImageList icons=new ImageList();
    Font groupFont=new Font("Segoe UI",12F,FontStyle.Bold);
    readonly string cadHome=Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
    int currentFontLevel=1;

    public DesignPanel() {
        Dock=DockStyle.Fill; BackColor=Color.White; Font=new Font("Segoe UI",11.5F);

        header.Dock=DockStyle.Top; header.Height=35; header.Padding=new Padding(7,3,7,3);
        header.BackColor=Color.FromArgb(237,241,244);
        headingTitle.Text="CÔNG CỤ THIẾT KẾ"; headingTitle.Dock=DockStyle.Fill;
        headingTitle.Font=new Font("Segoe UI",11F,FontStyle.Bold); headingTitle.ForeColor=Color.FromArgb(43,57,68);
        headingTitle.TextAlign=ContentAlignment.MiddleLeft;

        fontSelector.Dock=DockStyle.Right; fontSelector.Width=92; fontSelector.DropDownStyle=ComboBoxStyle.DropDownList;
        fontSelector.Font=new Font("Segoe UI",9F); fontSelector.AccessibleName="Cỡ chữ hiển thị";
        fontSelector.Items.AddRange(new object[] { "Nhỏ (9pt)", "Vừa (11pt)", "Lớn (13pt)" });
        fontSelector.SelectedIndexChanged+=delegate {
            if(fontSelector.SelectedIndex!=currentFontLevel && fontSelector.SelectedIndex>=0) {
                ApplyFontSizeLevel(fontSelector.SelectedIndex);
            }
        };
        header.Controls.Add(headingTitle); header.Controls.Add(fontSelector);

        selector.Dock=DockStyle.Top; selector.Height=48; selector.Padding=new Padding(7,4,7,3); selector.BackColor=Color.FromArgb(247,249,250);
        moduleSelect.Dock=DockStyle.Fill; moduleSelect.DropDownStyle=ComboBoxStyle.DropDownList; moduleSelect.DrawMode=DrawMode.OwnerDrawFixed; moduleSelect.ItemHeight=36; moduleSelect.AccessibleName="Chọn nhóm công cụ AVRoad";
        foreach(XmlElement module in DesignCommands.Mapping.DocumentElement.ChildNodes) moduleSelect.Items.Add(new ModuleChoice(module));
        moduleSelect.DrawItem+=delegate(object sender,DrawItemEventArgs e) {
            e.DrawBackground(); if(e.Index<0) return;
            ModuleChoice choice=(ModuleChoice)moduleSelect.Items[e.Index];
            using(Bitmap symbol=ToolIcons.Create(choice.Icon,28)) e.Graphics.DrawImage(symbol,e.Bounds.X+4,e.Bounds.Y+4,28,28);
            TextRenderer.DrawText(e.Graphics,choice.Title,Font,new Rectangle(e.Bounds.X+40,e.Bounds.Y+2,e.Bounds.Width-44,e.Bounds.Height-4),e.ForeColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        };
        moduleSelect.SelectedIndexChanged+=delegate { if(!selectingModule && moduleSelect.SelectedItem!=null) ShowModule(((ModuleChoice)moduleSelect.SelectedItem).Id); };
        selector.Controls.Add(moduleSelect);

        search.Dock=DockStyle.Fill; search.AccessibleName="Tìm trong Toolpage đang mở"; search.TextChanged+=delegate { Filter(); };
        icons.ImageSize=new Size(28,28); icons.ColorDepth=ColorDepth.Depth32Bit;
        foreach(string mark in ToolIcons.Keys) icons.Images.Add(mark,ToolIcons.Create(mark,28));
        tree.ImageList=icons; tree.Dock=DockStyle.Fill; tree.BorderStyle=BorderStyle.None; tree.BackColor=BackColor; tree.HideSelection=false; tree.FullRowSelect=true; tree.ShowNodeToolTips=true; tree.ItemHeight=34; tree.Indent=22;
        tree.AfterSelect+=delegate(object s,TreeViewEventArgs e) { Describe(e.Node); };
        tree.NodeMouseClick+=delegate(object s,TreeNodeMouseClickEventArgs e) {
            tree.SelectedNode=e.Node;
            if(e.Button==MouseButtons.Left && e.Node.Tag==null && e.X>e.Node.Bounds.Left) e.Node.Toggle();
            if(e.Button==MouseButtons.Right && e.Node.Tag!=null) {
                context.Items.Clear(); ActionItem item=(ActionItem)e.Node.Tag;
                ToolStripMenuItem run=new ToolStripMenuItem("Gọi lệnh "+item.Command); run.Enabled=item.Available;
                run.Click+=delegate { Execute(item); }; context.Items.Add(run); context.Show(tree,e.Location);
            }
        };
        tree.NodeMouseDoubleClick+=delegate(object s,TreeNodeMouseClickEventArgs e) { if(e.Node.Tag!=null) Execute((ActionItem)e.Node.Tag); };
        tree.KeyDown+=delegate(object s,KeyEventArgs e) { if(e.KeyCode==Keys.Enter && tree.SelectedNode!=null && tree.SelectedNode.Tag!=null) { e.Handled=true; e.SuppressKeyPress=true; Execute((ActionItem)tree.SelectedNode.Tag); } };

        detail.Dock=DockStyle.Bottom; detail.Height=56; detail.Padding=new Padding(7,4,5,0); detail.Font=new Font("Segoe UI",10F); detail.BackColor=Color.FromArgb(241,245,247); detail.ForeColor=Color.FromArgb(74,88,98);

        find.Dock=DockStyle.Top; find.Height=38; find.Padding=new Padding(7,3,7,3); find.BackColor=Color.FromArgb(247,249,250);
        PictureBox magnifier=new PictureBox(); magnifier.Image=ToolIcons.Create("search",22); magnifier.SizeMode=PictureBoxSizeMode.CenterImage; magnifier.Dock=DockStyle.Left; magnifier.Width=28;
        find.Controls.Add(search); find.Controls.Add(magnifier);

        Controls.Add(tree); Controls.Add(detail); Controls.Add(find); Controls.Add(selector); Controls.Add(header);

        // Load font preference
        int initLevel=1;
        try {
            string saved=File.ReadAllText(Path.Combine(DesignCommands.Home,"font-size.txt")).Trim();
            int val; if(int.TryParse(saved,out val) && val>=0 && val<=2) initLevel=val;
            else if(Screen.PrimaryScreen.Bounds.Height<=800) initLevel=0;
        } catch { if(Screen.PrimaryScreen.Bounds.Height<=800) initLevel=0; }
        currentFontLevel=initLevel;
        fontSelector.SelectedIndex=initLevel;
        ApplyFontSizeLevel(initLevel, false);
    }

    void ApplyFontSizeLevel(int level) { ApplyFontSizeLevel(level, true); }
    void ApplyFontSizeLevel(int level, bool save) {
        currentFontLevel=level;
        float mainSize, grpSize, detSize;
        int tRow, mRow, selH, fH, detH;
        if(level==0) { // Small (9pt)
            mainSize=9.5F; grpSize=10F; detSize=8.5F;
            tRow=26; mRow=28; selH=40; fH=32; detH=48;
        } else if(level==2) { // Large (13pt)
            mainSize=13.5F; grpSize=14F; detSize=11F;
            tRow=42; mRow=43; selH=56; fH=44; detH=64;
        } else { // Medium (11pt)
            mainSize=11.5F; grpSize=12F; detSize=10F;
            tRow=34; mRow=36; selH=48; fH=38; detH=56;
        }
        Font=new Font("Segoe UI",mainSize);
        tree.Font=Font; tree.ItemHeight=tRow;
        groupFont=new Font("Segoe UI",grpSize,FontStyle.Bold);
        moduleSelect.Font=Font; moduleSelect.ItemHeight=mRow;
        selector.Height=selH; find.Height=fH; search.Font=Font;
        detail.Font=new Font("Segoe UI",detSize); detail.Height=detH;

        if(DesignCommands.host!=null && DesignCommands.host.IsAttached) {
            SyncDlbdFont(DesignCommands.host);
        }
        if(save) {
            try { File.WriteAllText(Path.Combine(DesignCommands.Home,"font-size.txt"),level.ToString()); } catch { }
        }
        if(tree.Nodes.Count>0) {
            UpdateGroupNodeFonts(tree.Nodes);
        }
    }

    void UpdateGroupNodeFonts(TreeNodeCollection nodes) {
        foreach(TreeNode n in nodes) {
            if(n.Tag==null) n.NodeFont=groupFont;
            if(n.Nodes.Count>0) UpdateGroupNodeFonts(n.Nodes);
        }
    }

    internal void SyncDlbdFont(NativeDlbdHost h) {
        if(h==null || !h.IsAttached) return;
        float dlbdSize=currentFontLevel==0?9.5F:(currentFontLevel==2?12.5F:11F);
        int dlbdRow=currentFontLevel==0?22:(currentFontLevel==2?30:26);
        h.ApplyFontSize(dlbdSize,dlbdRow);
    }

    internal void ShowPage(XmlElement page) {
        System.Diagnostics.Stopwatch elapsed=System.Diagnostics.Stopwatch.StartNew();
        menuCache.Clear();
        tree.BeginUpdate(); tree.Nodes.Clear(); roots.Clear(); actions.Clear(); search.Text="";
        string module=page.GetAttribute("id").Split('-')[0];
        selectingModule=true;
        foreach(ModuleChoice choice in moduleSelect.Items) if(choice.Id.StartsWith(module+"-",StringComparison.Ordinal)) { moduleSelect.SelectedItem=choice; break; }
        selectingModule=false;
        foreach(XmlElement child in page.ChildNodes) { TreeNode node=Build(child); if(node!=null) roots.Add(node); }
        foreach(TreeNode node in roots) tree.Nodes.Add(node);
        detail.Text="Nhấp đúp để chạy · Chuột phải để xem lệnh.\nChỉ tìm trong nhóm đang chọn."; tree.EndUpdate();
        Log("PAGE",page.GetAttribute("id")+" | "+actions.Count+" entries | "+elapsed.ElapsedMilliseconds+" ms; CUI reads="+menuCache.Count);
    }

    void ShowModule(string id) {
        XmlElement module=DesignCommands.Mapping.SelectSingleNode("//group[@id='"+id+"']") as XmlElement;
        if(module==null) return;
        ShowPage(module);
        try { File.WriteAllText(Path.Combine(DesignCommands.Home,"last-page.txt"),id); } catch { }
    }

    static bool Omit(XmlElement el) {
        if(el.Name=="action") {
            if(el.GetAttribute("menu")=="CUSTOM") return false;
            return el.GetAttribute("command")=="DLBD" || el.GetAttribute("command")=="TAT";
        }
        string id=el.GetAttribute("id");
        if(id.StartsWith("TIENICH-", StringComparison.OrdinalIgnoreCase)) return false;
        string title=el.GetAttribute("title");
        return title=="In ấn" || title=="In" || title.StartsWith("Tiện ích",StringComparison.Ordinal) || title=="Tạo hatch người dùng";
    }

    TreeNode Build(XmlElement el) {
        if(Omit(el)) return null;
        if(el.Name=="group") {
            TreeNode group=new TreeNode(el.GetAttribute("title")); group.NodeFont=groupFont;
            group.ImageKey=ToolIcons.Kind(el.GetAttribute("title"),true); group.SelectedImageKey=group.ImageKey;
            foreach(XmlElement child in el.ChildNodes) { TreeNode node=Build(child); if(node!=null) group.Nodes.Add(node); }
            return group.Nodes.Count==0?null:group;
        }
        ActionItem item=new ActionItem(); item.Title=el.GetAttribute("title"); item.Command=el.GetAttribute("command");
        item.Description=el.GetAttribute("description"); item.Menu=el.GetAttribute("menu"); item.Uid=el.GetAttribute("uid");
        item.MenuName=el.GetAttribute("menuName"); item.Available=Verify(item); actions.Add(item);
        TreeNode leaf=new TreeNode(item.Title); leaf.Tag=item; leaf.ToolTipText=item.Command+" — "+item.Description;
        leaf.ImageKey=ToolIcons.Kind(item.Title,false); leaf.SelectedImageKey=leaf.ImageKey;
        if(!item.Available) { leaf.ForeColor=SystemColors.GrayText; leaf.ToolTipText="Mapping không khớp menu đang cài; không gọi lệnh."; }
        return leaf;
    }

    bool Verify(ActionItem item) { return Verify(item,false); }
    bool Verify(ActionItem item,bool fresh) {
        if(item.Menu=="CUSTOM") {
            return Regex.IsMatch(item.Command,@"^[A-Za-z0-9_\+\-]+$");
        }
        if(!Regex.IsMatch(item.Command,"^[A-Za-z0-9_]+$") || !Regex.IsMatch(item.Menu,"^[A-Z0-9]+$") || !Regex.IsMatch(item.Uid,"^MMU_[0-9]+$")) return false;
        try { XmlDocument menu;
            if(fresh || !menuCache.TryGetValue(item.Menu,out menu)) {
                menu=new XmlDocument(); menu.XmlResolver=null; menu.Load(Path.Combine(cadHome,item.Menu+".cui"));
                if(!fresh) menuCache[item.Menu]=menu;
            }
            XmlNode macro=menu.SelectSingleNode("//MenuMacro[@UID='"+item.Uid+"']/Macro"); return macro!=null && macro["Command"].InnerText.Trim()=="^C^C"+item.Command && macro["Name"].InnerText==item.MenuName;
        } catch { return false; }
    }

    void Describe(TreeNode node) { ActionItem item=node.Tag as ActionItem; detail.Text=item==null ? node.Text+"\nBấm để mở / đóng nhóm." : item.Command+" — "+item.Description+(item.Available ? "" : "\nMapping chưa khớp: đã khóa."); }

    static string Normalize(string value) {
        StringBuilder b=new StringBuilder();
        foreach(char c in value.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            if(System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)!=System.Globalization.UnicodeCategory.NonSpacingMark)
                b.Append(c=='đ' ? 'd' : c);
        return b.ToString();
    }

    void Filter() {
        string query=Normalize(search.Text.Trim()); tree.BeginUpdate(); tree.Nodes.Clear();
        if(query.Length==0) { foreach(TreeNode node in roots) tree.Nodes.Add(node); }
        else foreach(ActionItem item in actions) if(Normalize(item.Title+" "+item.Command+" "+item.Description).IndexOf(query)>=0) {
            TreeNode node=new TreeNode(item.Title); node.Tag=item; node.ImageKey=ToolIcons.Kind(item.Title,false);
            node.SelectedImageKey=node.ImageKey; node.ToolTipText=item.Command+" — "+item.Description;
            if(!item.Available) node.ForeColor=SystemColors.GrayText; tree.Nodes.Add(node);
        }
        tree.EndUpdate();
    }

    void Execute(ActionItem item) {
        Document document=AcApp.DocumentManager.MdiActiveDocument; if(document==null || item==null) return;
        if (!LicenseManager.IsActiveLicenseValid()) {
            detail.Text = "Chưa kích hoạt bản quyền! Vui lòng nạp key để sử dụng.";
            DesignCommands.ShowActivationPrompt();
            return;
        }
        if(!item.Available || !Verify(item,true)) { detail.Text="Mapping không khớp menu đang cài. Không gọi lệnh."; return; }
        if(Convert.ToInt32(AcApp.GetSystemVariable("CMDACTIVE"))!=0) { detail.Text="Hoàn thành lệnh đang chạy, hoặc nhấn Esc trong CAD rồi chọn lại."; return; }
        try {
            if(item.Menu=="CUSTOM") {
                string toolsDir=Path.Combine(cadHome,@"Support\Tools");
                if(!Directory.Exists(toolsDir)) {
                    string alt=Path.Combine(DesignCommands.Home,@"Support\Tools");
                    if(Directory.Exists(alt)) toolsDir=alt;
                }
                if(item.Command=="OPENTOOLSDIR") {
                    if(!Directory.Exists(toolsDir)) Directory.CreateDirectory(toolsDir);
                    System.Diagnostics.Process.Start("explorer.exe",toolsDir);
                    detail.Text="Đã mở thư mục tiện ích: "+toolsDir;
                    return;
                }
                if(item.Command=="APPLOAD") {
                    document.SendStringToExecute("._APPLOAD\n",true,false,false);
                    detail.Text="Mở hộp thoại APPLOAD nạp ứng dụng";
                    return;
                }
                if(item.Command=="MPL") {
                    string mplPath=Path.Combine(toolsDir,"MPL_inModelLayout.vlx").Replace('\\','/');
                    string lisp="(vl-load-com) (if (null c:mpl) (load \""+mplPath+"\" nil)) (c:mpl)\n";
                    document.SendStringToExecute(lisp,true,false,false);
                    Log("SENT_CUSTOM","MPL");
                    detail.Text="In nhanh Model & Layout (Lệnh: MPL)";
                    return;
                }
                if(item.Command=="CHUYENDOIFONT" || item.Command=="CFT" || item.Command=="CFV" || item.Command=="CFU" || item.Command=="CFK" || item.Command=="CF+" || item.Command=="CF-" || item.Command=="CFX") {
                    string cfPath=Path.Combine(toolsDir,"convertfont.vlx").Replace('\\','/');
                    string lisp="(vl-load-com) (if (null c:chuyendoifont) (load \""+cfPath+"\" nil)) (c:"+item.Command.ToLowerInvariant()+")\n";
                    document.SendStringToExecute(lisp,true,false,false);
                    Log("SENT_CUSTOM",item.Command);
                    detail.Text=item.Title+" (Lệnh: "+item.Command+")";
                    return;
                }
            }
            Log("QUEUED",item.Command+" | "+document.Name);
            document.SendStringToExecute(item.Command+"\n",true,false,true);
            Log("SENT",item.Command);
            detail.Text=item.Command+" — "+item.Description;
        }
        catch(System.Exception ex) { detail.Text="Không gọi được lệnh: "+ex.Message; Log("ERROR",ex.ToString()); }
    }

    internal static void Log(string kind,string text) {
        try { File.AppendAllText(Path.Combine(DesignCommands.Home,"commands-observed.log"),DateTime.Now.ToString("s")+" "+kind+" "+text+Environment.NewLine); } catch { }
    }
}
}
