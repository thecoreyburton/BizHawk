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
			"NEG A", // 40
			"???", // 41
			"???", // 42
			"COM A", // 43
			"LSR A", // 44
			"???", // 45
			"ROR A", // 46
			"ASR A", // 47
			"ASL A", // 48
			"ROL A", // 49
			"DEC A", // 4a
			"???", // 4b
			"INC A", // 4c
			"TST A", // 4d
			"???", // 4e
			"CLR A", // 4f
			"NEG B", // 50
			"???", // 51
			"???", // 52
			"COM B", // 53
			"LSR B", // 54
			"???", // 55
			"ROR B", // 56
			"ASR B", // 57
			"ASL B", // 58
			"ROL B", // 59
			"DEC B", // 5a
			"???", // 5b
			"INC B", // 5c
			"TST B", // 5d
			"???", // 5e
			"CLR B", // 5f
			"NEG (Ix + n)", // 60
			"???", // 61
			"???", // 62
			"COM (Ix + n)", // 63
			"LSR (Ix + n)", // 64
			"???", // 65
			"ROR (Ix + n)", // 66
			"ASR (Ix + n)", // 67
			"ASL (Ix + n)", // 68
			"ROL (Ix + n)", // 69
			"DEC (Ix + n)", // 6a
			"???", // 6b
			"INC (Ix + n)", // 6c
			"TST (Ix + n)", // 6d
			"JMP (Ix + n)", // 6e
			"CLR (Ix + n)", // 6f
			"NEG (nn)", // 70
			"???", // 71
			"???", // 72
			"COM (nn)", // 73
			"LSR (nn)", // 74
			"???", // 75
			"ROR (nn)", // 76
			"ASR (nn)", // 77
			"ASL (nn)", // 78
			"ROL (nn)", // 79
			"DEC (nn)", // 7a
			"???", // 7b
			"INC (nn)", // 7c
			"TST (nn)", // 7d
			"JMP (nn)", // 7e
			"CLR (nn)", // 7f
			"SUB  A,n", // 80
			"CMP  A,n", // 81
			"SBC  A,n", // 82
			"???", // 83
			"AND  A,n", // 84
			"BIT  A,n", // 85
			"LDA  A,n", // 86
			"STA  A,n", // 87
			"EOR  A,n", // 88
			"ADC  A,n", // 89
			"ORA  A,n", // 8a
			"ADD  A,n", // 8b
			"CPX  Ix,nn", // 8c
			"BSR  n", // 8d
			"LDS  nn", // 8e
			"STS  nn", // 8f
			"SUB  A,(n)", // 90
			"CMP  A,(n)", // 91
			"SBC  A,(n)", // 92
			"???", // 93
			"AND  A,(n)", // 94
			"BIT  A,(n)", // 95
			"LDA  A,(n)", // 96
			"STA  A,(n)", // 97
			"EOR  A,(n)", // 98
			"ADC  A,(n)", // 99
			"ORA  A,(n)", // 9a
			"ADD  A,(n)", // 9b
			"CPX  Ix,(n)", // 9c
			"???", // 9d
			"LDS  (n)", // 9e
			"STS  (n)", // 9f
			"SUB  A,(Ix + n)", // a0
			"CMP  A,(Ix + n)", // a1
			"SBC  A,(Ix + n)", // a2
			"???", // a3
			"AND  A,(Ix + n)", // a4
			"BIT  A,(Ix + n)", // a5
			"LDA  A,(Ix + n)", // a6
			"STA  A,(Ix + n)", // a7
			"EOR  A,(Ix + n)", // a8
			"ADC  A,(Ix + n)", // a9
			"ORA  A,(Ix + n)", // aa
			"ADD  A,(Ix + n)", // ab
			"CPX  Ix,(Ix + n)", // ac
			"JSR  (Ix + n)", // ad
			"LDS  (Ix + n)", // ae
			"STS  (Ix + n)", // af
			"SUB  A,(nn)", // b0
			"CMP  A,(nn)", // b1
			"SBC  A,(nn)", // b2
			"???", // b3
			"AND  A,(nn)", // b4
			"BIT  A,(nn)", // b5
			"LDA  A,(nn)", // b6
			"STA  A,(nn)", // b7
			"EOR  A,(nn)", // b8
			"ADC  A,(nn)", // b9
			"ORA  A,(nn)", // ba
			"ADD  A,(nn)", // bb
			"CPX  Ix,(nn)", // bc
			"JSR  (nn)", // bd
			"LDS  (nn)", // be
			"STS  (nn)", // bf
			"SUB  B,n", // c0
			"CMP  B,n", // c1
			"SBC  B,n", // c2
			"???", // c3
			"AND  B,n", // c4
			"BIT  B,n", // c5
			"LDA  B,n", // c6
			"STA  B,n", // c7
			"EOR  B,n", // c8
			"ADC  B,n", // c9
			"ORA  B,n", // ca
			"ADD  B,n", // cb
			"???", // cc
			"???", // cd
			"LDIx  nn", // ce
			"STIx  nn", // cf
			"SUB  B,(n)", // d0
			"CMP  B,(n)", // d1
			"SBC  B,(n)", // d2
			"???", // d3
			"AND  B,(n)", // d4
			"BIT  B,(n)", // d5
			"LDA  B,(n)", // d6
			"STA  B,(n)", // d7
			"EOR  B,(n)", // d8
			"ADC  B,(n)", // d9
			"ORA  B,(n)", // da
			"ADD  B,(n)", // db
			"???", // dc
			"???", // dd
			"LDIx  (n)", // de
			"STIx  (n)", // df
			"SUB  B,(Ix + n)", // e0
			"CMP  B,(Ix + n)", // e1
			"SBC  B,(Ix + n)", // e2
			"???", // e3
			"AND  B,(Ix + n)", // e4
			"BIT  B,(Ix + n)", // e5
			"LDA  B,(Ix + n)", // e6
			"STA  B,(Ix + n)", // e7
			"EOR  B,(Ix + n)", // e8
			"ADC  B,(Ix + n)", // e9
			"ORA  B,(Ix + n)", // ea
			"ADD  B,(Ix + n)", // eb
			"???", // ec
			"???", // ed
			"LDIx  (Ix + n)", // ee
			"STIx  (Ix + n)", // ef
			"SUB  B,(nn)", // f0
			"CMP  B,(nn)", // f1
			"SBC  B,(nn)", // f2
			"???", // f3
			"AND  B,(nn)", // f4
			"BIT  B,(nn)", // f5
			"LDA  B,(nn)", // f6
			"STA  B,(nn)", // f7
			"EOR  B,(nn)", // f8
			"ADC  B,(nn)", // f9
			"ORA  B,(nn)", // fa
			"ADD  B,(nn)", // fb
			"???", // fc
			"???", // fd
			"LDIx  (nn)", // fe
			"STIx  (nn)", // ff
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
