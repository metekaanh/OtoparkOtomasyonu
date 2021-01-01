using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoparkOtomasyonu
{
    public class Arac
    {

        public string Plaka;
        public int AracNo;

        public static List<Arac> Arabalar = new List<Arac> 
        { 
            new Arac { AracNo = 0, Plaka = "34AB3385" },
            new Arac { AracNo = 1, Plaka = "34HBG35" },
            new Arac { AracNo = 2, Plaka = "34HKN91" },
            new Arac { AracNo = 3, Plaka = "34DAR915" },
        };

        public static Arac ArabaGetir(string plaka)
        {
            return Arabalar.Where(s => s.Plaka == plaka).FirstOrDefault();
        }

    }
}
