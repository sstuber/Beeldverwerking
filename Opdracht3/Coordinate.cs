namespace INFOIBV
{
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int xCoordinate, int yCoordinate)
        {
            this.x = xCoordinate;
            this.y = yCoordinate;
        }

        public static bool operator == (Coordinate cor1, Coordinate cor2)
        {
            return cor1.x == cor2.x && cor1.y == cor2.y;
        }

        public static bool operator != (Coordinate cor1, Coordinate cor2)
        {
            return cor1.x != cor2.x || cor1.y != cor2.y;
        }

        public static Coordinate operator + (Coordinate cor1, Coordinate cor2)
        {
            return new Coordinate(cor1.x + cor2.x, cor1.y + cor2.y);
        }

        public static Coordinate operator - (Coordinate cor1, Coordinate cor2)
        {
            return new Coordinate(cor1.x - cor2.x, cor1.y - cor2.y);
        }
    }
}
