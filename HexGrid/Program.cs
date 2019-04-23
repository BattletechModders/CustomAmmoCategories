using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGrid {
  public class MapPoint {
    public int x;
    public int y;
    public static int offcet = 4096;
    public MapPoint(int X, int Y) {
      this.x = X;
      this.y = Y;
    }
    public override int GetHashCode() {
      return this.y * MapPoint.offcet + this.x;
    }
    public override bool Equals(object obj) {
      MapPoint mp = obj as MapPoint;
      if ((object)mp == null) { return false; };
      return (this.x == mp.x) && (this.y == mp.y);
    }
    public static void Swap(ref int a,ref int b) {
      a = a ^ b;
      b = a ^ b;
      a = a ^ b;
    }
    public static List<MapPoint> BresenhamLine(int x0, int y0, int x1, int y1) {
      List<MapPoint> result = new List<MapPoint>();
      var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0); // Проверяем рост отрезка по оси икс и по оси игрек
                                                         // Отражаем линию по диагонали, если угол наклона слишком большой
      if (steep) {
        Swap(ref x0, ref y0); // Перетасовка координат вынесена в отдельную функцию для красоты
        Swap(ref x1, ref y1);
      }
      // Если линия растёт не слева направо, то меняем начало и конец отрезка местами
      if (x0 > x1) {
        Swap(ref x0, ref x1);
        Swap(ref y0, ref y1);
      }
      int dx = x1 - x0;
      int dy = Math.Abs(y1 - y0);
      int error = dx / 2; // Здесь используется оптимизация с умножением на dx, чтобы избавиться от лишних дробей
      int ystep = (y0 < y1) ? 1 : -1; // Выбираем направление роста координаты y
      int y = y0;
      for (int x = x0; x <= x1; x++) {
        result.Add(new MapPoint(steep ? y : x, steep ? x : y)); // Не забываем вернуть координаты на место
        error -= dy;
        if (error < 0) {
          y += ystep;
          error += dx;
        }
      }
      return result;
    }
    public static List<MapPoint> createHexagon(int x,int y,int r) {
      List<MapPoint> result = new List<MapPoint>();
      int dx = (int)((float)r / 2f);
      int dy = (int)Math.Round((float)r * 0.86025f);
      List<MapPoint> line = BresenhamLine(x + dx, y + dy, x + r, y);
      foreach(var point in line) {
        int tdx = point.x - x;
        int tdy = point.y - y;
        for (int tx = x - tdx; tx <= point.x; ++tx) {
          result.Add(new MapPoint(tx, point.y));
          result.Add(new MapPoint(tx, y - tdy));
        }
      }
      return result;
    }
  }
  class Program {
    public static int MaxX = 79;
    public static int MaxY = 24;
    public static char[,] ServicePoint = new char[Program.MaxX, Program.MaxY];
    public static void drawArray(List<MapPoint> points,char c) {
      foreach(var point in points) {
        if (point.x < 0) { continue; }
        if (point.x >= MaxX) { continue; }
        if (point.y < 0) { continue; }
        if (point.y >= MaxY) { continue; }
        ServicePoint[point.x, point.y] = c;
      }
    }
    static void Main(string[] args) {
      Console.WriteLine();
      for (int y = 0; y < MaxY; ++y) {
        for(int x = 0; x < MaxX; ++x) {
          ServicePoint[x, y] = '-';
        }
      }
      int R = 4;
      int hexStepX = (R * 3) / 2 + 1;
      int hexStepY = (int)Math.Round((float)R * 0.866025f);
      //List<MapPoint> hexPattern = MapPoint.createHexagon(0, 0, R);

      int hex_x = MaxX / hexStepX;
      if ((MaxX % hexStepX) != 0) { ++hex_x; }
      int hex_y = MaxY / ((hexStepY  * 2) - 1);
      if ((MaxY % ((hexStepY * 2) - 1)) != 0) { ++hex_y; }
      int counter = 0;
      //DynamicMapHelper.hexGrid = new MapTerrainHexCell[hex_x, hex_y];
      for (int hix = 0; hix < hex_x; ++hix) {
        for (int hiy = 0; hiy < hex_y; ++hiy) {
          int hx = hix * hexStepX;
          int hy = hiy * hexStepY * 2 + (hx % 2) * hexStepY;
          char fill = (counter % 10).ToString().ElementAt(0);
          drawArray(MapPoint.createHexagon(hx, hy, R), fill);
          ++counter;
        }
      }

      for (int y = 0; y < MaxY; ++y) {
        for (int x = 0; x < MaxX; ++x) {
          Console.Write(ServicePoint[x,y]);
        }
        Console.WriteLine();
      }
      Console.WriteLine("HexX:"+hex_x+" HexY:"+hex_y);
    }
  }
}
