var des = new DES16Bits.DES16Bits("100111000011");

var enc = des.EncryptMessage("Hello, World!!");
Console.WriteLine(enc);

var dec = des.DecryptMessage(enc);
Console.WriteLine(dec);