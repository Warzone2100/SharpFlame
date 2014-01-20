using System;
using System.Drawing;
using System.IO;
using OpenTK.Graphics.OpenGL;
using SharpFlame.AppSettings;
using SharpFlame.Bitmaps;
using SharpFlame.Colors;
using SharpFlame.FileIO;
using SharpFlame.Util;

namespace SharpFlame.Mapping.Tiles
{
    public class clsTileset
    {
        public bool IsOriginal { get; set; }

        public sTile[] Tiles { get; set; }

        public int TileCount { get; set; }

        public string Name { get; set; }

        public struct sTile
        {
            public int MapViewGlTextureNum;
            public int TextureViewGlTextureNum;
            public sRGB_sng AverageColour;
            public byte DefaultType;
        }

        public sRGB_sng BGColour = new sRGB_sng(0.5F, 0.5F, 0.5F);

        public sResult LoadTileType(string path)
        {
            sResult returnResult = new sResult();
            BinaryReader file;

            try
            {
                file = new BinaryReader(new FileStream(path, FileMode.Open));
            }
            catch ( Exception ex )
            {
                returnResult.Problem = ex.Message;
                return returnResult;
            }
            returnResult = ReadTileType(file);
            file.Close();
            return returnResult;
        }

        private sResult ReadTileType(BinaryReader file)
        {
            sResult returnResult = new sResult();
            returnResult.Success = false;
            returnResult.Problem = "";

            UInt32 uintTemp = 0;
            int i = 0;
            UInt16 ushortTemp = 0;
            string strTemp = "";

            try
            {
                strTemp = IOUtil.ReadOldTextOfLength(file, 4);
                if ( strTemp != "ttyp" )
                {
                    returnResult.Problem = "Bad identifier.";
                    return returnResult;
                }

                uintTemp = file.ReadUInt32();
                if ( !(uintTemp == 8U) )
                {
                    returnResult.Problem = "Unknown version.";
                    return returnResult;
                }

                uintTemp = file.ReadUInt32();
                TileCount = Convert.ToInt32(uintTemp);
                Tiles = new sTile[TileCount];

                for ( i = 0; i <= Math.Min(Convert.ToInt32(uintTemp), TileCount) - 1; i++ )
                {
                    ushortTemp = file.ReadUInt16();
                    if ( ushortTemp > App.TileTypes.Count )
                    {
                        returnResult.Problem = "Unknown tile type.";
                        return returnResult;
                    }
                    Tiles[i].DefaultType = (byte)ushortTemp;
                }
            }
            catch ( Exception ex )
            {
                returnResult.Problem = ex.Message;
                return returnResult;
            }

            returnResult.Success = true;
            return returnResult;
        }

