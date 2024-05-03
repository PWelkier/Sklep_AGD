using System;
using System.IO;

// Abstrakcyjna klasa Produkt
public abstract class Produkt
{
    protected string nazwa;
    protected int rokProdukcji;
    protected double cena;

    public Produkt(string nazwa, int rokProdukcji = 2022, double cena = 500)
    {
        this.nazwa = nazwa;
        this.rokProdukcji = rokProdukcji;
        this.cena = cena;
    }

    public abstract bool CzyRabat(int rok);
    public abstract double ObliczCene(int rok);

    public void AktualizujCene(double cena, int rok)
    {
        this.cena = cena;
        this.rokProdukcji = rok;
    }

    public override string ToString()
    {
        return $"{nazwa} {2022 - rokProdukcji} {cena}";
    }
}

// Klasa AGD dziedzicząca po klasie Produkt
public class AGD : Produkt
{
    public AGD(string nazwa) : base(nazwa) { }

    public override bool CzyRabat(int rok)
    {
        return rok - rokProdukcji <= 5;
    }

    public override double ObliczCene(int rok)
    {
        double rabat = 0;
        if (CzyRabat(rok))
        {
            rabat = cena * 0.2;
        }
        return cena - rabat;
    }
}

// Klasa Eko dziedzicząca po klasie AGD
public class Eko : AGD
{
    private int poziom;

    public Eko(string nazwa) : base(nazwa) { }

    public int Poziom
    {
        get { return poziom; }
        set { poziom = value; }
    }

    public override bool CzyRabat(int rok)
    {
        return base.CzyRabat(rok) && poziom > 2;
    }

    public override double ObliczCene(int rok)
    {
        double rabat = 0;
        if (CzyRabat(rok))
        {
            rabat = cena * 0.4;
        }
        else if (base.CzyRabat(rok))
        {
            rabat = cena * 0.2;
        }
        return cena - rabat;
    }

    public override string ToString()
    {
        return base.ToString() + $" {poziom}";
    }
}

// Interfejs IRabat
public interface IRabat
{
    bool CzyRabat(int rok);
    double ObliczCene(int rok);
}

// Szablon klasy Magazyn<T>
public class Magazyn<T> where T : Produkt
{
    public string nazwa;
    public int rok;
    public T[] obiekty;

    public event EventHandler<T> ProduktDodany;
    public event EventHandler<T> ProduktUsuniety;

    public Magazyn() { }

    public Magazyn(string nazwa, int rok, T[] obiekty)
    {
        this.nazwa = nazwa;
        this.rok = rok;
        this.obiekty = obiekty;
    }

    public int IleRabatow()
    {
        int count = 0;
        foreach (var obiekt in obiekty)
        {
            if (obiekt.CzyRabat(rok))
                count++;
        }
        return count;
    }

    public double SumaRabatow()
    {
        double suma = 0;
        foreach (var obiekt in obiekty)
        {
            suma += obiekt.ObliczCene(rok);
        }
        return suma;
    }

    public void DodajProdukt(T produkt)
    {
        Array.Resize(ref obiekty, obiekty.Length + 1);
        obiekty[obiekty.Length - 1] = produkt;
        ProduktDodany?.Invoke(this, produkt);
    }

    public void UsunProdukt(T produkt)
    {
        for (int i = 0; i < obiekty.Length; i++)
        {
            if (obiekty[i].Equals(produkt))
            {
                T[] tempArray = new T[obiekty.Length - 1];
                Array.Copy(obiekty, 0, tempArray, 0, i);
                Array.Copy(obiekty, i + 1, tempArray, i, obiekty.Length - i - 1);
                obiekty = tempArray;
                ProduktUsuniety?.Invoke(this, produkt);
                break;
            }
        }
    }
}

// Klasa MagazynAGD dziedzicząca po klasie Magazyn
public class MagazynAGD : Magazyn<Produkt>
{
    public MagazynAGD(string nazwa, int rok, Produkt[] obiekty) : base(nazwa, rok, obiekty) { }

    public double Suma()
    {
        double suma = 0;
        foreach (var produkt in obiekty)
        {
            suma += produkt.ObliczCene(rok);
        }
        return suma;
    }

    public int IleSpelnia(WarunekHandler kryterium, double wartosc)
    {
        int count = 0;
        foreach (var produkt in obiekty)
        {
            if (produkt is IRabat && kryterium((IRabat)produkt, wartosc)) // Sprawdź czy produkt implementuje IRabat
                count++;
        }
        return count;
    }

}

public delegate bool WarunekHandler(IRabat obiekt, double wartosc);


class Program
{
    static bool CenaWyzszaNiz(IRabat obiekt, double wartosc)
    {
        return obiekt.ObliczCene(DateTime.Now.Year) > wartosc;
    }

    static bool CenaNizszaNiz(IRabat obiekt, double wartosc)
    {
        return obiekt.ObliczCene(DateTime.Now.Year) < wartosc;
    }

    static void Main(string[] args)
    {
        Produkt[] produkty = new Produkt[]
        {
            new AGD("Lodówka"),
            new Eko("Pralka") { Poziom = 3 },
            new AGD("Zmywarka"),
            new Eko("Kuchenka") { Poziom = 1 }
        };

        MagazynAGD magazyn = new MagazynAGD("Komis AGD Adama", DateTime.Now.Year, produkty);

        // Dodawanie nowego produktu do magazynu
        magazyn.ProduktDodany += (sender, produkt) =>
        {
            Console.WriteLine($"Dodano produkt do magazynu: {produkt}");
        };

        // Usuwanie produktu z magazynu
        magazyn.ProduktUsuniety += (sender, produkt) =>
        {
            Console.WriteLine($"Usunięto produkt z magazynu: {produkt}");
        };

        // Dodawanie nowego produktu
        magazyn.DodajProdukt(new AGD("Mikrofalówka"));

        // Usuwanie produktu
        magazyn.UsunProdukt(produkty[0]);

        // Wyświetlanie liczby produktów spełniających warunek
        int ileSpelnia = magazyn.IleSpelnia(CenaNizszaNiz, 300);
        Console.WriteLine($"Liczba produktów spełniających warunek: {ileSpelnia}");

        // Wyświetlanie sumy sprzedaży wszystkich produktów
        Console.WriteLine($"Suma sprzedaży wszystkich produktów: {magazyn.Suma()}");

        // Wyświetlanie liczby produktów, dla których zostanie naliczony rabat
        Console.WriteLine($"Liczba produktów objętych rabatem: {magazyn.IleRabatow()}");

        Console.ReadLine();
    }
}
