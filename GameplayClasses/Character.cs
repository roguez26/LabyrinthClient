using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameplayClasses
{
    public class Character
    {
        public string Name { get; set; }
        public BitmapImage ImageCharacter { get; private set; }

        public Character(string name)
        {
            Name = name;
            ImageCharacter = GameBoard.LoadImage($"GameplayClasses.characters.{name}.png");
        }
    }
}
