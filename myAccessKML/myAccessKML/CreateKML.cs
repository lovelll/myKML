using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace myAccessKML
{
    class CreateKML
    {
        /// <summary>
        /// 逐级增加节点方法
        /// 由于要关联图片用到description:::<![CDATA[<img src='……' width='250 '/>]]>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sPath"></param>
        public static void CreateXML(DataTable dt,string fPath)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(dec);
            //创建一个根节点（一级）
            XmlElement root = doc.CreateElement("kml", "http://earth.google.com/kml/2.1");
            doc.AppendChild(root);
            //创建节点（二级）
            XmlNode Document = doc.CreateElement("Document",null);
            //创建节点（三级）
            XmlNode Folder = doc.CreateElement("Folder");

            int nCount = dt.Rows.Count;
            for (int i = 0; i < nCount; i++)
            {
  
                string sLong = dt.Rows[i][4].ToString().Trim();
                string sLat = dt.Rows[i][5].ToString().Trim();
                string sCoordinates = sLong + "," + sLat + ","  + dt.Rows[i][6].ToString().Trim();
                string sImageLink = dt.Rows[i][9].ToString().Trim();
                string sDescription = "<img src=\"" + sImageLink + "\" width=\"300\"/>";

                //创建节点（4级）
                XmlNode Placemark = doc.CreateElement("Placemark");

                //写入name标签
                XmlElement PlacemarkName = doc.CreateElement("name");
                string pName = dt.Rows[i][0].ToString().Trim() +"  "+ dt.Rows[i][1].ToString().Trim() + dt.Rows[i][2].ToString().Trim();
                PlacemarkName.InnerText = pName;
                Placemark.AppendChild(PlacemarkName);

                XmlNode description = doc.CreateElement("Description");
                XmlNode CDATA = doc.CreateNode(XmlNodeType.CDATA, "", "");
                CDATA.InnerText = sDescription;
                description.AppendChild(CDATA);
                
                //建立反映各种作物情况的表格
                XmlNode tb = doc.CreateElement("Table");
                XmlNode border = doc.CreateNode(XmlNodeType.Attribute, "border", "");
                border.Value = "1";
                tb.Attributes.SetNamedItem(border);

                for(int a = 1;a <= 4;a++)
                {
                    XmlNode tr1 = doc.CreateElement("Tr");
                    XmlNode tr2 = doc.CreateElement("Tr");
                    
                    for (int b = 10 + (a - 1) * 6; b < (a - 1) * 6 + 16; b++)
                    {
                        XmlNode td1 = doc.CreateElement("td");
                        XmlNode td2 = doc.CreateElement("td");
                       

                        td1.InnerText = dt.Columns[b].ColumnName;

                        if (dt.Rows[i][b].ToString().Trim() == "")//Trim()清除字符串中的空格
                        {     
                            td2.InnerText = "0";
                        }
                        else
                        {
                            XmlNode tdStyle = doc.CreateNode(XmlNodeType.Attribute,"style","");
                            tdStyle.Value = "color:red";
                            td1.Attributes.SetNamedItem(tdStyle);
                            td2.Attributes.SetNamedItem(tdStyle);
                            td2.InnerText = dt.Rows[i][b].ToString().Trim();
                        }
                        
                        tr1.AppendChild(td1);
                        tr2.AppendChild(td2);
                    }

                    tb.AppendChild(tr1);
                    tb.AppendChild(tr2);
                    
                }
                description.AppendChild(tb);
                //显示经纬度、海拔、速度、时间
                XmlNode p1 = doc.CreateElement("p");

                p1.InnerText = dt.Columns[4].ColumnName + ":" + dt.Rows[i][4].ToString().Trim() + ";"
                    + dt.Columns[6].ColumnName + ":" + dt.Rows[i][6].ToString().Trim();

                XmlNode p2 = doc.CreateElement("p");
                p2.InnerText = dt.Columns[5].ColumnName + ":" + dt.Rows[i][5].ToString().Trim() + ";"
                    + dt.Columns[7].ColumnName + ":" + dt.Rows[i][7].ToString().Trim();

                XmlNode p3 = doc.CreateElement("p");
                p3.InnerText = dt.Columns[8].ColumnName + ":" + dt.Rows[i][8].ToString().Trim();

                description.AppendChild(p1);
                description.AppendChild(p2);
                description.AppendChild(p3);
                

                Placemark.AppendChild(description);
                //写入点坐标
                XmlNode Point = doc.CreateElement("Point");
                XmlNode coordinates = doc.CreateElement("coordinates");
                coordinates.InnerText = sCoordinates;

                Point.AppendChild(coordinates);
                Placemark.AppendChild(Point);
                Folder.AppendChild(Placemark);
            }
            Document.AppendChild(Folder);

            //设置线的样式
            XmlNode style = doc.CreateElement("Style");
            XmlAttribute idStyle = doc.CreateAttribute("id");
            idStyle.Value = "yellowLineGreenPoly";
            style.Attributes.Append(idStyle);

            XmlNode lineStyle = doc.CreateElement("LineStyle");
            XmlElement colorLineStyle = doc.CreateElement("color");
            colorLineStyle.InnerText = "7f00ffff";
            lineStyle.AppendChild(colorLineStyle);

            XmlElement width = doc.CreateElement("width");
            width.InnerText = "4";
            lineStyle.AppendChild(width);
            style.AppendChild(lineStyle);


            XmlNode PolyStyle = doc.CreateElement("PolyStyle");
            XmlElement colorPolyStyle = doc.CreateElement("color");
            colorPolyStyle.InnerText = "7f00ff00";
            PolyStyle.AppendChild(colorPolyStyle);
            style.AppendChild(PolyStyle);

            Document.AppendChild(style);

            //生成线
            XmlNode PlaceMarkLine = doc.CreateElement("Placemark");

            XmlElement nameLine = doc.CreateElement("name");
            nameLine.InnerText = "线路追踪路径";
            PlaceMarkLine.AppendChild(nameLine);

            XmlElement visibilityLine = doc.CreateElement("visibility");
            visibilityLine.InnerText = "1";
            PlaceMarkLine.AppendChild(visibilityLine);

            XmlElement descriptionLine = doc.CreateElement("description");
            descriptionLine.InnerText = "GVG导出系统";
            PlaceMarkLine.AppendChild(descriptionLine);

            XmlElement styleUrlLine = doc.CreateElement("styleUrl");
            styleUrlLine.InnerText = "#yellowLineGreenPoly";
            PlaceMarkLine.AppendChild(styleUrlLine);

            XmlNode lineString = doc.CreateElement("LineString");

            XmlElement tessellateLine = doc.CreateElement("tessellate");
            tessellateLine.InnerText = "1";
            lineString.AppendChild(tessellateLine);

            XmlElement coordinatesLine = doc.CreateElement("coordinates");
            string sCoordinatesLine = "\n";
            for (int i = 0; i < nCount; i++)
            {

                string sLong = dt.Rows[i][4].ToString().Trim();
                string sLat = dt.Rows[i][5].ToString().Trim();
                string sCoordinates = sLong + "," + sLat + "," + dt.Rows[i][6].ToString().Trim();
                sCoordinatesLine += sCoordinates + "\n";
            }
            coordinatesLine.InnerText = sCoordinatesLine;
            lineString.AppendChild(coordinatesLine);
            PlaceMarkLine.AppendChild(lineString);


            Document.AppendChild(PlaceMarkLine);


            root.AppendChild(Document);
            doc.Save(fPath);
        }

        public static void CreateKMLToShp(DataTable dt, string fPath)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(dec);
            //创建一个根节点（一级）
            XmlNode root = doc.CreateElement("kml", "http://earth.google.com/kml/2.1");
            doc.AppendChild(root);
            //创建节点（二级）
            XmlNode Document = doc.CreateElement("Document");
            ////创建节点（三级）
            //XmlNode Folder = doc.CreateElement("Folder");
            XmlNode style = doc.CreateElement("Style");
            XmlAttribute idStyle = doc.CreateAttribute("id");
            idStyle.Value = "yellowLineGreenPoly";
            style.Attributes.Append(idStyle);

            XmlNode lineStyle = doc.CreateElement("LineStyle");
            XmlElement colorLineStyle = doc.CreateElement("color");
            colorLineStyle.InnerText = "7f00ffff";
            lineStyle.AppendChild(colorLineStyle);

            XmlElement width = doc.CreateElement("width");
            width.InnerText = "4";
            lineStyle.AppendChild(width);
            style.AppendChild(lineStyle);


            XmlNode PolyStyle = doc.CreateElement("PolyStyle");
            XmlElement colorPolyStyle = doc.CreateElement("color");
            colorPolyStyle.InnerText = "7f00ff00";
            PolyStyle.AppendChild(colorPolyStyle);
            style.AppendChild(PolyStyle);

            Document.AppendChild(style);

            int nCount = dt.Rows.Count;
            XmlNode PlaceMarkLine = doc.CreateElement("Placemark");

            XmlElement nameLine = doc.CreateElement("name");
            nameLine.InnerText = "线路追踪路径";
            PlaceMarkLine.AppendChild(nameLine);

            XmlElement visibilityLine = doc.CreateElement("visibility");
            visibilityLine.InnerText = "1";
            PlaceMarkLine.AppendChild(visibilityLine);

            XmlElement descriptionLine = doc.CreateElement("description");
            descriptionLine.InnerText = "GVG导出系统";
            PlaceMarkLine.AppendChild(descriptionLine);

            XmlElement styleUrlLine = doc.CreateElement("styleUrl");
            styleUrlLine.InnerText = "#yellowLineGreenPoly";
            PlaceMarkLine.AppendChild(styleUrlLine);

            XmlNode lineString = doc.CreateElement("LineString");

            XmlElement tessellateLine = doc.CreateElement("tessellate");
            tessellateLine.InnerText = "1";
            lineString.AppendChild(tessellateLine);

            XmlElement coordinatesLine = doc.CreateElement("coordinates");
            string sCoordinatesLine = "\n";
            for (int i = 0; i < nCount; i++)
            {

                string sLong = dt.Rows[i][4].ToString().Trim();
                string sLat = dt.Rows[i][5].ToString().Trim();
                string sCoordinates = sLong + "," + sLat + "," + dt.Rows[i][6].ToString().Trim();     
                sCoordinatesLine += sCoordinates + "\n";
            }
            coordinatesLine.InnerText = sCoordinatesLine;
            lineString.AppendChild(coordinatesLine);
            PlaceMarkLine.AppendChild(lineString);


            Document.AppendChild(PlaceMarkLine);
            root.AppendChild(Document);
            doc.Save(fPath);
        }

    }
}
