//var validExts = new HashSet<string> { ".jpg", ".gif", ".png", ".bmp" };
//var basicPath = @"\\195.250.145.25\disk_q\lmnet2\webapps\eduauthornew\";
//var courses = new string[] { "english1", "english2", "english3", "english4", "english5", "english1e", "english2e", "english3e", "english4e", "english5e", "german1", "german2", "german3", "italian1", "italian2", "italian3", "spanish1", "spanish2", "spanish3", "french1", "french2", "french3", "russian1", "russian2", "russian3" };
//var allImages = courses.SelectMany(crs => Directory.EnumerateFiles(basicPath + crs, "*.*", SearchOption.AllDirectories)).Where(fn => validExts.Contains(Path.GetExtension(fn))).Select(f => "/lm/oldea/" + f.Substring(basicPath.Length));
//File.WriteAllLines(@"d:\temp\images-disk.txt", allImages.OrderBy(s => s));
var disk = File.ReadAllLines(@"d:\temp\images-disk.txt").Select(f => f.Replace('\\', '/').ToLower()).ToDictionary(s => s, s => true);
var url = File.ReadAllLines(@"d:\temp\images.txt").Select(f => f.Replace('\\', '/').ToLower()).ToDictionary(s => s, s => true);
File.WriteAllLines(@"d:\temp\not-url.txt", disk.Keys.Where(f => !url.ContainsKey(f)).OrderBy(s => s));
      File.WriteAllLines(@"d:\temp\not-disk.txt", url.Keys.Where(f => !disk.ContainsKey(f)).OrderBy(s => s));



List<string> errors = new List<string>();
var files = Directory.EnumerateFiles(srcDir, "*.xml", SearchOption.AllDirectories).Select(f => f.ToLower()).Where(f => !f.EndsWith(@"\meta.xml")).ToArray();
var allLocData = initLocXml();
var images = new HashSet<string>();

//if (destPath != @"d:\rw\data\lm\oldea\english2\grammar\sec03\g02.tsx") continue;
var root = XElement.Load(fn);

          //images
          foreach (var el in root.DescendantsAndSelf("img")) images.Add(el.AttributeValue("src"));



        var url = File.ReadAllLines(@"d:\rw\design\convert-old-xml2tsx\images\images.txt");
File.WriteAllLines(@"D:\rw\design\convert-old-xml2tsx\images\images.cmd", new string[] { @"set srcdir=\\lmdata\p\alpha\rew\web4", @"set destdir=d:\rw\data-src", "" }.Concat(url.Select(f => {
  var fn = f.Replace('/', '\\');
  var fnDest = fn.Substring(0, fn.LastIndexOf('\\') + 1);
  return string.Format("xcopy %srcdir%{0} %destdir%{1} /Y", fn, fn); // fnDest);
})), Encoding.ASCII);
      return;