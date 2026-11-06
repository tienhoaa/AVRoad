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
    Font currentFont=new Font("Segoe UI",11F);
    IntPtr currentFontHandle;
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
        internal IntPtr Text; internal int MaxText,Image,SelectedImage,Children;
        internal IntPtr Parameter;
    }
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindow callback,IntPtr parameter);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent,EnumWindow callback,IntPtr parameter);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle,out uint process);
    [DllImport("user32.dll")] static extern int GetDlgCtrlID(IntPtr handle);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr handle,StringBuilder value,int maximum);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr handle,StringBuilder value,int maximum);
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

    internal NativeDlbdHost(IntPtr dataTree,DesignPanel designPanel) {
        tree=dataTree; panel=designPanel; parent=GetParent(tree);
        GetWindowRect(parent,out originalClient);
        GetWindowRect(tree,out originalBounds);
        MapWindowPoints(IntPtr.Zero,parent,ref originalBounds,2);
        GetClientRect(parent,out originalClient);
        originalFont=SendMessage(tree,GetFont,IntPtr.Zero,IntPtr.Zero);
        originalImages=SendMessage(tree,GetImages,IntPtr.Zero,IntPtr.Zero);
        originalRowHeight=SendMessage(tree,GetHeight,IntPtr.Zero,IntPtr.Zero).ToInt32();
        currentFontHandle=currentFont.ToHfont();
        try {
            AssignHandle(parent);
            toggleData.Text="\u1ea8n DLBD  \u25b2";
            toggleData.AccessibleName="\u1ea8n ho\u1eb7c hi\u1ec7n D\u1eef li\u1ec7u b\u00ecnh \u0111\u1ed3";
            toggleData.Font=new Font("Segoe UI",9.5F);
            toggleData.FlatStyle=FlatStyle.Flat;
            toggleData.BackColor=Color.FromArgb(237,241,244);
            toggleData.Click+=delegate {
                dataCollapsed=!dataCollapsed;
                toggleData.Text=dataCollapsed?"\u0048i\u1ec7n DLBD  \u25bc":"\u1ea8n DLBD  \u25b2";
                RefreshLayout();
            };
            panel.Visible=true;
            SetParent(toggleData.Handle,parent);
            SetParent(panel.Handle,parent);
            SendMessage(tree,SetFont,currentFontHandle,new IntPtr(1));
            SendMessage(tree,SetHeight,new IntPtr(26),IntPtr.Zero);
            ApplyModernIcons();
            RefreshLayout();
        } catch { Dispose(); throw; }
    }
    internal bool IsAttached { get { return !disposed && IsWindow(tree) && IsWindow(parent); } }
    internal bool IsVisible { get { return IsAttached && IsWindowVisible(tree); } }
    internal void ApplyFontSize(float sizePt, int rowHeight) {
        if(!IsAttached || disposed) return;
        try {
            IntPtr oldHandle = currentFontHandle;
            currentFont = new Font("Segoe UI", sizePt);
            currentFontHandle = currentFont.ToHfont();
            SendMessage(tree, SetFont, currentFontHandle, new IntPtr(1));
            SendMessage(tree, SetHeight, new IntPtr(rowHeight), IntPtr.Zero);
            if(oldHandle != IntPtr.Zero) DeleteObject(oldHandle);
            RefreshLayout();
        } catch { }
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
            int buttonHeight=Math.Min(28,height);
            toggleData.SetBounds(left,top,width,buttonHeight);
            int dataHeight=dataCollapsed?0:Math.Max(1,height/3-buttonHeight);
            ShowWindow(tree,dataCollapsed?0:5);
            SetWindowPos(tree,IntPtr.Zero,left,top+buttonHeight,width,dataHeight,NoActivate|NoZOrder);
            int toolsTop=top+buttonHeight+dataHeight+4;
            int toolsHeight=Math.Max(1,height-(buttonHeight+dataHeight+4));
            SetWindowPos(panel.Handle,IntPtr.Zero,left,toolsTop,width,toolsHeight,NoActivate|NoZOrder);
            panel.Invalidate(true);
        } finally { layingOut=false; }
    }
    void ApplyModernIcons() {
        if(originalImages==IntPtr.Zero) return;
        int count=ImageList_GetImageCount(originalImages);
        if(count<=0) return;
        images=new ImageList(); images.ImageSize=new Size(16,16); images.ColorDepth=ColorDepth.Depth32Bit;
        iconBitmaps.Clear();
        for(int i=0;i<count;i++) {
            IntPtr hIcon=ImageList_GetIcon(originalImages,i,0);
            if(hIcon!=IntPtr.Zero) {
                using(Icon icon=Icon.FromHandle(hIcon)) {
                    Bitmap bmp=icon.ToBitmap();
                    iconBitmaps.Add(bmp);
                    images.Images.Add(bmp);
                }
                DestroyIcon(hIcon);
            }
        }
        SendMessage(tree,SetImages,IntPtr.Zero,images.Handle);
    }
    protected override void WndProc(ref Message m) {
        base.WndProc(ref m);
        if(m.Msg==5 || m.Msg==0x0047) RefreshLayout(); // WM_SIZE or WM_WINDOWPOSCHANGED
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
        if(images!=null) { images.Dispose(); images=null; }
        foreach(Bitmap b in iconBitmaps) b.Dispose();
        iconBitmaps.Clear();
        if(currentFontHandle!=IntPtr.Zero) { DeleteObject(currentFontHandle); currentFontHandle=IntPtr.Zero; }
        if(currentFont!=null) { currentFont.Dispose(); currentFont=null; }
    }
    internal static IntPtr FindDataTree() {
        uint currentProcess=(uint)System.Diagnostics.Process.GetCurrentProcess().Id;
        IntPtr result=IntPtr.Zero;
        EnumWindows(delegate(IntPtr top,IntPtr p) {
            uint proc; GetWindowThreadProcessId(top,out proc);
            if(proc!=currentProcess || !IsWindowVisible(top)) return true;
            StringBuilder name=new StringBuilder(256);
            StringBuilder textBuf=new StringBuilder(256);
            EnumChildWindows(top,delegate(IntPtr child,IntPtr cp) {
                if(!IsWindowVisible(child)) return true;
                GetClassName(child,name,name.Capacity);
                if(name.ToString()=="SysTreeView32") {
                    // Check parent chain for AfxControlBar80u or "Dữ liệu bình đồ" / DLBD dockbar
                    IntPtr cur=GetParent(child);
                    for(int depth=0; depth<5 && cur!=IntPtr.Zero; depth++) {
                        GetClassName(cur,name,name.Capacity);
                        string cls=name.ToString();
                        GetWindowText(cur,textBuf,textBuf.Capacity);
                        string t=textBuf.ToString();
                        if(t.IndexOf("DLBD",StringComparison.OrdinalIgnoreCase)>=0 ||
                           t.IndexOf("li",StringComparison.OrdinalIgnoreCase)>=0 ||
                           (cls.IndexOf("AfxControlBar",StringComparison.OrdinalIgnoreCase)>=0 && t.IndexOf("Command",StringComparison.OrdinalIgnoreCase)<0)) {
                            result=child;
                            return false;
                        }
                        cur=GetParent(cur);
                    }
                }
                return true;
            },IntPtr.Zero);
            return result==IntPtr.Zero;
        },IntPtr.Zero);
        return result;
    }
}
}
