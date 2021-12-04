using System.Linq;
using System.Text;

public static class program {
	class color {
		public string Name;
		public UInt32 Value;
	}
	static string tabs(int count) => new string ('\t', count);
	static void process (int level, int tab, IEnumerable<IGrouping<char, color>> elts, StringBuilder chain) {
		if (elts.Count() > 1) {
			bool chainTest = false;
			if (chain.Length > 0) {
				if (chain.Length == 1)
					Console.WriteLine ($"{tabs(tab)}if (tolower(value[{level-chain.Length}]) == \'{chain[0]}\') {{//up");
				else
					Console.WriteLine ($"{tabs(tab)}if (!strncasecmp(&value[{level-chain.Length}],\"{chain.ToString()}\",{chain.Length})) {{//up");
				tab++;
				chain.Clear();
				chainTest = true;
			}
			Console.WriteLine ($"{tabs(tab)}switch(tolower(value[{level}])) {{");
			foreach (IGrouping<char, color> elt in elts) {
				Console.WriteLine ($"{tabs(tab)}case '{char.ToLower(elt.Key)}':");
				if (elt.Count() == 1) {
					Console.WriteLine ($"{tabs(tab+1)}return 0x{elt.First().Value:X8};//{elt.First().Name}");
					continue;
				}
				color ed = elt.FirstOrDefault (e=>e.Name.Length == level + 1);
				if (ed != null) {
					Console.WriteLine ($"{tabs(tab+1)}if (valLenght == {level + 1})");
					Console.WriteLine ($"{tabs(tab+2)}return 0x{ed.Value:X8};//{ed.Name}");
					process (level+1, tab + 1, elt.Where(el => el != ed). GroupBy (e=>e.Name[level+1]), chain);
				} else
					process (level+1, tab + 1, elt.GroupBy (e=>e.Name[level+1]), chain);
				Console.WriteLine ($"{tabs(tab+1)}break;");
			}
			Console.WriteLine ($"{tabs(tab)}}}");
			if (chainTest) {
				tab--;
				Console.WriteLine ($"{tabs(tab)}}} else");
				Console.WriteLine ($"{tabs(tab+1)}return 0;//UNKNOWN COLOR");
			}
		} else {
			IGrouping<char, color> elt = elts.First();
			if (elt.Count() == 1) {
				color c = elt.First();
				Console.WriteLine ($"{tabs(tab)}if (!strcasecmp(&value[{level}],\"{c.Name.Substring(level).ToLower()}\"))");
				Console.WriteLine ($"{tabs(tab+1)}return 0x{c.Value:X8};//{c.Name}");
				Console.WriteLine ($"{tabs(tab)}else");
				Console.WriteLine ($"{tabs(tab+1)}return 0;//UNKNOWN COLOR");
				return;
			}
			chain.Append (char.ToLower(elt.Key));
			color ed = elt.FirstOrDefault (e=>e.Name.Length == level + 1);
			if (ed != null) {
				if (chain.Length == 1)
					Console.WriteLine ($"{tabs(tab)}if (tolower(value[{level-chain.Length+1}]) == \'{chain[0]}\') {{//down");
				else
					Console.WriteLine ($"{tabs(tab)}if (!strncasecmp(&value[{level-chain.Length+1}],\"{chain.ToString()}\",{chain.Length})) {{//down");
				Console.WriteLine ($"{tabs(tab+1)}if (valLenght == {level + 1})");
				Console.WriteLine ($"{tabs(tab+2)}return 0x{ed.Value:X8};//{ed.Name}");
				chain.Clear();
				process (level+1, tab + 1, elt.Where(el => el != ed). GroupBy (e=>e.Name[level+1]), chain);
				Console.WriteLine ($"{tabs(tab)}}}");
			} else
				process (level+1, tab, elt.GroupBy (e=>e.Name[level+1]), chain);
		}
		/*foreach (IGrouping<char, color> elt in elts) {
			Console.WriteLine ($"{new string(' ', level)} {level}: {elt.Key}");
			if (elt.Count() == 1) {
				Console.WriteLine ($"{new string(' ', level + 1)} {level} -> {elt.First().Name}");
				continue;
			}
			color ed = elt.FirstOrDefault (e=>e.Name.Length == level + 1);
			if (ed != null) {
				Console.WriteLine ($"{new string(' ', level + 1)} {level} -> {ed.Name}");
				process (level+1, elt.Where(el => el != ed). GroupBy (e=>e.Name[level+1]));
			} else
				process (level+1, elt.GroupBy (e=>e.Name[level+1]));
		}*/

	}
	public static void Main () {
		List<color> colors = new List<color>(1000);
	// See https://aka.ms/new-console-template for more information
		using (Stream s = new FileStream ("../x11-colors.csv", FileMode.Open)) {
			using (StreamReader sr = new StreamReader (s)) {
				while (!sr.EndOfStream) {
					string l = sr.ReadLine ();
					string[] tmp = l.Split(',');
					string cv = $"ff{tmp[1].AsSpan (5,2)}{tmp[1].AsSpan (3,2)}{tmp[1].AsSpan (1,2)}";
					colors.Add (new color () {
						Name = tmp[0],
						Value = UInt32.Parse (cv, System.Globalization.NumberStyles.HexNumber)
					});
				}
			}
		}
		process (0,0, colors.OrderBy (c=>c.Name).GroupBy (e=>e.Name[0]), new StringBuilder());
	}
}

