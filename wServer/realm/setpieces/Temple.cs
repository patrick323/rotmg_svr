﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.logic.loot;
using wServer.logic;

namespace wServer.realm.setpieces
{
    abstract class Temple : ISetPiece
    {
        public abstract int Size { get; }
        public abstract void RenderSetPiece(World world, IntPoint pos);

        protected static readonly byte DarkGrass = (byte)XmlDatas.IdToType["Dark Grass"];
        protected static readonly byte Floor = (byte)XmlDatas.IdToType["Jungle Temple Floor"];
        protected static readonly short WallA = XmlDatas.IdToType["Jungle Temple Bricks"];
        protected static readonly short WallB = XmlDatas.IdToType["Jungle Temple Walls"];
        protected static readonly short WallC = XmlDatas.IdToType["Jungle Temple Column"];
        protected static readonly short Flower = XmlDatas.IdToType["Jungle Ground Flowers"];
        protected static readonly short Grass = XmlDatas.IdToType["Jungle Grass"];
        protected static readonly short Tree = XmlDatas.IdToType["Jungle Tree Big"];

        protected static readonly Loot chest = new Loot(
                new TierLoot(4, ItemType.Weapon, 0.3),
                new TierLoot(5, ItemType.Weapon, 0.2),

                new TierLoot(4, ItemType.Armor, 0.3),
                new TierLoot(5, ItemType.Armor, 0.2),

                new TierLoot(1, ItemType.Ability, 0.25),
                new TierLoot(2, ItemType.Ability, 0.15),

                new TierLoot(2, ItemType.Ring, 0.3),
                new TierLoot(3, ItemType.Ring, 0.2),

                new TierLoot(1, ItemType.Potion, 0.5),
                new TierLoot(1, ItemType.Potion, 0.5),
                new TierLoot(1, ItemType.Potion, 0.5)
            );

        protected static void Render(Temple temple, World world, IntPoint pos, int[,] ground, int[,] objs)
        {

            for (int x = 0; x < temple.Size; x++)                  //Rendering
                for (int y = 0; y < temple.Size; y++)
                {
                    if (ground[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.TileId = DarkGrass; tile.ObjType = 0;
                        world.Obstacles[x + pos.X, y + pos.Y] = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (ground[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.TileId = Floor; tile.ObjType = 0;
                        world.Obstacles[x + pos.X, y + pos.Y] = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }

                    if (objs[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = WallA;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 2;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (objs[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = WallB;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 2;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (objs[x, y] == 3)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = WallC;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 2;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (objs[x, y] == 4)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = Flower;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (objs[x, y] == 5)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = Grass;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    else if (objs[x, y] == 6)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.ObjType = Tree;
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        world.Obstacles[x + pos.X, y + pos.Y] = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                }
        }
    }
}
