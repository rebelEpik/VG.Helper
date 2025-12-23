using System;
using System.Collections.Generic;
using VG.Helper.Data;

namespace VG.Helper.Data
{
    public class Spaceship
    {
        public Guid guid { get; set; }
        public string currentHullHP { get; set; }
        public string currentArmorHP { get; set; }
        public string currentShieldHP { get; set; }

        public string maxHullHP { get; set; }
        public string maxArmorHP { get; set; }
        public string maxShieldHP { get; set; }

        public bool traveling { get; set; }
        public string travelSpeed { get; set; }

        public List<Item> cargo { get; set; } = new List<Item>();
        public string cargoCapacity { get; set; }

        public List<Item> equipment { get; set; } = new List<Item>();
        public List<Item> boosters { get; set; } = new List<Item>();

        public string shipClass { get; set; }
    }
}
