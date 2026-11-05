using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AVRoad.SinglePanel21 {
// Composes our control below the existing native tree. The TDT tree is never
// recreated, reparented, repopulated or given replacement event handlers.
// Native bounds, font, row height and image-list handle are restored on detach.
internal sealed class NativeDlbdHost : NativeWindow, IDisposable {
    readonly IntPtr tree,parent;
    readonly DesignPanel panel;
    readonly Button toggleData=new Button();
    bool dataCollapsed;
    readonly Rect originalBounds,originalClient;
    readonly IntPtr originalFont,originalImages;
    readonly int originalRowHeight;
    readonly Font font=new Font("Segoe UI",12F);
    readonly IntPtr fontHandle;
    ImageList images;
    readonly List<Bitmap> iconBitmaps=new List<Bitmap>();
    bool disposed,layingOut;
    const uint NoActivate=0x10,NoZOrder=0x4;
    const int GetRoot=0x110A,GetItem=0x113E,GetImages=0x1108,SetImages=0x1109;
    const int GetHeight=0x111C,SetHeight=0x111B,GetFont=0x31,SetFont=0x30;
    delegate bool EnumWindow(IntPtr handle,IntPtr parameter);
    [StructLayout(LayoutKind.Sequential)] struct Rect { internal int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential)] struct TreeItem {
        internal uint Mask; internal IntPtr Item; internal uint State,StateMask;
        internal IntPtr Text; internal int MaxText,Image,SelectedImage,Children; internal IntPtr Parameter;
    }
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindow callback,IntPtr parameter);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent,EnumWindow callback,IntPtr parameter);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle,out uint process);
    [DllImport("user32.dll")] static extern int GetDlgCtrlID(IntPtr handle);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr handle,StringBuilder value,int maximum);
    [DllImport("user32.dll")] static extern bool IsWindow(IntPtr handle);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr handle);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr handle,int command);
    [DllImport("user32.dll")] static extern IntPtr GetParent(IntPtr handle);
    [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr child,IntPtr parent);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr handle,out Rect rect);
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr handle,out Rect rect);
    [DllImport("user32.dll")] static extern int MapWindowPoints(IntPtr from,IntPtr to,ref Rect rect,uint count);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr handle,IntPtr after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr handle,int message,IntPtr wParam,IntPtr lParam);
    [DllImport("user32.dll",EntryPoint="SendMessageW")] static extern IntPtr ReadItem(IntPtr handle,int message,IntPtr wParam,ref TreeItem item);
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr handle);
    [DllImport("comctl32.dll")] static extern int ImageList_GetImageCount(IntPtr handle);
    [DllImport("comctl32.dll")] static extern IntPtr ImageList_GetIcon(IntPtr handle,int index,uint flags);

    internal static IntPtr FindDataTree() {
        IntPtr found=IntPtr.Zero;
        uint current=(uint)System.Diagnostics.Process.GetCurrentProcess().Id;
        EnumWindow check=delegate(IntPtr h,IntPtr p) {
            uint owner; GetWindowThreadProcessId(h,out owner);
            if(owner!=current || GetDlgCtrlID(h)!=1029 || !IsWindowVisible(h)) return true;
            StringBuilder type=new StringBuilder(64); GetClassName(h,type,type.Capacity);
            if(type.ToString()!="SysTreeView32") return true;
            IntPtr root=SendMessage(h,GetRoot,IntPtr.Zero,IntPtr.Zero);
            TreeItem item; string text;
            if(TryRead(h,root,out item,out text) && text=="Điểm đo") { found=h; return false; }
            return true;
        };
        EnumWindows(delegate(IntPtr h,IntPtr p) {
            uint owner; GetWindowThreadProcessId(h,out owner);
            if(owner==current) { check(h,p); if(found==IntPtr.Zero) EnumChildWindows(h,check,IntPtr.Zero); }
            return found==IntPtr.Zero;
        },IntPtr.Zero);
        return found;
    }

    internal NativeDlbdHost(IntPtr treeHandle,DesignPanel designPanel) {
        tree=treeHandle; panel=designPanel; parent=GetParent(tree);
        if(parent==IntPtr.Zero) throw new InvalidOperationException("DLBD has no native content container.");
        GetWindowRect(tree,out originalBounds); MapWindowPoints(IntPtr.Zero,parent,ref originalBounds,2);
        GetClientRect(parent,out originalClient);
        originalFont=SendMessage(tree,GetFont,IntPtr.Zero,IntPtr.Zero);
        originalImages=SendMessage(tree,GetImages,IntPtr.Zero,IntPtr.Zero);
        originalRowHeight=SendMessage(tree,GetHeight,IntPtr.Zero,IntPtr.Zero).ToInt32();
        fontHandle=font.ToHfont();
        try {
            AssignHandle(parent);
            toggleData.Text="Ẩn DLBD ▲";
            toggleData.AccessibleName="Ẩn hoặc hiện Dữ liệu bình đồ";
            toggleData.Font=new Font("Segoe UI",10F);
            toggleData.FlatStyle=FlatStyle.Flat;
            toggleData.BackColor=Color.FromArgb(237,241,244);
            toggleData.Click+=delegate {
                dataCollapsed=!dataCollapsed;
                toggleData.Text=dataCollapsed?"Hiện DLBD ▼":"Ẩn DLBD ▲";
                RefreshLayout();
            };
            toggleData.CreateControl();
            SetParent(toggleData.Handle,parent);
            toggleData.Visible=true;
            panel.Dock=DockStyle.None;
            panel.CreateControl();
            SetParent(panel.Handle,parent);
            panel.Visible=true;
            SendMessage(tree,SetFont,fontHandle,new IntPtr(1));
            SendMessage(tree,SetHeight,new IntPtr(32),IntPtr.Zero);
            StyleIcons();
            RefreshLayout();
        } catch { Dispose(); throw; }
    }
    internal bool IsAttached { get { return !disposed && IsWindow(parent) && IsWindow(tree); } }
    internal bool IsVisible { get { return IsAttached && IsWindowVisible(parent); } }

    static bool TryRead(IntPtr tree,IntPtr node,out TreeItem item,out string text) {
        item=new TreeItem(); text="";
        if(node==IntPtr.Zero) return false;
        IntPtr buffer=Marshal.AllocHGlobal(1024);
        try {
            item.Mask=1|2|0x20; item.Item=node; item.Text=buffer; item.MaxText=512;
            Marshal.WriteInt16(buffer,0);
            if(ReadItem(tree,GetItem,IntPtr.Zero,ref item)==IntPtr.Zero) return false;
            text=Marshal.PtrToStringUni(buffer); return true;
        } finally { Marshal.FreeHGlobal(buffer); item.Text=IntPtr.Zero; }
    }

    void StyleIcons() {
        if(originalImages==IntPtr.Zero) return;
        int count=ImageList_GetImageCount(originalImages);
        if(count<=0 || count>512) return;
        images=new ImageList(); images.ImageSize=new Size(24,24); images.ColorDepth=ColorDepth.Depth32Bit;
        for(int i=0;i<count;i++) {
            IntPtr native=ImageList_GetIcon(originalImages,i,0);
            Bitmap bitmap;
            if(native==IntPtr.Zero) bitmap=ToolIcons.Create("folder",24);
            else {
                try { using(Icon original=Icon.FromHandle(native)) bitmap=original.ToBitmap(); }
                finally { DestroyIcon(native); }
            }
            iconBitmaps.Add(bitmap); images.Images.Add(bitmap);
        }
        IntPtr node=SendMessage(tree,GetRoot,IntPtr.Zero,IntPtr.Zero);
        while(node!=IntPtr.Zero) {
            TreeItem item; string text;
            if(TryRead(tree,node,out item,out text)) {
                string kind=text=="Điểm đo"?"survey":text=="Mô hình địa hình"?"terrain":text=="Lưới tuyến"?"road":text=="Khung bản đồ"?"table":null;
                if(kind!=null) {
                    foreach(int index in new int[]{item.Image,item.SelectedImage}) if(index>=0 && index<count) {
                        Bitmap bitmap=ToolIcons.Create(kind,24); iconBitmaps.Add(bitmap); images.Images[index]=bitmap;
                    }
                }
            }
            node=SendMessage(tree,GetRoot,new IntPtr(1),node); // TVGN_NEXT
        }
        // Preserve every native image index, including images for dynamic TDT data.
        SendMessage(tree,SetImages,IntPtr.Zero,images.Handle);
    }

    internal void RefreshLayout() {
        if(!IsAttached || layingOut) return;
        layingOut=true;
        try {
            Rect client; GetClientRect(parent,out client);
            int left=originalBounds.Left,top=originalBounds.Top;
            int rightMargin=Math.Max(0,originalClient.Right-originalBounds.Right);
            int bottomMargin=Math.Max(0,originalClient.Bottom-originalBounds.Bottom);
            int width=Math.Max(1,client.Right-left-rightMargin);
            int height=Math.Max(1,client.Bottom-top-bottomMargin);
            int buttonHeight=Math.Min(30,height);
            toggleData.SetBounds(left,top,width,buttonHeight);
            int dataHeight=dataCollapsed?0:Math.Max(1,height/3-buttonHeight);
            ShowWindow(tree,dataCollapsed?0:5);
            SetWindowPos(tree,IntPtr.Zero,left,top+buttonHeight,width,Math.Max(1,dataHeight),NoActivate|NoZOrder);
            int toolsTop=buttonHeight+dataHeight+5;
            panel.SetBounds(left,top+toolsTop,width,Math.Max(1,height-toolsTop));
            DesignPanel.Log("LAYOUT_ACTUAL","DLBD="+dataHeight+";Tools="+panel.Height+";Total="+height);
        } finally { layingOut=false; }
    }

    protected override void WndProc(ref Message message) {
        base.WndProc(ref message);
        if(message.Msg==5 && !disposed) RefreshLayout(); // layout after the TDT WM_SIZE handler
    }

    public void Dispose() {
        if(disposed) return;
        disposed=true;
        ReleaseHandle();
        toggleData.Dispose();
        if(panel!=null && !panel.IsDisposed) {
            panel.Visible=false;
            if(IsWindow(panel.Handle)) SetParent(panel.Handle,IntPtr.Zero);
        }
        if(IsWindow(tree)) {
            ShowWindow(tree,5);
            SendMessage(tree,SetFont,originalFont,new IntPtr(1));
            SendMessage(tree,SetHeight,new IntPtr(originalRowHeight),IntPtr.Zero);
            SendMessage(tree,SetImages,IntPtr.Zero,originalImages);
            Rect client; GetClientRect(parent,out client);
            int width=Math.Max(1,client.Right-originalBounds.Left-Math.Max(0,originalClient.Right-originalBounds.Right));
            int height=Math.Max(1,client.Bottom-originalBounds.Top-Math.Max(0,originalClient.Bottom-originalBounds.Bottom));
            SetWindowPos(tree,IntPtr.Zero,originalBounds.Left,originalBounds.Top,width,height,NoActivate|NoZOrder);
        }
        if(images!=null) images.Dispose();
        foreach(Bitmap bitmap in iconBitmaps) bitmap.Dispose();
        if(fontHandle!=IntPtr.Zero) DeleteObject(fontHandle);
        font.Dispose();
        DesignPanel.Log("HOST_DETACHED","Native tree style and full bounds restored");
    }
}
}
