using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AVRoad.SinglePanel21 {
internal static class ToolIcons {
    internal static readonly string[] Keys={"survey","road","grading","canal","geology","traffic","terrain","profile","section","table","edit","delete","pick","calc","folder","command","search"};
    internal static string Kind(string title,bool group) {
        string s=title.ToLowerInvariant();
        if(s.Contains("biển báo")||s.Contains("vạch")||s.Contains("giao thông")) return "traffic";
        if(s.Contains("lỗ khoan")||s.Contains("địa chất")) return "geology";
        if(s.Contains("kênh")||s.Contains("thủy")) return "canal";
        if(s.Contains("xóa")||s.Contains("xoá")||s.Contains("loại bỏ")) return "delete";
        if(s.Contains("trắc dọc")||s.Contains("đường đỏ")||s.Contains("cong đứng")) return "profile";
        if(s.Contains("trắc ngang")||s.Contains("khuôn")||s.Contains("áo đường")||s.Contains("taluy")) return "section";
        if(s.Contains("địa hình")||s.Contains("bề mặt")||s.Contains("đồng mức")) return "terrain";
        if(s.Contains("bảng")||s.Contains("xuất")||s.Contains("thống kê")) return "table";
        if(s.Contains("tính")||s.Contains("đào đắp")||s.Contains("diện tích")) return "calc";
        if(s.Contains("điểm")||s.Contains("đo đạc")||s.Contains("tọa độ")) return "survey";
        if(s.Contains("tuyến")||s.Contains("lộ giới")||s.Contains("mặt bằng")) return "road";
        if(s.Contains("san nền")||s.Contains("lưới")||s.Contains("biên lô")||s.Contains("hướng dốc")) return "grading";
        if(s.Contains("hiệu chỉnh")||s.Contains("cài đặt")||s.Contains("điền")||s.Contains("sửa")) return "edit";
        if(s.Contains("chọn")||s.Contains("định nghĩa")) return "pick";
        return group?"folder":"command";
    }
    internal static Bitmap Create(string kind,int size) {
        Bitmap bitmap=new Bitmap(size,size);
        using(Graphics g=Graphics.FromImage(bitmap)) {
            g.SmoothingMode=SmoothingMode.AntiAlias;
            g.ScaleTransform(size/24F,size/24F);
            Color c=Color.FromArgb(56,114,147);
            if(kind=="road"||kind=="terrain") c=Color.FromArgb(33,130,110);
            if(kind=="grading"||kind=="edit"||kind=="folder") c=Color.FromArgb(177,126,40);
            if(kind=="profile"||kind=="calc") c=Color.FromArgb(117,92,160);
            if(kind=="delete") c=Color.FromArgb(184,72,67);
            using(Pen p=new Pen(c,1.8F)) using(SolidBrush b=new SolidBrush(Color.FromArgb(30,c))) {
                p.StartCap=LineCap.Round; p.EndCap=LineCap.Round; p.LineJoin=LineJoin.Round;
                if(kind=="canal") {
                    g.DrawLines(p,new PointF[]{new PointF(2,6),new PointF(7,18),new PointF(17,18),new PointF(22,6)});
                    g.DrawBezier(p,6,11,9,7,15,15,19,11); g.DrawLine(p,9,15,15,15);
                } else if(kind=="geology") {
                    g.DrawRectangle(p,3,3,18,18); g.DrawLines(p,new PointF[]{new PointF(3,9),new PointF(9,7),new PointF(15,10),new PointF(21,8)}); g.DrawLine(p,3,15,21,16); g.DrawLine(p,12,2,12,22);
                } else if(kind=="traffic") {
                    g.DrawPolygon(p,new PointF[]{new PointF(12,3),new PointF(22,19),new PointF(2,19)}); g.DrawLine(p,12,8,12,13); g.DrawEllipse(p,11.4F,15.5F,1.2F,1.2F);
                } else if(kind=="survey") {
                    g.DrawEllipse(p,5,5,14,14); g.DrawLine(p,12,2,12,8); g.DrawLine(p,12,16,12,22); g.DrawLine(p,2,12,8,12); g.DrawLine(p,16,12,22,12);
                    using(SolidBrush dot=new SolidBrush(c)) g.FillEllipse(dot,10,10,4,4);
                } else if(kind=="road") {
                    g.DrawBezier(p,5,22,3,12,17,14,15,2); g.DrawBezier(p,12,22,10,12,24,14,22,2);
                    using(Pen center=new Pen(c,1.1F)) { center.DashStyle=DashStyle.Dash; g.DrawBezier(center,8.5F,22,6.5F,12,20.5F,14,18.5F,2); }
                } else if(kind=="grading"||kind=="table") {
                    g.FillRectangle(b,3,4,18,16); g.DrawRectangle(p,3,4,18,16); g.DrawLine(p,3,9,21,9); g.DrawLine(p,3,14,21,14); g.DrawLine(p,9,4,9,20); g.DrawLine(p,15,4,15,20);
                    if(kind=="grading") { using(SolidBrush dot=new SolidBrush(c)) { g.FillEllipse(dot,7,7,4,4); g.FillEllipse(dot,13,12,4,4); } }
                } else if(kind=="terrain") {
                    PointF[] pts={new PointF(2,19),new PointF(9,5),new PointF(14,13),new PointF(17,8),new PointF(22,19)};
                    g.FillPolygon(b,pts); g.DrawPolygon(p,pts); g.DrawLine(p,7,14,17,14); g.DrawLine(p,5,17,20,17);
                } else if(kind=="profile") {
                    g.DrawLine(p,3,3,3,20); g.DrawLine(p,3,20,22,20); g.DrawLines(p,new PointF[]{new PointF(4,16),new PointF(9,12),new PointF(14,14),new PointF(21,5)});
                    using(Pen baseline=new Pen(Color.FromArgb(160,170,175),1)) { baseline.DashStyle=DashStyle.Dot; g.DrawLine(baseline,4,17,21,10); }
                } else if(kind=="section") {
                    g.DrawLine(p,2,20,22,20); g.DrawLines(p,new PointF[]{new PointF(2,16),new PointF(7,11),new PointF(17,11),new PointF(22,16)}); g.DrawLine(p,12,5,12,18); g.DrawLine(p,9,7,15,7);
                } else if(kind=="edit") {
                    g.DrawPolygon(p,new PointF[]{new PointF(4,16),new PointF(15,5),new PointF(19,9),new PointF(8,20),new PointF(3,21)}); g.DrawLine(p,13,7,17,11); g.DrawLine(p,4,16,8,20);
                } else if(kind=="delete") {
                    g.DrawLine(p,5,5,19,19); g.DrawLine(p,5,19,19,5);
                } else if(kind=="calc") {
                    g.FillRectangle(b,5,2,14,20); g.DrawRectangle(p,5,2,14,20); g.DrawRectangle(p,8,5,8,4); g.DrawLine(p,8,13,10,13); g.DrawLine(p,14,13,16,13); g.DrawLine(p,8,17,10,17); g.DrawLine(p,14,17,16,17);
                } else if(kind=="pick") {
                    PointF[] pts={new PointF(5,2),new PointF(5,19),new PointF(10,15),new PointF(13,22),new PointF(17,20),new PointF(14,13),new PointF(21,13)}; g.FillPolygon(b,pts); g.DrawPolygon(p,pts);
                } else if(kind=="folder") {
                    PointF[] pts={new PointF(3,6),new PointF(10,6),new PointF(12,9),new PointF(21,9),new PointF(21,20),new PointF(3,20)}; g.FillPolygon(b,pts); g.DrawPolygon(p,pts);
                } else if(kind=="search") {
                    g.DrawEllipse(p,3,3,12,12); g.DrawLine(p,14,14,21,21);
                } else {
                    g.DrawRectangle(p,4,3,16,18); g.DrawLine(p,8,8,16,8); g.DrawLine(p,8,12,16,12); g.DrawLine(p,8,16,13,16);
                }
            }
        }
        return bitmap;
    }
}
}