        public clsResult LoadDirectory(string path)
        {
            var returnResult = new clsResult( "Loading tileset from '{0}'".Format2( path ) );

            Bitmap bitmap = null;
            sSplitPath SplitPath = new sSplitPath(path);
            string slashPath = PathUtil.EndWithPathSeperator(path);
            sResult result = new sResult();

            if ( SplitPath.FileTitle != "" )
            {
                Name = SplitPath.FileTitle;
            }
            else if ( SplitPath.PartCount >= 2 )
            {
                Name = SplitPath.Parts[SplitPath.PartCount - 2];
            }
            
            var ttpFileName = Path.ChangeExtension(Name, ".ttp");

            result = LoadTileType(Path.Combine(slashPath, ttpFileName));

            if ( !result.Success )
            {
                returnResult.ProblemAdd("Loading tile types: " + result.Problem);
                return returnResult;
            }

            int redTotal = 0;
            int greenTotal = 0;
            int blueTotal = 0;
            int tileNum = 0;
            string strTile = "";
            BitmapGLTexture bmpTextureArgs = new BitmapGLTexture();
            float[] AverageColour = new float[4];
            int x = 0;
            int y = 0;
            Color Pixel = new Color();

            string graphicPath = "";

            //tile count has been set by the ttp file

            for ( tileNum = 0; tileNum <= TileCount - 1; tileNum++ )
            {
                strTile = "tile-" + App.MinDigits(tileNum, 2) + ".png";

                //-------- 128 --------

                var tileDir = System.IO.Path.Combine(Name + "-128", strTile);
                graphicPath = System.IO.Path.Combine(slashPath, tileDir);


                result = BitmapUtil.LoadBitmap(graphicPath, ref bitmap);
                if ( !result.Success )
                {
                    //ignore and exit, since not all tile types have a corresponding tile graphic
                    return returnResult;
                }

                if ( bitmap.Width != 128 | bitmap.Height != 128 )
                {
                    returnResult.WarningAdd("Tile graphic " + graphicPath + " from tileset " + Name + " is not 128x128.");
                    return returnResult;
                }

                bmpTextureArgs.Texture = bitmap;
                bmpTextureArgs.MipMapLevel = 0;
                bmpTextureArgs.MagFilter = TextureMagFilter.Nearest;
                bmpTextureArgs.MinFilter = TextureMinFilter.Nearest;
                bmpTextureArgs.TextureNum = 0;
                bmpTextureArgs.Perform();
                Tiles[tileNum].TextureViewGlTextureNum = bmpTextureArgs.TextureNum;

                bmpTextureArgs.MagFilter = TextureMagFilter.Nearest;
                if ( SettingsManager.Settings.Mipmaps )
                {
                    bmpTextureArgs.MinFilter = TextureMinFilter.LinearMipmapLinear;
                }
                else
                {
                    bmpTextureArgs.MinFilter = TextureMinFilter.Nearest;
                }
                bmpTextureArgs.TextureNum = 0;

                bmpTextureArgs.Perform();
                Tiles[tileNum].MapViewGlTextureNum = bmpTextureArgs.TextureNum;

                if ( SettingsManager.Settings.Mipmaps )
                {
                    if ( SettingsManager.Settings.MipmapsHardware )
                    {
                        GL.Enable(EnableCap.Texture2D);
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                        GL.Disable(EnableCap.Texture2D);
                    }
                    else
                    {
                        clsResult MipmapResult = default(clsResult);
                        MipmapResult = GenerateMipMaps(slashPath, strTile, bmpTextureArgs, tileNum);
                        returnResult.Add(MipmapResult);
                        if ( MipmapResult.HasProblems )
                        {
                            return returnResult;
                        }
                    }
                    GL.GetTexImage<Single>(TextureTarget.Texture2D, 7, PixelFormat.Rgba, PixelType.Float, AverageColour);
                    Tiles[tileNum].AverageColour.Red = AverageColour[0];
                    Tiles[tileNum].AverageColour.Green = AverageColour[1];
                    Tiles[tileNum].AverageColour.Blue = AverageColour[2];
                }
                else
                {
                    redTotal = 0;
                    greenTotal = 0;
                    blueTotal = 0;
                    for ( y = 0; y <= bitmap.Height - 1; y++ )
                    {
                        for ( x = 0; x <= bitmap.Width - 1; x++ )
                        {
                            Pixel = bitmap.GetPixel(x, y);
                            redTotal += Pixel.R;
                            greenTotal += Pixel.G;
                            blueTotal += Pixel.B;
                        }
                    }
                    Tiles[tileNum].AverageColour.Red = (float)(redTotal / 4177920.0D);
                    Tiles[tileNum].AverageColour.Green = (float)(greenTotal / 4177920.0D);
                    Tiles[tileNum].AverageColour.Blue = (float)(blueTotal / 4177920.0D);
                }
            }

            return returnResult;
        }

