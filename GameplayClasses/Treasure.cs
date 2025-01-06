using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameplayClasses
{
    public class Treasure
    {
        public bool IsFound { get; set; } = false;
        public string Name { get; set; }
        public BitmapImage ImageTreasure { get; set; }

        public Treasure(string name, BitmapImage image) 
        {
            Name = name;
            ImageTreasure = image;
        }

        public void TreasureWasFounded()
        {
            IsFound = true;
        }
    }
}
