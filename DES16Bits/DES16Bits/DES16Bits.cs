using System.Text;

namespace DES16Bits;

public class DES16Bits
{
    // Permutation table
    private static readonly int[] Permut = { 4, 2, 7, 5, 1, 3, 8, 6, 3, 7, 2, 8 };

    // S-Boxes
    private static readonly string[,] S1Box = new string[4, 16]
    {
        { "0110", "1011", "1110", "1001", "0100", "1100", "0001", "0011", "0111", "0000", "1010", "0101", "1101", "1000", "1111", "0010" },
        { "0001", "1110", "1011", "0100", "0110", "0000", "1101", "1010", "1001", "0011", "1111", "0111", "1000", "1100", "0010", "0101" },
        { "1110", "0001", "0100", "1011", "1101", "1001", "0110", "1111", "0000", "0111", "1000", "0010", "0101", "1010", "1100", "0011" },
        { "1011", "1111", "0101", "1100", "0001", "0110", "0010", "1110", "1010", "1000", "0011", "0000", "1101", "0111", "1001", "0100" }
    };

    private static readonly string[,] S2Box = new string[4, 16]
    {
        { "1101", "0010", "0110", "1110", "0000", "1001", "1011", "0100", "0011", "1111", "0101", "1100", "1010", "0111", "1111", "1000" },
        { "0100", "1011", "1001", "0000", "1110", "0110", "0010", "1101", "1111", "1010", "0111", "1100", "1000", "0101", "0011", "0001" },
        { "1110", "0111", "1010", "1101", "0101", "0011", "1111", "0001", "1001", "0100", "1000", "0000", "1100", "0110", "0010", "1011" },
        { "0011", "1100", "1111", "0110", "1000", "1010", "1101", "0001", "0100", "1110", "0000", "1001", "0111", "0101", "0010", "1011" }
    };


    private int _rounds;
    private readonly List<string> _subkeys = new List<string>();

    public DES16Bits(string key, int rounds = 6)
    {
        _rounds = rounds;
        GenerateSubkeys(key);
    }

    private void GenerateSubkeys(string key)
    {
        string currentKey = key;

        for (int i = 0; i < _rounds; i++)
        {
            _subkeys.Add(currentKey);
            currentKey = ShiftLeft(currentKey);
        }
    }

    private string ShiftLeft(string num)
    {
        return num.Substring(1) + num[0];
    }

    private int XOR(string str1, string str2)
    {
        return Convert.ToInt32(str1, 2) ^ Convert.ToInt32(str2, 2);
    }

    private (string, string) SplitMessage(string message)
    {
        return (message.Substring(0, 8), message.Substring(8));
    }

    private string SBoxes(int num, string key)
    {
        string binStr = Convert.ToString(num, 2).PadLeft(key.Length, '0');

        int S1ColIndx = Convert.ToInt32(binStr.Substring(0, 2), 2);
        int S1RowIndx = Convert.ToInt32(binStr.Substring(2, 4), 2);
        int S2ColIndx = Convert.ToInt32(binStr.Substring(6, 2), 2);
        int S2RowIndx = Convert.ToInt32(binStr.Substring(8, 4), 2);

        string s1 = S1Box[S1ColIndx, S1RowIndx];
        string s2 = S2Box[S2ColIndx, S2RowIndx];

        return s1 + s2;
    }

    private List<string> GetMessage(string message)
    {
        var binMesList = message.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')).ToList();

        if (binMesList.Count % 2 != 0)
            binMesList.Add("00100000");

        return Enumerable.Range(0, binMesList.Count / 2)
            .Select(i => binMesList[2 * i] + binMesList[2 * i + 1])
            .ToList();
    }

    private (string, string) Encrypt(string message)
    {
        (string L, string Ri) = SplitMessage(message);

        for (int round = 0; round < _rounds; round++)
        {
            var permList = Permut.Select(i => Ri[i - 1]).ToList();
            string EofRi = string.Join("", permList);
            int ERixorK = XOR(EofRi, _subkeys[round]);
            string sbox = SBoxes(ERixorK, _subkeys[round]);
            int sboxXORl = XOR(sbox, L);
            string R = Convert.ToString(sboxXORl, 2).PadLeft(L.Length, '0');
            L = Ri;
            Ri = R;
        }

        return (Ri, L);
    }

    private (string, string) Decrypt(string message)
    {
        (string L, string Ri) = SplitMessage(message);

        for (int round = _rounds - 1; round >= 0; round--)
        {
            var permList = Permut.Select(i => Ri[i - 1]).ToList();
            string EofRi = string.Join("", permList);
            int ERixorK = XOR(EofRi, _subkeys[round]);
            string sbox = SBoxes(ERixorK, _subkeys[round]);
            int sboxXORl = XOR(sbox, L);
            string R = Convert.ToString(sboxXORl, 2).PadLeft(L.Length, '0');
            L = Ri;
            Ri = R;
        }

        return (L, Ri);
    }


    public string EncryptMessage(string message)
    {
        string enc = "";

        foreach (string coupledBins in GetMessage(message))
        {
            var (R, L) = Encrypt(coupledBins);
            enc += string.Concat(Enumerable.Range(0, (R + L).Length / 8)
                .Select(i => (char)Convert.ToInt32((R + L).Substring(8 * i, 8), 2)));
        }

        return enc;
    }

    public string DecryptMessage(string message)
    {
        string dec = "";

        foreach (string coupledBins in GetMessage(message))
        {
            var (dL, dR) = Decrypt(coupledBins);
            dec += string.Concat(Enumerable.Range(0, (dR + dL).Length / 8)
                .Select(i => (char)Convert.ToInt32((dR + dL).Substring(8 * i, 8), 2)));
        }

        return dec;
    }
}
