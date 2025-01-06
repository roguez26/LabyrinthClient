using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using GameplayClasses;
using System.Collections.Generic;

namespace GameplayClasses
{
    public class Tile : ICloneable
    {
        public int Type { get; set; }
        public bool IsTopOpen { get; set; }
        public bool IsRightOpen { get; set; }
        public bool IsBottomOpen { get; set; }
        public bool IsLeftOpen { get; set; }
        public BitmapImage Image { get; set; }
        public TransformedBitmap RotatedImage { get; private set; }
        public int RotationAngle { get; set; }
        public List<Player> PlayersOnTile { get; set; } = new List<Player>();
        public Treasure TreasureOnTile { get; set; }

        public (int Row, int Col) Position { get; set; }

        public Tile(int type, BitmapImage image, bool isTopOpen, bool isRightOpen, bool isBottomOpen, bool isLeftOpen)
        {
            Type = type;
            Image = image;
            IsTopOpen = isTopOpen;
            IsRightOpen = isRightOpen;
            IsBottomOpen = isBottomOpen;
            IsLeftOpen = isLeftOpen;
            UpdateRotatedImage();
        }

        public object Clone()
        {
            Tile clone = (Tile)this.MemberwiseClone();
            clone.Image = (BitmapImage)this.Image.Clone();
            clone.UpdateRotatedImage();
            return clone;
        }

        public void RotateRight()
        {
            bool temp;
            temp = IsTopOpen;
            IsTopOpen = IsLeftOpen;
            IsLeftOpen = IsBottomOpen;
            IsBottomOpen = IsRightOpen;
            IsRightOpen = temp;

            RotationAngle = (RotationAngle + 90) % 360;
            UpdateRotatedImage();
        }

        private void UpdateRotatedImage()
        {
            if (Image != null)
            {
                RotatedImage = new TransformedBitmap();
                RotatedImage.BeginInit();
                RotatedImage.Source = (BitmapImage)Image.Clone();
                RotatedImage.Transform = new RotateTransform(RotationAngle);
                RotatedImage.EndInit();
                RotatedImage.Freeze();
            }
        }

        public BitmapSource GetCurrentImage()
        {
            return RotatedImage;
        }

        public void AddPlayer(ref Player player)
        {
            PlayersOnTile.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            PlayersOnTile.Remove(player);
        }

        public bool HasPlayers()
        {
            return PlayersOnTile.Count > 0;
        }

    }
}   
