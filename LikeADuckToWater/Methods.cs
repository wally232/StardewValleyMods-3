﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace LikeADuckToWater
{
    public partial class ModEntry
    {

        private bool CheckDuck(FarmAnimal animal, Vector2 t)
        {
            if (pickedTiles.Contains(t))
                return false;
            var c = new PathFindController(animal, animal.currentLocation, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), hopTileDict[t][0].dir, false, new PathFindController.endBehavior(TryHop), 200, new Point((int)t.X, (int)t.Y), true);
            if (c.pathToEndPoint is not null)
            {
                pickedTiles.Add(t);
                animal.controller = c;
                SMonitor.Log($"{animal.displayName} is travelling from {animal.getTileLocation()} to {t} to swim");
                return true;
            }
            return false;
        }
        private static void DoHop(FarmAnimal animal, Vector2 key)
        {
            animal.Position = key * 64;
            animal.isSwimming.Value = true;
            animal.hopOffset = hopTileDict[key][0].offset;
            animal.pauseTimer = 0;
            animal.modData[swamTodayKey] = "true";
            SMonitor.Log($"{animal.displayName} is hopping into the water");
        }

        private static void TryHop(Character character, GameLocation location)
        {
            if (!hopTileDict.Any())
                return;
            var tile = character.getTileLocation();
            var keys = hopTileDict.Keys.ToList();
            foreach (var t in keys)
            {
                if (Vector2.Distance(tile, t) < 2)
                {
                    DoHop((FarmAnimal) character, t);
                    return;
                }
            }
        }
        private static void TryMoveToWater(FarmAnimal animal, GameLocation location)
        {
            if (!hopTileDict.Any())
                return;
            var tile = animal.getTileLocation();
            var keys = hopTileDict.Keys.ToList();
            keys.Sort(delegate (Vector2 a, Vector2 b)
            {
                return Vector2.Distance(b, tile).CompareTo(Vector2.Distance(a, tile));
            });
            Stack<Vector2> stack = new Stack<Vector2>();
            foreach (var t in keys)
            {
                if(Vector2.Distance(tile, t) < 2)
                {
                    DoHop(animal, t);
                    return;
                }
                else if(Vector2.Distance(tile, t) > Config.MaxDistance)
                {
                    continue;
                }
                stack.Push(t);
            }
            ducksToCheck[animal] = stack;
        }
        private static void RebuildHopSpots(Farm farm)
        {
            if (!Config.ModEnabled || farm.waterTiles is null)
                return;

            Stopwatch s = new Stopwatch();
            s.Start();

            long id = ((Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null)).getNewID();
            FarmAnimal animal = new FarmAnimal("Duck", id, Game1.player.UniqueMultiplayerID);
            hopTileDict = new();
            for (int y = 0; y < farm.map.Layers[0].LayerHeight; y++)
            {
                for (int x = 0; x < farm.map.Layers[0].LayerWidth; x++)
                {
                    animal.Position = new Vector2(x, y) * 64;
                    if (!farm.waterTiles.waterTiles[x,y].isWater && !farm.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, false, 0, false, animal, false, false, false))
                    {
                        List<HopInfo> hoppableDirs = GetHoppableDirs(farm, animal);
                        if(hoppableDirs.Any())
                        {
                            hopTileDict.Add(new Vector2(x, y), hoppableDirs);
                        }
                    }
                }
            }
            s.Stop();
            SMonitor.Log($"Rebuild took {s.ElapsedMilliseconds}ms");

            SMonitor.Log($"Got {hopTileDict.Count} hoppable tiles");
        }

        private static List<HopInfo> GetHoppableDirs(Farm farm, FarmAnimal animal)
        {
            List<HopInfo> list = new List<HopInfo>();
            var tile = animal.getTileLocationPoint();
            for(int i = 0; i < 4; i++)
            {
                Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, i, 1f);
                if (offset != Vector2.Zero)
                {
                    Point hop_over_tile = tile + Utility.Vector2ToPoint(offset);
                    Point hop_tile = hop_over_tile + Utility.Vector2ToPoint(offset);
                    Rectangle hop_destination = animal.GetBoundingBox();
                    hop_destination.Offset(offset * 128);
                    if (farm.isWaterTile(hop_over_tile.X, hop_over_tile.Y) && farm.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !farm.isCollidingPosition(hop_destination, Game1.viewport, false, 0, false, animal) && farm.isOpenWater(hop_tile.X, hop_tile.Y))
                    {
                        list.Add(new HopInfo() { 
                            dir = i,
                            offset = offset * 128
                        });
                    }
                }
            }
            return list;
        }
    }
}