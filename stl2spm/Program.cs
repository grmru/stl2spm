using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace tias.stl2spm
{
    public class Programm
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("stl2spm started...");

            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(string.Format("args[{0}] = {1};", i, args[i]));
            }

            if (args.Length >= 1)
            {
                string stl_file_path = args[0];
                FileInfo fi_stl_file = new FileInfo(stl_file_path);
                if (fi_stl_file.Exists)
                {
                    byte[] fileBytes = File.ReadAllBytes(stl_file_path);

                    string headerData = System.Text.Encoding.UTF8.GetString(getHeaderBytes(fileBytes));
                    Console.WriteLine(string.Format("headerData={0}", headerData));

                    int triangeCount = getTrianglesCount(fileBytes);
                    Console.WriteLine(string.Format("triangeCount={0}", triangeCount));

                    List<Triangle> triangles = getTriangles(fileBytes, triangeCount);
                    Console.WriteLine(string.Format("triangles.Count={0}", triangles.Count));

                    createSPMfile(triangles, "output.spm");
                }
                else
                {
                    Console.WriteLine(string.Format("File {0} does not exists!", stl_file_path));
                }
            }

            Console.WriteLine("stl2spm done!");
        }

        public static byte[] getHeaderBytes(byte[] inputBytes)
        {
            byte[] outBytes = new byte[80];
            for (int i = 0; i < 80; i++)
            {
                outBytes[i] = inputBytes[i];
            }
            return outBytes;
        }

        public static int getTrianglesCount(byte[] inputBytes)
        {
            int ret = 0;
            ret = BitConverter.ToInt32(inputBytes, 80);
            return ret;
        }

        public static List<Triangle> getTriangles(byte[] inputBytes, int triangleCount)
        {
            List<Triangle> ret_list = new List<Triangle>();

            int oneRecordInBytes = 50;
            int byteStart = 84;
            for (int i = 0; i < triangleCount; i++)
            {
                int sByte = byteStart + (i * oneRecordInBytes);

                Triangle tr = new Triangle();
                tr.Normal.X = BitConverter.ToSingle(inputBytes, sByte);
                tr.Normal.Y = BitConverter.ToSingle(inputBytes, sByte + 4);
                tr.Normal.Z = BitConverter.ToSingle(inputBytes, sByte + 8);

                tr.Point1.X = BitConverter.ToSingle(inputBytes, sByte + 12);
                tr.Point1.Y = BitConverter.ToSingle(inputBytes, sByte + 16);
                tr.Point1.Z = BitConverter.ToSingle(inputBytes, sByte + 20);

                tr.Point2.X = BitConverter.ToSingle(inputBytes, sByte + 24);
                tr.Point2.Y = BitConverter.ToSingle(inputBytes, sByte + 28);
                tr.Point2.Z = BitConverter.ToSingle(inputBytes, sByte + 32);

                tr.Point3.X = BitConverter.ToSingle(inputBytes, sByte + 36);
                tr.Point3.Y = BitConverter.ToSingle(inputBytes, sByte + 40);
                tr.Point3.Z = BitConverter.ToSingle(inputBytes, sByte + 44);

                tr.Attr = (short)BitConverter.ToUInt16(inputBytes, sByte + 48);

                ret_list.Add(tr);
            }

            return ret_list;
        }

        public static void createSPMfile(List<Triangle> triangles, string filename)
        {
            Dictionary<string, spmPoint> points = new Dictionary<string, spmPoint>();
            int spmPointIndex = 1;
            for (int i = 0; i < triangles.Count; i++)
            {
                string key = string.Format("{0}|{1}|{2}",
                                           triangles[i].Point1.X.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point1.Y.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point1.Z.ToString("0.######").Replace(',', '.'));

                if (!points.ContainsKey(key))
                {
                    spmPoint p = new spmPoint();
                    p.X = triangles[i].Point1.X;
                    p.Y = triangles[i].Point1.Y;
                    p.Z = triangles[i].Point1.Z;
                    p.index = spmPointIndex;
                    points.Add(key, p);

                    triangles[i].spmPoint1Index = spmPointIndex;

                    spmPointIndex++;
                }
                else
                {
                    triangles[i].spmPoint1Index = points[key].index;
                }



                string key2 = string.Format("{0}|{1}|{2}",
                                           triangles[i].Point2.X.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point2.Y.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point2.Z.ToString("0.######").Replace(',', '.'));

                if (!points.ContainsKey(key2))
                {
                    spmPoint p = new spmPoint();
                    p.X = triangles[i].Point2.X;
                    p.Y = triangles[i].Point2.Y;
                    p.Z = triangles[i].Point2.Z;
                    p.index = spmPointIndex;
                    points.Add(key2, p);

                    triangles[i].spmPoint2Index = spmPointIndex;

                    spmPointIndex++;
                }
                else
                {
                    triangles[i].spmPoint2Index = points[key2].index;
                }


                string key3 = string.Format("{0}|{1}|{2}",
                                           triangles[i].Point3.X.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point3.Y.ToString("0.######").Replace(',', '.'),
                                           triangles[i].Point3.Z.ToString("0.######").Replace(',', '.'));

                if (!points.ContainsKey(key3))
                {
                    spmPoint p = new spmPoint();
                    p.X = triangles[i].Point3.X;
                    p.Y = triangles[i].Point3.Y;
                    p.Z = triangles[i].Point3.Z;
                    p.index = spmPointIndex;
                    points.Add(key3, p);

                    triangles[i].spmPoint3Index = spmPointIndex;

                    spmPointIndex++;
                }
                else
                {
                    triangles[i].spmPoint3Index = points[key3].index;
                }
            }

            string[] keys = new string[points.Keys.Count];
            points.Keys.CopyTo(keys, 0);

            StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding(1251));
            sw.WriteLine("+ОБЪЕКТ;");
            sw.WriteLine(string.Format("КОД={0}; ИМЯ={1}; ТИП=3;", filename.TrimEnd('.', 's', 'p', 'm'), "Твердое тело"));
            sw.WriteLine("");
            sw.WriteLine("+ПАРАМЕТРЫ;");

            //int point_count = 1;
            //for (int i = 0; i < triangles.Count; i++)
            //{
            //    sw.WriteLine(string.Format("x{0}={1}; y{0}={2}; z{0}={3};", point_count, 
            //                                                                (triangles[i].Point1.X.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point1.Y.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point1.Z.ToString("0.######"))).Replace(',', '.'));
            //    point_count++;
            //    sw.WriteLine(string.Format("x{0}={1}; y{0}={2}; z{0}={3};", point_count, 
            //                                                                (triangles[i].Point2.X.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point2.Y.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point2.Z.ToString("0.######"))).Replace(',', '.'));                
            //    point_count++;
            //    sw.WriteLine(string.Format("x{0}={1}; y{0}={2}; z{0}={3};", point_count, 
            //                                                                (triangles[i].Point3.X.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point3.Y.ToString("0.######")).Replace(',', '.'), 
            //                                                                (triangles[i].Point3.Z.ToString("0.######"))).Replace(',', '.'));
            //    point_count++;
            //}
            //sw.WriteLine(string.Format("N={0};", point_count - 1));

            for (int i = 0; i < keys.Length; i++)
            {
                sw.WriteLine(string.Format("x{0}={1}; y{0}={2}; z{0}={3};", points[keys[i]].index,
                                                                            (points[keys[i]].X.ToString("0.######")).Replace(',', '.'),
                                                                            (points[keys[i]].Y.ToString("0.######")).Replace(',', '.'),
                                                                            (points[keys[i]].Z.ToString("0.######"))).Replace(',', '.'));
            }
            sw.WriteLine(string.Format("N={0};", keys.Length));

            int top_count = 1;
            for (int i = 0; i < triangles.Count; i++)
            {
                string top_line = string.Empty;
                top_line += string.Format("top{0}={1}; ", top_count, 3);
                top_count++;

                //float nx = ((y1-y2)*(z1-z3))-((z1-z2)*(y1-y3));
                //float ny = ((z1-z2)*(x1-x3))-((x1-x2)*(z1-z3));
                //float nz = ((x1-x2)*(y1-y3))-((y1-y2)*(x1-x3));

                float nx = ((triangles[i].Point1.Y - triangles[i].Point2.Y) * (triangles[i].Point1.Z - triangles[i].Point3.Z)) - ((triangles[i].Point1.Z - triangles[i].Point2.Z) * (triangles[i].Point1.Y - triangles[i].Point3.Y));
                float ny = ((triangles[i].Point1.Z - triangles[i].Point2.Z) * (triangles[i].Point1.X - triangles[i].Point3.X)) - ((triangles[i].Point1.X - triangles[i].Point2.X) * (triangles[i].Point1.Z - triangles[i].Point3.Z));
                float nz = ((triangles[i].Point1.X - triangles[i].Point2.X) * (triangles[i].Point1.Y - triangles[i].Point3.Y)) - ((triangles[i].Point1.Y - triangles[i].Point2.Y) * (triangles[i].Point1.X - triangles[i].Point3.X));

                float length = (float)System.Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                nx = nx / length;
                ny = ny / length;
                nz = nz / length;

                if (Math.Round(triangles[i].Normal.X, 1) == Math.Round(nx, 1) &&
                    Math.Round(triangles[i].Normal.Y, 1) == Math.Round(ny, 1) &&
                    Math.Round(triangles[i].Normal.Z, 1) == Math.Round(nz, 1))
                {
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint3Index);
                    top_count++;
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint2Index);
                    top_count++;
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint1Index);
                    top_count++;
                }
                else
                {
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint1Index);
                    top_count++;
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint2Index);
                    top_count++;
                    top_line += string.Format("top{0}={1}; ", top_count, triangles[i].spmPoint3Index);
                    top_count++;
                }

                sw.WriteLine(top_line);
            }
            sw.WriteLine(string.Format("Ntop={0};", top_count - 1));

            sw.Close();
        }

        public class Triangle
        {
            public Vertex Normal;
            public Vertex Point1;
            public Vertex Point2;
            public Vertex Point3;
            public short Attr;

            public int spmPoint1Index;
            public int spmPoint2Index;
            public int spmPoint3Index;

            public Triangle()
            {
                this.Normal = new Vertex();
                this.Point1 = new Vertex();
                this.Point2 = new Vertex();
                this.Point3 = new Vertex();
                this.Attr = 0;
                this.spmPoint1Index = 0;
                this.spmPoint2Index = 0;
                this.spmPoint3Index = 0;
            }
        }

        public class Vertex
        {
            public float X;
            public float Y;
            public float Z;

            public Vertex()
            {
                this.X = 0;
                this.Y = 0;
                this.Z = 0;
            }
        }

        public class spmPoint
        {
            public int index;
            public float X;
            public float Y;
            public float Z;

            public spmPoint()
            {
                this.index = 0;
                this.X = 0;
                this.Y = 0;
                this.Z = 0;
            }
        }
    }
}