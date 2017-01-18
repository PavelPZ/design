using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace data_old.code {
    public partial class Index : System.Web.UI.Page {
    //Rucni zasah: 
    // D:\rw\data\lm\oldea\german2\les3\chapc\t2a_kb_l3_c1b.tsx, sound_sentences
    // D:\rw\data\lm\oldea\russian1\lesson6\chaptera\istorija.tsx, <PairingItem right={'Юpий Гaгapин – '+ $l(l.ttrans3)}>1961</PairingItem>
    protected void Page_Load(object sender, EventArgs e) {
      //generateTsConfigs(); return;
      xmlToTsx.convert.toTsxDir(@"d:\rw\data-src\lm\oldea", @"d:\rw\data\lm\oldea", false);
      xmlToTsx.convert.toTsxDir(@"d:\rw\data-src\instr", @"d:\rw\rw\rw-instr", true);
      //xmlToTsx.convert.toTsxDir(@"D:\rw\rw\rw-course\examples", @"D:\rw\rw\rw-course\examples", false);
    }

    static void generateTsConfigs() {
      var cfgFn = @"d:\rw\data\tsc-build\tsc-{0}.json";
      var cfg = File.ReadAllText(@"d:\rw\data\tsc-{0}.json");
      var cmd = new List<string>() { @"del d:\rw\data\tsc-build\build.log", @"d:", @"cd d:\rw\data\lm\oldea\", @"del /S *.js", };
      foreach (var crs in new string[] { "english1", "english2", "english3", "english4", "english5", "english1e", "english2e", "english3e", "english4e", "english5e", "german1", "german2", "german3", "italian1", "italian2", "italian3", "spanish1", "spanish2", "spanish3", "french1", "french2", "french3", "russian1", "russian2", "russian3" }) {
        File.WriteAllText(string.Format(cfgFn, crs), string.Format(cfg, crs));
        cmd.Add(string.Format(@"call tsc --p D:\rw\data\tsc-build\tsc-{0}.json >> d:\rw\data\tsc-build\build.log", crs));
      }
      File.WriteAllLines(@"d:\rw\data\tsc-build\build.cmd", cmd, Encoding.ASCII);
    }

  }
}