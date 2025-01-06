using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GameplayClasses
{
    public class Player
    {
        public int InactivityCount { get; set; } = 0;
        public List<string> TreasuresForSearch { get; set; }

        public Character Character { get; set; }

        public (int Row, int Col) InitialPosition { get; set; }
        public (int Row, int Col) CurrentPosition { get; set; }
        public int PlayerId { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && GetType() == obj.GetType())
            {
                Player other = (Player)obj;
                result = Name == other.Name;
            }
            return result;
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0; 
        }

        public void AssignTreasure(string treasureName)
        {
            if (TreasuresForSearch == null)
            {
                TreasuresForSearch = new List<string>();
            }
            TreasuresForSearch.Add(treasureName);
        }

        public void CollectTreasure()
        {
            TreasuresForSearch.Remove(TreasuresForSearch.First());
        }

        public void ClearTreasures()
        {
            if (TreasuresForSearch == null)
            {
                TreasuresForSearch = new List<string>();
            }
            TreasuresForSearch.Clear();
        }

    }
}
