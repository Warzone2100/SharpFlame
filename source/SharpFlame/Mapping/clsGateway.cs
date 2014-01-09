using SharpFlame.Collections;
using SharpFlame.Maths;

namespace SharpFlame.Mapping
{
    public class clsGateway
    {
        public clsGateway()
        {
            MapLink = new ConnectedListLink<clsGateway, clsMap>(this);
        }

        public ConnectedListLink<clsGateway, clsMap> MapLink;
        public sXY_int PosA;
        public sXY_int PosB;

        public bool IsOffMap()
        {
            sXY_int TerrainSize = MapLink.Source.Terrain.TileSize;

            return PosA.X < 0
                   | PosA.X >= TerrainSize.X
                   | PosA.Y < 0
                   | PosA.Y >= TerrainSize.Y
                   | PosB.X < 0
                   | PosB.X >= TerrainSize.X
                   | PosB.Y < 0
                   | PosB.Y >= TerrainSize.Y;
        }

        public void Deallocate()
        {
            MapLink.Deallocate();
        }
    }
}