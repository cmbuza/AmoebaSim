using Microsoft.Xna.Framework;
using System;

namespace AmoebaSim.Desktop
{


    public struct Line
    {
        public Point Start;
        public Point End;
        public Color Color;
    }


    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ZiggyVector
    {

        #region DrawLine - Bresenham
        public static void DrawLine(Color[] buffer, int bufferWidth, int bufferHeight, Line line)
        {
            DrawLine(buffer, bufferWidth, bufferHeight, line.Start, line.End, line.Color);
        }
        #endregion
        public unsafe static void DrawLineHorizontal(Color[] buffer,
                      int bufferWidth,
                      int bufferHeight,
                      int startX,
                      int startY,
                      int endX,
                      Color color)
        {
            //test if the line is fully within the screen bounds
            bool safe = false;

            if (startX >= 0 && startX < bufferWidth &&
                 startY >= 0 && startY < bufferHeight &&
                endX >= 0 && endX < bufferWidth)
            {
                safe = true;
            }

            if (startX > endX)
            {
                int i = startX;
                startX = endX;
                endX = i;
            }
            fixed (Color* cp = buffer)
            {
                if (safe)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        cp[x + startY * bufferWidth] = color;
                    }
                }
                else
                {
                    for (int x = startX; x < endX; x++)
                    {
                        if (x >= 0 && x < bufferWidth && startY >= 0 && startY < bufferHeight)
                            cp[x + startY * bufferWidth] = color;
                    }
                }
            }
        }
        public unsafe static void DrawLine(Color[] buffer,
                      int bufferWidth,
                      int bufferHeight,
                      int startX,
                      int startY,
                      int endX,
                      int endY,
                      Color color)
        {
            //from pseudocode on http://en.wikipedia.org/wiki/Bresenham's_line_algorithm



            //boolean steep := abs(y1 - y0) > abs(x1 - x0)
            bool steep = Math.Abs(endY - startY) > Math.Abs(endX - startX);


            //test if the line is fully within the screen bounds
            bool safe = false;

            if (startX >= 0 && startX < bufferWidth &&
                 startY >= 0 && startY < bufferHeight &&
                endX >= 0 && endX < bufferWidth &&
                 endY >= 0 && endY < bufferHeight)
            {
                safe = true;
            }

            //if steep then
            //  swap(x0, y0)
            //  swap(x1, y1)
            if (steep)
            {
                int temp = startX;
                startX = startY;
                startY = temp;

                temp = endX;
                endX = endY;
                endY = temp;
            }

            //if x0 > x1 then
            //  swap(x0, x1)
            //  swap(y0, y1)
            if (startX > endX)
            {
                int temp = startX;
                startX = endX;
                endX = temp;

                temp = startY;
                startY = endY;
                endY = temp;
            }


            //int deltax := x1 - x0
            //int deltay := abs(y1 - y0)
            //real error := 0
            //real deltaerr := deltay / deltax
            //int ystep
            //int y := y0
            //if y0 < y1 then ystep := 1 else ystep := -1

            int deltaX = endX - startX;
            int deltaY = Math.Abs(endY - startY);

            int error = -(deltaX + 1) / 2;
            //float deltaErr = deltaY / deltaX;

            int y = startY;
            int yStep;

            if (startY < endY)
            {
                yStep = 1;
            }
            else
            {
                yStep = -1;
            }

            fixed (Color* cp = buffer)
            {
                //for x from x0 to x1
                //  if steep then plot(y,x) else plot(x,y)
                //  error := error + deltaerr
                //  if error > 0.5 then
                //     y := y + ystep
                //     error := error - 1.0
                if (safe)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        if (steep)
                            cp[y + x * bufferWidth] = color;
                        else
                            cp[x + y * bufferWidth] = color;

                        error += deltaY;
                        if (error > 0)
                        {
                            y += yStep;
                            error -= deltaX;
                        }
                    }
                }
                else
                {
                    for (int x = startX; x < endX; x++)
                    {
                        if (steep)
                        {
                            if (x >= 0 && x < bufferHeight && y >= 0 && y < bufferWidth)
                                cp[y + x * bufferWidth] = color;
                        }
                        else
                        {
                            if (x >= 0 && x < bufferWidth && y >= 0 && y < bufferHeight)
                                cp[x + y * bufferWidth] = color;
                        }
                        error += deltaY;
                        if (error > 0)
                        {
                            y += yStep;
                            error -= deltaX;
                        }
                    }
                }
            }
        }

        #region DrawLine - Bresenham
        public unsafe static void DrawLine(Color[] buffer,
                      int bufferWidth,
                      int bufferHeight,
                      Point start,
                      Point end,
                      Color color)
        {
            DrawLine(buffer, bufferWidth, bufferHeight, start.X, start.Y, end.X, end.Y, color);
        }
        #endregion

        #region DrawCircle
        public unsafe static void DrawCircle(Color[] buffer,
                                             int bufferWidth,
                                             int bufferHeight,
                                             int centerX,
                                             int centerY,
                                             int radius,
                                             Color color)
        {
            //http://en.wikipedia.org/wiki/Midpoint_circle_algorithm
            int f = 1 - radius;
            int ddF_x = 0;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            //test if sphere is fully within the screen bounds
            bool safe = false;

            if (centerX - radius >= 0 &&
                centerX + radius < bufferWidth &&
                centerY - radius >= 0 &&
                centerY + radius < bufferHeight)
            {
                safe = true;
            }

            fixed (Color* cp = buffer)
            {

                if (safe)
                {
                    int centerYPlusRad = centerY + radius;
                    int centerYMinusRad = centerY - radius;

                    //setPixel(x0, y0 + radius);
                    cp[centerX + centerYPlusRad * bufferWidth] = color;


                    //setPixel(x0, y0 - radius);
                    cp[centerX + centerYMinusRad * bufferWidth] = color;

                    int centerXPlusRad = centerX + radius;
                    int centerXMinusRad = centerX - radius;

                    //setPixel(x0 + radius, y0);
                    cp[centerXPlusRad + centerY * bufferWidth] = color;


                    //setPixel(x0 - radius, y0);
                    cp[centerXMinusRad + centerY * bufferWidth] = color;


                    while (x < y)
                    {
                        if (f >= 0)
                        {
                            y--;
                            ddF_y += 2;
                            f += ddF_y;
                        }
                        x++;
                        ddF_x += 2;
                        f += ddF_x + 1;

                        int centerXPlusX = centerX + x;
                        int centerXMinusX = centerX - x;
                        int centerXPlusY = centerX + y;
                        int centerXMinusY = centerX - y;


                        int centerYPlusY = centerY + y;
                        int centerYMinusY = centerY - y;
                        int centerYPlusX = centerY + x;
                        int centerYMinusX = centerY - x;


                        //setPixel(x0 + x, y0 + y);
                        cp[centerXPlusX + centerYPlusY * bufferWidth] = color;
                        //setPixel(x0 - x, y0 + y);
                        cp[centerXMinusX + centerYPlusY * bufferWidth] = color;
                        //setPixel(x0 + x, y0 - y);
                        cp[centerXPlusX + centerYMinusY * bufferWidth] = color;
                        //setPixel(x0 - x, y0 - y);
                        cp[centerXMinusX + centerYMinusY * bufferWidth] = color;
                        //setPixel(x0 + y, y0 + x);
                        cp[centerXPlusY + centerYPlusX * bufferWidth] = color;
                        //setPixel(x0 - y, y0 + x);
                        cp[centerXMinusY + centerYPlusX * bufferWidth] = color;
                        //setPixel(x0 + y, y0 - x);
                        cp[centerXPlusY + centerYMinusX * bufferWidth] = color;
                        //setPixel(x0 - y, y0 - x);
                        cp[centerXMinusY + centerYMinusX * bufferWidth] = color;
                    }
                }
                else
                {
                    int centerYPlusRad = centerY + radius;
                    int centerYMinusRad = centerY - radius;
                    //setPixel(x0, y0 + radius);
                    if (centerX >= 0 &&
                        centerX < bufferWidth &&
                        centerYPlusRad >= 0 &&
                        centerYPlusRad < bufferHeight)
                    {
                        cp[centerX + centerYPlusRad * bufferWidth] = color;
                    }

                    //setPixel(x0, y0 - radius);
                    if (centerX >= 0 &&
                        centerX < bufferWidth &&
                        centerYMinusRad >= 0 &&
                        centerYMinusRad < bufferHeight)
                    {
                        cp[centerX + centerYMinusRad * bufferWidth] = color;
                    }

                    int centerXPlusRad = centerX + radius;
                    int centerXMinusRad = centerX - radius;

                    //setPixel(x0 + radius, y0);
                    if (centerXPlusRad >= 0 &&
                        centerXPlusRad < bufferWidth &&
                        centerY >= 0 &&
                        centerY < bufferHeight)
                    {
                        cp[centerXPlusRad + centerY * bufferWidth] = color;
                    }

                    //setPixel(x0 - radius, y0);
                    if (centerXMinusRad >= 0 &&
                        centerXMinusRad < bufferWidth &&
                        centerY >= 0 &&
                        centerY < bufferHeight)
                    {
                        cp[centerXMinusRad + centerY * bufferWidth] = color;
                    }

                    while (x < y)
                    {
                        if (f >= 0)
                        {
                            y--;
                            ddF_y += 2;
                            f += ddF_y;
                        }
                        x++;
                        ddF_x += 2;
                        f += ddF_x + 1;

                        int centerXPlusX = centerX + x;
                        int centerXMinusX = centerX - x;
                        int centerXPlusY = centerX + y;
                        int centerXMinusY = centerX - y;


                        int centerYPlusY = centerY + y;
                        int centerYMinusY = centerY - y;
                        int centerYPlusX = centerY + x;
                        int centerYMinusX = centerY - x;


                        //setPixel(x0 + x, y0 + y);
                        if (centerXPlusX >= 0 && centerXPlusX < bufferWidth &&
                            centerYPlusY >= 0 && centerYPlusY < bufferHeight)
                            cp[centerXPlusX + centerYPlusY * bufferWidth] = color;
                        //setPixel(x0 - x, y0 + y);
                        if (centerXMinusX >= 0 && centerXMinusX < bufferWidth &&
                            centerYPlusY >= 0 && centerYPlusY < bufferHeight)
                            cp[centerXMinusX + centerYPlusY * bufferWidth] = color;
                        //setPixel(x0 + x, y0 - y);
                        if (centerXPlusX >= 0 && centerXPlusX < bufferWidth &&
                            centerYMinusY >= 0 && centerYMinusY < bufferHeight)
                            cp[centerXPlusX + centerYMinusY * bufferWidth] = color;
                        //setPixel(x0 - x, y0 - y);
                        if (centerXMinusX >= 0 && centerXMinusX < bufferWidth &&
                            centerYMinusY >= 0 && centerYMinusY < bufferHeight)
                            cp[centerXMinusX + centerYMinusY * bufferWidth] = color;
                        //setPixel(x0 + y, y0 + x);
                        if (centerXPlusY >= 0 && centerXPlusY < bufferWidth &&
                            centerYPlusX >= 0 && centerYPlusX < bufferHeight)
                            cp[centerXPlusY + centerYPlusX * bufferWidth] = color;
                        //setPixel(x0 - y, y0 + x);
                        if (centerXMinusY >= 0 && centerXMinusY < bufferWidth &&
                            centerYPlusX >= 0 && centerYPlusX < bufferHeight)
                            cp[centerXMinusY + centerYPlusX * bufferWidth] = color;
                        //setPixel(x0 + y, y0 - x);
                        if (centerXPlusY >= 0 && centerXPlusY < bufferWidth &&
                            centerYMinusX >= 0 && centerYMinusX < bufferHeight)
                            cp[centerXPlusY + centerYMinusX * bufferWidth] = color;
                        //setPixel(x0 - y, y0 - x);
                        if (centerXMinusY >= 0 && centerXMinusY < bufferWidth &&
                            centerYMinusX >= 0 && centerYMinusX < bufferHeight)
                            cp[centerXMinusY + centerYMinusX * bufferWidth] = color;
                    }
                }
            }
        }

        #endregion

        public unsafe static void DrawRectFilled(Color[] buffer,
                                                 int bufferWidth,
                                                 int bufferHeight,
                                                 Rectangle rect,
                                                 Color color)
        {
            DrawRectFilled(buffer, bufferWidth, bufferHeight, rect.X, rect.Y, rect.Right, rect.Bottom, color);
        }

        public unsafe static void DrawEllipse(Color[] buffer,
                                              int bufferWidth,
                                              int bufferHeight,
                                              int centerX,
                                              int centerY,
                                              float radiusX,
                                              float radiusY,
                                              Color color)
        {

            /*http://vgfort.com/cpp_algorithms.php
             * //Ellipse Generating Algorithm
               //Simple Cartesian-Coordinate
             * void ellipse(int xc, int yc, int xr, int yr)
                {
                int len;
                   if (xr > yr)
                   {
                   int y12, y22;
                   int y11 = yc, y21 = yc;
                      for (int x=xc-xr+1; x<=xc+xr; ++x)
                      {
                         len = yr * sqrt(1 - SQR((x - xc) / double(xr)));
                         y12 = yc + len;
                         y22 = yc - len;
                         line(x-1, y11, x, y12);
                         line(x-1, y21, x, y22);
                         y11 = y12;
                         y21 = y22;
                      }
                   }
                   else
                   {
                   int x12, x22;
                   int x11 = xc, x21 = xc;
                      for (int y=yc-yr+1; y<=yc+yr; ++y)
                      {
                         len = xr * sqrt(1 - SQR((y - yc) / double(yr)));
                         x12 = xc + len;
                         x22 = xc - len;
                         line(x11, y-1, x12, y);
                         line(x21, y-1, x22, y);
                         x11 = x12;
                         x21 = x22;
                      }
                   }
                }
             * */

            int len = 0;
            if (radiusX > radiusY)
            {
                int y12, y22;
                int y11 = centerY, y21 = centerY;
                for (int x = centerX - (int)Math.Round(radiusX) + 1; x <= centerX + radiusX; ++x)
                {
                    len = (int)(radiusY * Math.Sqrt(1 - Math.Pow((x - centerX) / (double)radiusX, 2)));
                    y12 = centerY + len;
                    y22 = centerY - len;
                    DrawLine(buffer, bufferWidth, bufferHeight, x - 1, y11, x, y12, color);
                    DrawLine(buffer, bufferWidth, bufferHeight, x - 1, y21, x, y22, color);
                    y11 = y12;
                    y21 = y22;
                }
            }
            else
            {
                int x12, x22;
                int x11 = centerX, x21 = centerX;
                for (int y = centerY - (int)Math.Round(radiusY) + 1; y <= centerY + radiusY; ++y)
                {
                    len = (int)(radiusX * Math.Sqrt(1 - Math.Pow((y - centerY) / (double)radiusY, 2)));
                    x12 = centerX + len;
                    x22 = centerX - len;
                    DrawLine(buffer, bufferWidth, bufferHeight, x11, y - 1, x12, y, color);
                    DrawLine(buffer, bufferWidth, bufferHeight, x21, y - 1, x22, y, color);
                    x11 = x12;
                    x21 = x22;
                }
            }
        }

        static float min(float x1, float x2, float x3)
        {
            return x1 < x2 && x1 < x3 ? x1 :
                x2 < x1 && x2 < x3 ? x2 :
                x3;
        }

        static float max(float x1, float x2, float x3)
        {
            return x1 > x2 && x1 > x3 ? x1 :
                x2 > x1 && x2 > x3 ? x2 :
                x3;
        }

        public unsafe static void DrawFilledTriangle(Color[] buffer,
                                                 int bufferWidth,
                                                 int bufferHeight,
                                                 Point v1,
                                                 Point v2,
                                                 Point v3,
                                                 Color color)
        {
            //2d triangle area
            if ((v2.X - v1.X) * (v3.Y - v1.Y) - (v3.X - v1.X) * (v2.Y - v1.Y) < 0)
                DrawFilledTriangleCW(buffer, bufferWidth, bufferHeight, v1, v3, v2, color);
            else
                DrawFilledTriangleCCW(buffer, bufferWidth, bufferHeight, v1, v3, v2, color);
        }

        public unsafe static void DrawFilledTriangleCW(Color[] buffer,
                                                 int bufferWidth,
                                                 int bufferHeight,
                                                 Point v1,
                                                 Point v2,
                                                 Point v3,
                                                 Color color)
        {
            DrawFilledTriangleCCW(buffer, bufferWidth, bufferHeight, v1, v3, v2, color);
        }

        public unsafe static void DrawFilledTriangleCCW(Color[] buffer,
                                                 int bufferWidth,
                                                 int bufferHeight,
                                                 Point v1,
                                                 Point v2,
                                                 Point v3,
                                                 Color color)
        {
            //http://www.devmaster.net/forums/showthread.php?t=1884
            float y1 = v1.Y;
            float y2 = v2.Y;
            float y3 = v3.Y;

            float x1 = v1.X;
            float x2 = v2.X;
            float x3 = v3.X;

            // Bounding rectangle
            int minx = (int)min(x1, x2, x3);
            int maxx = (int)max(x1, x2, x3);
            int miny = (int)min(y1, y2, y3);
            int maxy = (int)max(y1, y2, y3);
            fixed (Color* cp = buffer)
            {
                //(char*&)colorBuffer += miny * stride;
                // Scan through bounding rectangle
                for (int y = miny; y < maxy; y++)
                {
                    for (int x = minx; x < maxx; x++)
                    {
                        // When all half-space functions positive, pixel is in triangle
                        if ((x1 - x2) * (y - y1) - (y1 - y2) * (x - x1) > 0 &&
                            (x2 - x3) * (y - y2) - (y2 - y3) * (x - x2) > 0 &&
                            (x3 - x1) * (y - y3) - (y3 - y1) * (x - x3) > 0)
                        {
                            cp[x + y * bufferWidth] = color; // White
                        }
                    }
                    //(char*&)colorBuffer += stride;
                }
            }
        }

        public unsafe static void DrawRectFilled(Color[] buffer,
                                                 int bufferWidth,
                                                 int bufferHeight,
                                                 int x1,
                                                 int y1,
                                                 int x2,
                                                 int y2,
                                                 Color color)
        {
            /*
            http://vgfort.com/cpp_algorithms.php
            
            //Scan-Line Rectangle Fill Algorithm
            void rectangleFill(int x1, int y1, int x2, int y2)
            {
               for (int y=MIN(y1,y2); y<=MAX(y1,y2); ++y)
                  line(x1, y, x2, y);
            }
            */
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); ++y)
                DrawLineHorizontal(buffer, bufferWidth, bufferHeight, x1, y, x2, color);
        }

        public unsafe static void DrawCircleFilled(Color[] buffer,
                                             int bufferWidth,
                                             int bufferHeight,
                                             int centerX,
                                             int centerY,
                                             int radius,
                                             Color color)
        {
            /*
             http://vgfort.com/cpp_algorithms.php
         
            //Scan-Line Circle Fill Algorithm
            //Utilizing Cartesian-Coordinate
            void circleFill(int xc, int yc, int r)
            {
                int x1, x2;
                for (int y = yc - r; y <= yc + r; ++y)
                {
                    x1 = ROUND(xc + sqrt(SQR(r) - SQR(y - yc)));
                    x2 = ROUND(xc - sqrt(SQR(r) - SQR(y - yc)));
                    line(x1, y, x2, y);
                }
            }*/

            int x1, x2;
            for (int y = centerY - radius; y <= centerY + radius; ++y)
            {
                x1 = (int)Math.Round((double)centerX +
                        (int)Math.Sqrt(radius * radius -
                                       (y - centerY) *
                                       (y - centerY)));

                x2 = (int)Math.Round((double)centerX -
                        (int)Math.Sqrt(radius * radius -
                                       (y - centerY) *
                                       (y - centerY)));

                DrawLineHorizontal(buffer, bufferWidth, bufferHeight, x1, y, x2, color);
            }
        }

    }
}