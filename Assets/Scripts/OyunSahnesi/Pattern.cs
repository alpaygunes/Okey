using System;
using System.Collections.Generic;

public static class Pattern
{
    // Tanımlı tüm karakterler
    private static readonly Dictionary<char, int[,]> _patterns;
    private static readonly char[] _keys;
    private static readonly Random _rng = new Random();

    static Pattern()
    {
        // Geçici sözlük: her karakter için 5 satırlık ikili string dizisi
        var raw = new Dictionary<char, string[]>
        {
            // ==== HARFLER ====
            ['A'] = new[] {"01110","10001","10001","11111","10001"},
            ['B'] = new[] {"11110","10001","11110","10001","11110"},
            ['C'] = new[] {"01110","10001","10000","10001","01110"},
            ['D'] = new[] {"11110","10001","10001","10001","11110"},
            ['E'] = new[] {"11111","10000","11110","10000","11111"},
            ['F'] = new[] {"11111","10000","11110","10000","10000"},
            ['G'] = new[] {"01110","10000","10011","10001","01110"},
            ['H'] = new[] {"10001","10001","11111","10001","10001"},
            ['I'] = new[] {"11111","00100","00100","00100","11111"},
            ['J'] = new[] {"00111","00001","00001","10001","01110"},
            ['K'] = new[] {"10001","10010","11100","10010","10001"},
            ['L'] = new[] {"10000","10000","10000","10000","11111"},
            ['M'] = new[] {"10001","11011","10101","10001","10001"},
            ['N'] = new[] {"10001","11001","10101","10011","10001"},
            ['O'] = new[] {"01110","10001","10001","10001","01110"},
            ['P'] = new[] {"11110","10001","11110","10000","10000"},
            ['Q'] = new[] {"01110","10001","10001","10011","01111"},
            ['R'] = new[] {"11110","10001","11110","10010","10001"},
            ['S'] = new[] {"01111","10000","01110","00001","11110"},
            ['T'] = new[] {"11111","00100","00100","00100","00100"},
            ['U'] = new[] {"10001","10001","10001","10001","01110"},
            ['V'] = new[] {"10001","10001","10001","01010","00100"},
            ['W'] = new[] {"10001","10001","10101","11011","10001"},
            ['X'] = new[] {"10001","01010","00100","01010","10001"},
            ['Y'] = new[] {"10001","01010","00100","00100","00100"},
            ['Z'] = new[] {"11111","00010","00100","01000","11111"},

            // ==== RAKAMLAR ====
            ['0'] = new[] {"01110","10001","10001","10001","01110"},
            ['1'] = new[] {"00100","01100","00100","00100","01110"},
            ['2'] = new[] {"01110","10001","00010","00100","11111"},
            ['3'] = new[] {"11110","00001","01110","00001","11110"},
            ['4'] = new[] {"10010","10010","11111","00010","00010"},
            ['5'] = new[] {"11111","10000","11110","00001","11110"},
            ['6'] = new[] {"01111","10000","11110","10001","01110"},
            ['7'] = new[] {"11111","00001","00010","00100","01000"},
            ['8'] = new[] {"01110","10001","01110","10001","01110"},
            ['9'] = new[] {"01110","10001","01111","00001","11110"},
        };

        _patterns = new Dictionary<char, int[,]>(raw.Count);
        foreach (var kvp in raw)
        {
            _patterns[kvp.Key] = ToMatrix(kvp.Value);
        }

        _keys = new char[_patterns.Count];
        _patterns.Keys.CopyTo(_keys, 0);
    }

    /// <summary>
    /// Belirtilen karakterin 5x5 matrisini döndürür. Harfler için büyük/küçük fark etmez.
    /// Tanımsız karakterde ArgumentException fırlatır.
    /// </summary>
    public static int[,] getCharMatris(char ch)
    {
        ch = char.ToUpperInvariant(ch);
        if (_patterns.TryGetValue(ch, out var mat))
            return (int[,])mat.Clone(); // Koruma: dışarıda değiştirilmesin
        throw new ArgumentException($"'{ch}' için desen tanımlı değil.");
    }

    /// <summary>
    /// String parametreyi destekleyen aşırı yükleme (ilk karakter alınır).
    /// </summary>
    public static int[,] getCharMatris(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("Boş string geçilemez.");
        return getCharMatris(s[0]);
    }

    /// <summary>
    /// Tanımlı karakterler arasından rasgele birini ve matrisini döndürür.
    /// </summary>
    public static (char ch, int[,] matrix) getRandom()
    {
        var idx = _rng.Next(_keys.Length);
        var ch = _keys[idx];
        return (ch, getCharMatris(ch)); // Clone içeride yapılıyor
    }

    /// <summary>
    /// Yardımcı: 5 adet ikili string -> 5x5 int[,] dönüşümü.
    /// </summary>
    private static int[,] ToMatrix(string[] rows)
    {
        if (rows.Length != 5) throw new ArgumentException("5 satır olmalı.");
        var m = new int[5,5];
        for (int r = 0; r < 5; r++)
        {
            var row = rows[r];
            if (row.Length != 5) throw new ArgumentException("Her satır 5 karakter olmalı.");
            for (int c = 0; c < 5; c++)
            {
                char ch = row[c];
                m[r,c] = ch == '1' ? 1 : 0;
            }
        }
        return m;
    }
}
