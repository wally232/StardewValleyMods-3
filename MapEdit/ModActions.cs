﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class ModActions
    {
        public static void SwitchTile(bool increase)
        {
            List<string> layers = new List<string>();
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (ModEntry.currentTileDict.Value.ContainsKey(layer.Id))
                    layers.Add(layer.Id);
            }

            if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.LayerModButton))
            {
                if (layers.Count < 2)
                    return;
                if (increase)
                {
                    if (ModEntry.currentLayer.Value >= layers.Count - 1)
                        ModEntry.currentLayer.Value = 0;
                    else
                        ModEntry.currentLayer.Value++;
                }
                else
                {
                    if (ModEntry.currentLayer.Value < 1)
                        ModEntry.currentLayer.Value = layers.Count - 1;
                    else
                        ModEntry.currentLayer.Value--;
                }

                string text = string.Format(ModEntry.SHelper.Translation.Get("current-layer"), layers[ModEntry.currentLayer.Value]);
                ModEntry.SMonitor.Log(text);
                ModActions.ShowMessage(text);
            }
            else if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.SheetModButton))
            {
                if (ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]] is AnimatedTile)
                    return;

                List<TileSheet> sheets = new List<TileSheet>();
                foreach (TileSheet sheet in Game1.player.currentLocation.map.TileSheets)
                {
                    if (sheet.Id != "Paths")
                        sheets.Add(sheet);
                }
                if (sheets.Count < 2)
                    return;
                Tile tile = ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]];
                if (tile == null)
                {
                    Layer layer = Game1.player.currentLocation.map.GetLayer(layers[ModEntry.currentLayer.Value]);
                    tile = new StaticTile(layer, sheets[0], BlendMode.Alpha, -1);
                }
                int tileSheetIndex = sheets.IndexOf(tile.TileSheet);
                if (increase)
                {
                    if (tileSheetIndex >= sheets.Count - 1)
                        tileSheetIndex = 0;
                    else
                        tileSheetIndex++;
                }
                else
                {
                    if (tileSheetIndex < 1)
                        tileSheetIndex = sheets.Count - 1;
                    else
                        tileSheetIndex--;
                }
                TileSheet tileSheet = sheets[tileSheetIndex];
                ModEntry.SHelper.Reflection.GetField<TileSheet>(tile, "m_tileSheet").SetValue(tileSheet);
                tile.TileIndex = 0;
                string text = string.Format(ModEntry.SHelper.Translation.Get("current-tilesheet"), tileSheet.Id);
                ModEntry.SMonitor.Log(text);
                ModActions.ShowMessage(text);
            }
            else
            {
                if (ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]] == null)
                {
                    ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]] = new StaticTile(Game1.player.currentLocation.map.GetLayer(layers[ModEntry.currentLayer.Value]), Game1.player.currentLocation.map.TileSheets[0], BlendMode.Alpha, -1);
                }
                if (ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]] is AnimatedTile)
                    return;

                Tile tile = ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]];
                //context.Monitor.Log($"old index {tile.TileIndex}");
                if (tile == null)
                {
                    ModEntry.SMonitor.Log($"Layer {layers[ModEntry.currentLayer.Value]} copied tile is null, creating empty tile");
                    Layer layer = Game1.player.currentLocation.map.GetLayer(layers[ModEntry.currentLayer.Value]);
                    ModEntry.currentTileDict.Value[layers[ModEntry.currentLayer.Value]] = new StaticTile(layer, Game1.player.currentLocation.map.TileSheets[0], BlendMode.Alpha, -1);
                }
                if (increase)
                {
                    if (tile.TileIndex >= tile.TileSheet.TileCount - 1)
                        tile.TileIndex = -1;
                    else
                        tile.TileIndex++;
                }
                else
                {
                    if (tile.TileIndex < 0)
                        tile.TileIndex = tile.TileSheet.TileCount - 1;
                    else
                        tile.TileIndex--;
                }
                //context.Monitor.Log($"new index {tile.TileIndex}");
            }
            Game1.playSound(ModEntry.Config.ScrollSound);
        }


        public static void CreateTextures()
        {
            int thickness = ModEntry.Config.BorderThickness;

            ModEntry.existsTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            Color[] data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = ModEntry.Config.ExistsColor;
                else
                    data[i] = Color.Transparent;
            }
            ModEntry.existsTexture.SetData(data);

            ModEntry.activeTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = ModEntry.Config.ActiveColor;
                else
                    data[i] = Color.Transparent;
            }
            ModEntry.activeTexture.SetData(data);

            ModEntry.copiedTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = ModEntry.Config.CopiedColor;
                else
                    data[i] = Color.Transparent;
            }
            ModEntry.copiedTexture.SetData(data);

        }

        public static void DeactivateMod()
        {
            ModEntry.modActive.Value = false;
            ModEntry.copiedTileLoc.Value = new Vector2(-1, -1);
            ModEntry.pastedTileLoc.Value = new Vector2(-1, -1);
            ModEntry.currentTileDict.Value = new Dictionary<string, Tile>();
            ModEntry.currentLayer.Value = 0;
        }


        public static void ShowMessage(string message)
        {
            Game1.hudMessages.RemoveAll((HUDMessage p) => p.number == ModEntry.modNumber);
            Game1.addHUDMessage(new HUDMessage(message, 3)
            {
                noIcon = true,
                number = ModEntry.modNumber
            });
        }
    }
}