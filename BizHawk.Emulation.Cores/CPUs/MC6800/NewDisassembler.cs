using System;
using System.Collections.Generic;
using System.Text;

namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public sealed partial class MC6800 : IDisassemblable
	{
		static string[] table =
		{
			"???", // 00
			"NOP", // 01
			"???", // 02
			"???", // 03
			"???", // 04
			"???", // 05
			"TAP", // 06
			"TPA", // 07
			"INX", // 08
			"DEX", // 09
			"CLV", // 0a
			"SEV", // 0b
			"CLC", // 0c
			"SEC", // 0d
			"CLI", // 0e
			"SEI", // 0f
			"SBA", // 10
			"CBA", // 11
			"???", // 12
			"???", // 13
			"!NBA", // 14
			"???", // 15
			"TAB", // 16
			"TBA", // 17
			"???", // 18
			"DAA", // 19
			"???", // 1a
			"ADD A,B", // 1b
			"???", // 1c
			"???", // 1d
			"???", // 1e
			"???", // 1f
			"BRA", // 20
			"???", // 21
			"BHI", // 22
			"BLS", // 23
			"BCC", // 24
			"BCS", // 25
			"BNE", // 26
			"BEQ", // 27
			"BVC", // 28
			"BVS", // 29
			"BPL", // 2a
			"BMI", // 2b
			"BGE", // 2c
			"BLT", // 2d
			"BGT", // 2e
			"BLE", // 2f
			"TSX", // 30
			"INS", // 31
			"POP A", // 32
			"POP B", // 33
			"DES", // 34
			"TXS", // 35
			"PSH A", // 36
			"PSH B", // 37
			"???", // 38
			"RTS", // 39
			"???", // 3a
			"RTI", // 3b
			"???", // 3c
			"???", // 3d
			"WAI", // 3e
			"SWI", // 3f
			"LD   B,B", // 40
			"LD   B,C", // 41
			"LD   B,D", // 42
			"LD   B,E", // 43
			"LD   B,H", // 44
			"LD   B,L", // 45
			"LD   B,(HL)", // 46
			"LD   B,A", // 47
			"LD   C,B", // 48
			"LD   C,C", // 49
			"LD   C,D", // 4a
			"LD   C,E", // 4b
			"LD   C,H", // 4c
			"LD   C,L", // 4d
			"LD   C,(HL)", // 4e
			"LD   C,A", // 4f
			"LD   D,B", // 50
			"LD   D,C", // 51
			"LD   D,D", // 52
			"LD   D,E", // 53
			"LD   D,H", // 54
			"LD   D,L", // 55
			"LD   D,(HL)", // 56
			"LD   D,A", // 57
			"LD   E,B", // 58
			"LD   E,C", // 59
			"LD   E,D", // 5a
			"LD   E,E", // 5b
			"LD   E,H", // 5c
			"LD   E,L", // 5d
			"LD   E,(HL)", // 5e
			"LD   E,A", // 5f
			"LD   H,B", // 60
			"LD   H,C", // 61
			"LD   H,D", // 62
			"LD   H,E", // 63
			"LD   H,H", // 64
			"LD   H,L", // 65
			"LD   H,(HL)", // 66
			"LD   H,A", // 67
			"LD   L,B", // 68
			"LD   L,C", // 69
			"LD   L,D", // 6a
			"LD   L,E", // 6b
			"LD   L,H", // 6c
			"LD   L,L", // 6d
			"LD   L,(HL)", // 6e
			"LD   L,A", // 6f
			"LD   (HL),B", // 70
			"LD   (HL),C", // 71
			"LD   (HL),D", // 72
			"LD   (HL),E", // 73
			"LD   (HL),H", // 74
			"LD   (HL),L", // 75
			"HALT", // 76
			"LD   (HL),A", // 77
			"LD   A,B", // 78
			"LD   A,C", // 79
			"LD   A,D", // 7a
			"LD   A,E", // 7b
			"LD   A,H", // 7c
			"LD   A,L", // 7d
			"LD   A,(HL)", // 7e
			"LD   A,A", // 7f
			"ADD  A,B", // 80
			"ADD  A,C", // 81
			"ADD  A,D", // 82
			"ADD  A,E", // 83
			"ADD  A,H", // 84
			"ADD  A,L", // 85
			"ADD  A,(HL)", // 86
			"ADD  A,A", // 87
			"ADC  A,B", // 88
			"ADC  A,C", // 89
			"ADC  A,D", // 8a
			"ADC  A,E", // 8b
			"ADC  A,H", // 8c
			"ADC  A,L", // 8d
			"ADC  A,(HL)", // 8e
			"ADC  A,A", // 8f
			"SUB  B", // 90
			"SUB  C", // 91
			"SUB  D", // 92
			"SUB  E", // 93
			"SUB  H", // 94
			"SUB  L", // 95
			"SUB  (HL)", // 96
			"SUB  A", // 97
			"SBC  A,B", // 98
			"SBC  A,C", // 99
			"SBC  A,D", // 9a
			"SBC  A,E", // 9b
			"SBC  A,H", // 9c
			"SBC  A,L", // 9d
			"SBC  A,(HL)", // 9e
			"SBC  A,A", // 9f
			"AND  B", // a0
			"AND  C", // a1
			"AND  D", // a2
			"AND  E", // a3
			"AND  H", // a4
			"AND  L", // a5
			"AND  (HL)", // a6
			"AND  A", // a7
			"XOR  B", // a8
			"XOR  C", // a9
			"XOR  D", // aa
			"XOR  E", // ab
			"XOR  H", // ac
			"XOR  L", // ad
			"XOR  (HL)", // ae
			"XOR  A", // af
			"OR   B", // b0
			"OR   C", // b1
			"OR   D", // b2
			"OR   E", // b3
			"OR   H", // b4
			"OR   L", // b5
			"OR   (HL)", // b6
			"OR   A", // b7
			"CP   B", // b8
			"CP   C", // b9
			"CP   D", // ba
			"CP   E", // bb
			"CP   H", // bc
			"CP   L", // bd
			"CP   (HL)", // be
			"CP   A", // bf
			"RET  NZ", // c0
			"POP  BC", // c1
			"JP   NZ,a16", // c2
			"JP   a16", // c3
			"CALL NZ,a16", // c4
			"PUSH BC", // c5
			"ADD  A,d8", // c6
			"RST  00H", // c7
			"RET  Z", // c8
			"RET", // c9
			"JP   Z,a16", // ca
			"PREFIX CB", // cb
			"CALL Z,a16", // cc
			"CALL a16", // cd
			"ADC  A,d8", // ce
			"RST  08H", // cf
			"RET  NC", // d0
			"POP  DE", // d1
			"JP   NC,a16", // d2
			"???", // d3
			"CALL NC,a16", // d4
			"PUSH DE", // d5
			"SUB  d8", // d6
			"RST  10H", // d7
			"RET  C", // d8
			"RETI", // d9
			"JP   C,a16", // da
			"???", // db
			"CALL C,a16", // dc
			"???", // dd
			"SBC  A,d8", // de
			"RST  18H", // df
			"LDH  (a8),A", // e0
			"POP  HL", // e1
			"LD   (C),A", // e2
			"???", // e3
			"???", // e4
			"PUSH HL", // e5
			"AND  d8", // e6
			"RST  20H", // e7
			"ADD  SP,r8", // e8
			"JP   (HL)", // e9
			"LD   (a16),A", // ea
			"???", // eb
			"???", // ec
			"???", // ed
			"XOR  d8", // ee
			"RST  28H", // ef
			"LDH  A,(a8)", // f0
			"POP  AF", // f1
			"LD   A,(C)", // f2
			"DI", // f3
			"???", // f4
			"PUSH AF", // f5
			"OR   d8", // f6
			"RST  30H", // f7
			"LD   HL,SP+r8", // f8
			"LD   SP,HL", // f9
			"LD   A,(a16)", // fa
			"EI   ", // fb
			"???", // fc
			"???", // fd
			"CP   d8", // fe
			"RST  38H", // ff
		};

		public static string Disassemble(ushort addr, Func<ushort, byte> reader, out ushort size)
		{
			ushort origaddr = addr;
			List<byte> bytes = new List<byte>();
			bytes.Add(reader(addr++));

			string result = table[bytes[0]];

			if (result.Contains("d8"))
			{
				byte d = reader(addr++);
				bytes.Add(d);
				result = result.Replace("d8", string.Format("#{0:X2}h", d));
			}
			else if (result.Contains("d16"))
			{
				byte dlo = reader(addr++);
				byte dhi = reader(addr++);
				bytes.Add(dlo);
				bytes.Add(dhi);
				result = result.Replace("d16", string.Format("#{0:X2}{1:X2}h", dhi, dlo));
			}
			else if (result.Contains("a16"))
			{
				byte dlo = reader(addr++);
				byte dhi = reader(addr++);
				bytes.Add(dlo);
				bytes.Add(dhi);
				result = result.Replace("a16", string.Format("#{0:X2}{1:X2}h", dhi, dlo));
			}
			else if (result.Contains("a8"))
			{
				byte d = reader(addr++);
				bytes.Add(d);
				result = result.Replace("a8", string.Format("#FF{0:X2}h", d));
			}
			else if (result.Contains("r8"))
			{
				byte d = reader(addr++);
				bytes.Add(d);
				int offs = d;
				if (offs >= 128)
					offs -= 256;
				result = result.Replace("r8", string.Format("{0:X4}h", (ushort)(addr + offs)));
			}
			StringBuilder ret = new StringBuilder();
			ret.Append(string.Format("{0:X4}:  ", origaddr));
			foreach (var b in bytes)
				ret.Append(string.Format("{0:X2} ", b));
			while (ret.Length < 17)
				ret.Append(' ');
			ret.Append(result);
			size = (ushort)(addr - origaddr);
			return ret.ToString();
		}


		public string Cpu
		{
			get { return "MC6800"; }
			set { }
		}

		public string PCRegisterName
		{
			get { return "PC"; }
		}

		public IEnumerable<string> AvailableCpus
		{
			get { yield return "MC6800"; }
		}

		public string Disassemble(MemoryDomain m, uint addr, out int length)
		{
			int loc = (int)addr;
			ushort unused = 0;
			string ret = Disassemble((ushort)addr, a => m.PeekByte(a), out unused);
			length = loc - (int)addr;
			return ret;
		}
	}
}