        public clsResult GenerateMipMaps(string SlashPath, string strTile, BitmapGLTexture BitmapTextureArgs, int TileNum)
        {
            clsResult ReturnResult = new clsResult("Generating mipmaps");
            string GraphicPath = "";
            int PixX = 0;
            int PixY = 0;
            Color PixelColorA = new Color();
            Color PixelColorB = new Color();
            Color PixelColorC = new Color();
            Color PixelColorD = new Color();
            int X1 = 0;
            int Y1 = 0;
            int X2 = 0;
            int Y2 = 0;
            int Red = 0;
            int Green = 0;
            int Blue = 0;
            Bitmap Bitmap8 = default(Bitmap);
            Bitmap Bitmap4 = default(Bitmap);
            Bitmap Bitmap2 = default(Bitmap);
            Bitmap Bitmap1 = default(Bitmap);
            Bitmap Bitmap = null;
            sResult Result = new sResult();

            //-------- 64 --------

            GraphicPath = SlashPath + Name + "-64" + Convert.ToString(App.PlatformPathSeparator) + strTile;

            Result = BitmapUtil.LoadBitmap(GraphicPath, ref Bitmap);
            if ( !Result.Success )
            {
                ReturnResult.WarningAdd("Unable to load tile graphic: " + Result.Problem);
                return ReturnResult;
            }

            if ( Bitmap.Width != 64 | Bitmap.Height != 64 )
            {
                ReturnResult.WarningAdd("Tile graphic " + GraphicPath + " from tileset " + Name + " is not 64x64.");
                return ReturnResult;
            }

            BitmapTextureArgs.Texture = Bitmap;
            BitmapTextureArgs.MipMapLevel = 1;
            BitmapTextureArgs.Perform();

            //-------- 32 --------

            GraphicPath = SlashPath + Name + "-32" + Convert.ToString(App.PlatformPathSeparator) + strTile;

            Result = BitmapUtil.LoadBitmap(GraphicPath, ref Bitmap);
            if ( !Result.Success )
            {
                ReturnResult.WarningAdd("Unable to load tile graphic: " + Result.Problem);
                return ReturnResult;
            }

            if ( Bitmap.Width != 32 | Bitmap.Height != 32 )
            {
                ReturnResult.WarningAdd("Tile graphic " + GraphicPath + " from tileset " + Name + " is not 32x32.");
                return ReturnResult;
            }

            BitmapTextureArgs.Texture = Bitmap;
            BitmapTextureArgs.MipMapLevel = 2;
            BitmapTextureArgs.Perform();

            //-------- 16 --------

            GraphicPath = SlashPath + Name + "-16" + Convert.ToString(App.PlatformPathSeparator) + strTile;

            Result = BitmapUtil.LoadBitmap(GraphicPath, ref Bitmap);
            if ( !Result.Success )
            {
                ReturnResult.WarningAdd("Unable to load tile graphic: " + Result.Problem);
                return ReturnResult;
            }

            if ( Bitmap.Width != 16 | Bitmap.Height != 16 )
            {
                ReturnResult.WarningAdd("Tile graphic " + GraphicPath + " from tileset " + Name + " is not 16x16.");
                return ReturnResult;
            }

            BitmapTextureArgs.Texture = Bitmap;
            BitmapTextureArgs.MipMapLevel = 3;
            BitmapTextureArgs.Perform();

            //-------- 8 --------

            Bitmap8 = new Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for ( PixY = 0; PixY <= 7; PixY++ )
            {
                Y1 = PixY * 2;
                Y2 = Y1 + 1;
                for ( PixX = 0; PixX <= 7; PixX++ )
                {
                    X1 = PixX * 2;
                    X2 = X1 + 1;
                    PixelColorA = Bitmap.GetPixel(X1, Y1);
                    PixelColorB = Bitmap.GetPixel(X2, Y1);
                    PixelColorC = Bitmap.GetPixel(X1, Y2);
                    PixelColorD = Bitmap.GetPixel(X2, Y2);
                    Red = Convert.ToInt32(((PixelColorA.R) + PixelColorB.R + PixelColorC.R + PixelColorD.R) / 4.0F);
                    Green = Convert.ToInt32(((PixelColorA.G) + PixelColorB.G + PixelColorC.G + PixelColorD.G) / 4.0F);
                    Blue = Convert.ToInt32(((PixelColorA.B) + PixelColorB.B + PixelColorC.B + PixelColorD.B) / 4.0F);
                    Bitmap8.SetPixel(PixX, PixY, ColorTranslator.FromOle(ColorUtil.OSRGB(Red, Green, Blue)));
                }
            }

            BitmapTextureArgs.Texture = Bitmap8;
            BitmapTextureArgs.MipMapLevel = 4;
            BitmapTextureArgs.Perform();

            //-------- 4 --------

            Bitmap = Bitmap8;
            Bitmap4 = new Bitmap(4, 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for ( PixY = 0; PixY <= 3; PixY++ )
            {
                Y1 = PixY * 2;
                Y2 = Y1 + 1;
                for ( PixX = 0; PixX <= 3; PixX++ )
                {
                    X1 = PixX * 2;
                    X2 = X1 + 1;
                    PixelColorA = Bitmap.GetPixel(X1, Y1);
                    PixelColorB = Bitmap.GetPixel(X2, Y1);
                    PixelColorC = Bitmap.GetPixel(X1, Y2);
                    PixelColorD = Bitmap.GetPixel(X2, Y2);
                    Red = Convert.ToInt32(((PixelColorA.R) + PixelColorB.R + PixelColorC.R + PixelColorD.R) / 4.0F);
                    Green = Convert.ToInt32(((PixelColorA.G) + PixelColorB.G + PixelColorC.G + PixelColorD.G) / 4.0F);
                    Blue = Convert.ToInt32(((PixelColorA.B) + PixelColorB.B + PixelColorC.B + PixelColorD.B) / 4.0F);
                    Bitmap4.SetPixel(PixX, PixY, ColorTranslator.FromOle(ColorUtil.OSRGB(Red, Green, Blue)));
                }
            }

            BitmapTextureArgs.Texture = Bitmap4;
            BitmapTextureArgs.MipMapLevel = 5;
            BitmapTextureArgs.Perform();

            //-------- 2 --------

            Bitmap = Bitmap4;
            Bitmap2 = new Bitmap(2, 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for ( PixY = 0; PixY <= 1; PixY++ )
            {
                Y1 = PixY * 2;
                Y2 = Y1 + 1;
                for ( PixX = 0; PixX <= 1; PixX++ )
                {
                    X1 = PixX * 2;
                    X2 = X1 + 1;
                    PixelColorA = Bitmap.GetPixel(X1, Y1);
                    PixelColorB = Bitmap.GetPixel(X2, Y1);
                    PixelColorC = Bitmap.GetPixel(X1, Y2);
                    PixelColorD = Bitmap.GetPixel(X2, Y2);
                    Red = Convert.ToInt32(((PixelColorA.R) + PixelColorB.R + PixelColorC.R + PixelColorD.R) / 4.0F);
                    Green = Convert.ToInt32(((PixelColorA.G) + PixelColorB.G + PixelColorC.G + PixelColorD.G) / 4.0F);
                    Blue = Convert.ToInt32(((PixelColorA.B) + PixelColorB.B + PixelColorC.B + PixelColorD.B) / 4.0F);
                    Bitmap2.SetPixel(PixX, PixY, ColorTranslator.FromOle(ColorUtil.OSRGB(Red, Green, Blue)));
                }
            }

            BitmapTextureArgs.Texture = Bitmap2;
            BitmapTextureArgs.MipMapLevel = 6;
            BitmapTextureArgs.Perform();

            //-------- 1 --------

            Bitmap = Bitmap2;
            Bitmap1 = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            PixX = 0;
            PixY = 0;
            Y1 = PixY * 2;
            Y2 = Y1 + 1;
            X1 = PixX * 2;
            X2 = X1 + 1;
            PixelColorA = Bitmap.GetPixel(X1, Y1);
            PixelColorB = Bitmap.GetPixel(X2, Y1);
            PixelColorC = Bitmap.GetPixel(X1, Y2);
            PixelColorD = Bitmap.GetPixel(X2, Y2);
            Red = Convert.ToInt32(((PixelColorA.R) + PixelColorB.R + PixelColorC.R + PixelColorD.R) / 4.0F);
            Green = Convert.ToInt32(((PixelColorA.G) + PixelColorB.G + PixelColorC.G + PixelColorD.G) / 4.0F);
            Blue = Convert.ToInt32(((PixelColorA.B) + PixelColorB.B + PixelColorC.B + PixelColorD.B) / 4.0F);
            Bitmap1.SetPixel(PixX, PixY, ColorTranslator.FromOle(ColorUtil.OSRGB(Red, Green, Blue)));

            BitmapTextureArgs.Texture = Bitmap1;
            BitmapTextureArgs.MipMapLevel = 7;
            BitmapTextureArgs.Perform();

            return ReturnResult;
        }
    }
}