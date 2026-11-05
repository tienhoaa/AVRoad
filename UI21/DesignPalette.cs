using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
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
    static NativeDlbdHost host;
    static XmlElement pending;
    static readonly System.Windows.Forms.Timer attachTimer=new System.Windows.Forms.Timer();
    static bool queued;
    static bool pendingLogged;
    static int attempts;
    public void Initialize() { }
    public void Terminate() { attachTimer.Stop(); if(host!=null) host.Dispose(); }
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
        try {
            IntPtr tree=NativeDlbdHost.FindDataTree();
            if(tree!=IntPtr.Zero) {
                if(host!=null) host.Dispose();
                host=new NativeDlbdHost(tree,panel);
                attachTimer.Stop(); queued=false; pendingLogged=false;
                DesignPanel.Log("HOST_ATTACHED","One native DLBD panel; data above, design tools below");
            } else if(!queued && attempts>=8) {
                Document doc=AcApp.DocumentManager.MdiActiveDocument;
                if(doc==null) return;
                queued=true;
                doc.SendStringToExecute("DLBD\n",true,false,false);
                DesignPanel.Log("DLBD_REQUESTED","Requested once after CAD startup; waiting for native DLBD window");
            }
        } catch(System.Exception ex) {
            attachTimer.Interval=1000;
            if(!pendingLogged) { pendingLogged=true; DesignPanel.Log("ATTACH_RETRY",ex.ToString()); }
        }
    }
    [CommandMethod("AVROADUI21CLOSE",CommandFlags.Session)] public void Hide() {
        attachTimer.Stop(); if(host!=null) { host.Dispose(); host=null; }
    }
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
        Icon=module=="KSVN"?"survey":module=="VNROAD"?"road":module=="SANNEN"?"grading":module=="KVN"?"canal":module=="DCCT"?"geology":"traffic";
    }
    public override string ToString() { return Title; }
}
public sealed class DesignPanel : UserControl {
    readonly TreeView tree=new TreeView(); readonly Label heading=new Label(); readonly Label detail=new Label(); readonly TextBox search=new TextBox();
    readonly List<ActionItem> actions=new List<ActionItem>(); readonly List<TreeNode> roots=new List<TreeNode>(); readonly ContextMenuStrip context=new ContextMenuStrip();
    readonly Dictionary<string,XmlDocument> menuCache=new Dictionary<string,XmlDocument>(StringComparer.OrdinalIgnoreCase);
    readonly ComboBox moduleSelect=new ComboBox();
    bool selectingModule;
    readonly ImageList icons=new ImageList();
    readonly Font groupFont=new Font("Segoe UI",14F,FontStyle.Bold);
    readonly string cadHome=Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
    public DesignPanel() {
        Dock=DockStyle.Fill; BackColor=Color.White; Font=new Font("Segoe UI",14F);
        heading.Dock=DockStyle.Top; heading.Height=37; heading.Padding=new Padding(8,6,0,0); heading.BackColor=Color.FromArgb(237,241,244); heading.ForeColor=Color.FromArgb(43,57,68); heading.Font=new Font("Segoe UI",13F,FontStyle.Bold);
        Panel selector=new Panel(); selector.Dock=DockStyle.Top; selector.Height=56; selector.Padding=new Padding(7,4,7,3); selector.BackColor=Color.FromArgb(247,249,250);
        moduleSelect.Dock=DockStyle.Fill; moduleSelect.DropDownStyle=ComboBoxStyle.DropDownList; moduleSelect.DrawMode=DrawMode.OwnerDrawFixed; moduleSelect.ItemHeight=43; moduleSelect.AccessibleName="Chọn nhóm công cụ AVRoad";
        foreach(XmlElement module in DesignCommands.Mapping.DocumentElement.ChildNodes) moduleSelect.Items.Add(new ModuleChoice(module));
        moduleSelect.DrawItem+=delegate(object sender,DrawItemEventArgs e) {
            e.DrawBackground(); if(e.Index<0) return;
            ModuleChoice choice=(ModuleChoice)moduleSelect.Items[e.Index];
            using(Bitmap symbol=ToolIcons.Create(choice.Icon,32)) e.Graphics.DrawImage(symbol,e.Bounds.X+6,e.Bounds.Y+5,32,32);
            TextRenderer.DrawText(e.Graphics,choice.Title,Font,new Rectangle(e.Bounds.X+47,e.Bounds.Y+2,e.Bounds.Width-51,39),e.ForeColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        };
        moduleSelect.SelectedIndexChanged+=delegate { if(!selectingModule && moduleSelect.SelectedItem!=null) ShowModule(((ModuleChoice)moduleSelect.SelectedItem).Id); };
        selector.Controls.Add(moduleSelect);
        search.Dock=DockStyle.Fill; search.AccessibleName="Tìm trong Toolpage đang mở"; search.TextChanged+=delegate { Filter(); };
        icons.ImageSize=new Size(32,32); icons.ColorDepth=ColorDepth.Depth32Bit;
        foreach(string mark in ToolIcons.Keys) icons.Images.Add(mark,ToolIcons.Create(mark,32));
        tree.ImageList=icons; tree.Dock=DockStyle.Fill; tree.BorderStyle=BorderStyle.None; tree.BackColor=BackColor; tree.HideSelection=false; tree.FullRowSelect=true; tree.ShowNodeToolTips=true; tree.ItemHeight=44; tree.Indent=25;
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
        detail.Dock=DockStyle.Bottom; detail.Height=64; detail.Padding=new Padding(7,5,5,0); detail.Font=new Font("Segoe UI",11F); detail.BackColor=Color.FromArgb(241,245,247); detail.ForeColor=Color.FromArgb(74,88,98);
        Panel find=new Panel(); find.Dock=DockStyle.Top; find.Height=44; find.Padding=new Padding(7,3,7,3); find.BackColor=Color.FromArgb(247,249,250);
        PictureBox magnifier=new PictureBox(); magnifier.Image=ToolIcons.Create("search",24); magnifier.SizeMode=PictureBoxSizeMode.CenterImage; magnifier.Dock=DockStyle.Left; magnifier.Width=31;
        find.Controls.Add(search); find.Controls.Add(magnifier);
        Controls.Add(tree); Controls.Add(detail); Controls.Add(find); Controls.Add(selector); Controls.Add(heading);
    }
    internal void ShowPage(XmlElement page) {
        System.Diagnostics.Stopwatch elapsed=System.Diagnostics.Stopwatch.StartNew();
        menuCache.Clear(); // One read per CUI for this refresh; execution still checks disk again.
        tree.BeginUpdate(); tree.Nodes.Clear(); roots.Clear(); actions.Clear(); search.Text="";
        heading.Text="CÔNG CỤ THIẾT KẾ";
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
        if(el.Name=="action") return el.GetAttribute("command")=="DLBD" || el.GetAttribute("command")=="TAT";
        string title=el.GetAttribute("title");
        return title=="In ấn" || title=="In" || title.StartsWith("Tiện ích",StringComparison.Ordinal) || title=="Tạo hatch người dùng";
    }
    TreeNode Build(XmlElement el) {
        if(Omit(el)) return null;
        if(el.Name=="group") { TreeNode group=new TreeNode(el.GetAttribute("title")); group.NodeFont=groupFont; group.ImageKey=ToolIcons.Kind(el.GetAttribute("title"),true); group.SelectedImageKey=group.ImageKey; foreach(XmlElement child in el.ChildNodes) { TreeNode node=Build(child); if(node!=null) group.Nodes.Add(node); } return group.Nodes.Count==0?null:group; }
        ActionItem item=new ActionItem(); item.Title=el.GetAttribute("title"); item.Command=el.GetAttribute("command"); item.Description=el.GetAttribute("description"); item.Menu=el.GetAttribute("menu"); item.Uid=el.GetAttribute("uid"); item.MenuName=el.GetAttribute("menuName"); item.Available=Verify(item); actions.Add(item);
        TreeNode leaf=new TreeNode(item.Title); leaf.Tag=item; leaf.ToolTipText=item.Command+" — "+item.Description;
        leaf.ImageKey=ToolIcons.Kind(item.Title,false); leaf.SelectedImageKey=leaf.ImageKey;
        if(!item.Available) { leaf.ForeColor=SystemColors.GrayText; leaf.ToolTipText="Mapping không khớp menu đang cài; không gọi lệnh."; }
        return leaf;
    }
    bool Verify(ActionItem item) { return Verify(item,false); }
    bool Verify(ActionItem item,bool fresh) {
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
    static string Normalize(string value) { StringBuilder b=new StringBuilder(); foreach(char c in value.ToLowerInvariant().Normalize(NormalizationForm.FormD)) if(System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)!=System.Globalization.UnicodeCategory.NonSpacingMark) b.Append(c=='đ' ? 'd' : c); return b.ToString(); }
    void Filter() {
        string query=Normalize(search.Text.Trim()); tree.BeginUpdate(); tree.Nodes.Clear();
        if(query.Length==0) { foreach(TreeNode node in roots) tree.Nodes.Add(node); }
        else foreach(ActionItem item in actions) if(Normalize(item.Title+" "+item.Command+" "+item.Description).IndexOf(query)>=0) { TreeNode node=new TreeNode(item.Title); node.Tag=item; node.ImageKey=ToolIcons.Kind(item.Title,false); node.SelectedImageKey=node.ImageKey; node.ToolTipText=item.Command+" — "+item.Description; if(!item.Available) node.ForeColor=SystemColors.GrayText; tree.Nodes.Add(node); }
        tree.EndUpdate();
    }
    void Execute(ActionItem item) {
        Document document=AcApp.DocumentManager.MdiActiveDocument; if(document==null || item==null) return;
        if(!item.Available || !Verify(item,true)) { detail.Text="Mapping không khớp menu đang cài. Không gọi lệnh."; return; }
        if(Convert.ToInt32(AcApp.GetSystemVariable("CMDACTIVE"))!=0) { detail.Text="Hoàn thành lệnh đang chạy, hoặc nhấn Esc trong CAD rồi chọn lại."; return; }
        try { Log("QUEUED",item.Command+" | "+document.Name); document.SendStringToExecute(item.Command+"\n",true,false,true); Log("SENT",item.Command); detail.Text=item.Command+" — "+item.Description; }
        catch(System.Exception ex) { detail.Text="Không gọi được lệnh: "+ex.Message; Log("ERROR",ex.ToString()); }
    }
    internal static void Log(string kind,string text) { try { File.AppendAllText(Path.Combine(DesignCommands.Home,"commands-observed.log"),DateTime.Now.ToString("s")+" "+kind+" "+text+Environment.NewLine); } catch { } }
}
}
