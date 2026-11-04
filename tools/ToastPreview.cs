using System;
using System.Drawing;
using System.Windows.Forms;
using AVRoadLauncher;
class ToastPreview {
 [STAThread] static void Main(string[] args) {
 Application.EnableVisualStyles();
 using (UpdateToastForm toast = new UpdateToastForm("1.3", "Cải thiện độ ổn định, cập nhật giao diện và trải nghiệm thiết kế.", "", null, AppDomain.CurrentDomain.BaseDirectory)) {
 toast.Show(); Application.DoEvents();
 Rectangle work = Screen.FromControl(toast).WorkingArea;
 if (toast.Right > work.Right || toast.Bottom > work.Bottom || Math.Abs(work.Right-toast.Right-16)>2 || Math.Abs(work.Bottom-toast.Bottom-16)>2) throw new Exception("Toast is outside bottom-right working area");
 if (toast.ShowInTaskbar || toast.FormBorderStyle != FormBorderStyle.None || !toast.TopMost) throw new Exception("Unexpected toast behavior");
 using(Bitmap b = new Bitmap(toast.Width,toast.Height)) { toast.DrawToBitmap(b,new Rectangle(Point.Empty,b.Size)); b.Save(args[0]); }
 toast.Close();
 }
 }
}
