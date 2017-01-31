//TODO:
// - remove Body.externals attribute
// - remove tag with id="alwaysVisiblePanel"
//D:\LMCom\rew\DesignTimeLib\DesignNew\XmlToTsx.cs port
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web;
using System;

namespace xmlToTsx {
  public static class convert {

    public class context {
      public bool isInstr; //generace instrukci
      public string titleLocKey; //existuje title k lokalizac. Jeho KEY
      public string noLocTitle; //titulek neni lokalizovan - jeho text
      public string url; //URL, napr. '/lm/oldea/english1/xxx' 
      public string name; //jmeno souboru, napr. xxx
      public bool needsMeta; //body tag a neni to instrukce
      public bool needsLoc; //existuje cosi k lokalizaci
      public bool needsLocInMeta; //je potreba importovat lokalizaci do meta souboru: iff titulek je lokalizovan
      public bool needsLocInPage; //je potreba importovat lokalizaci do page souboru: iff je lokalizovano neco mimo titulku
    }

    public static void genSiteMap() {
      var xml = XElement.Load(@"D:\rw\design\convert-old-xml2tsx\products.xml");
      var roots = xml.Elements().SelectMany(e => e.Elements().SelectMany(ell => ell.Elements())).ToArray();
      var sitemapLocs = initLocXml()["sitemap"];
      var oldea = new List<string>();
      foreach (var root in roots) {
        var fn = root.AttributeValue("url").Replace("/lm/oldea/", null); fn = fn.Substring(0, fn.Length - 1).Replace('/', '-');
        var line = fn.Split(new char[] { '_', '-' })[0];
        oldea.Add(string.Format("lm/oldea/{0}/{1}", line, fn));
        var sitemapDir = @"d:\rw\data\lm\oldea\" + line + "\\";
        var ctx = new context() { name = fn };
        // localization
        var toLoc = new Dictionary<string, string>();
        var titles = root.DescendantsAndSelf().SelectMany(el => el.Attributes()).Where(a => a.Name.LocalName == "title").ToArray();
        foreach (var title in titles) localizeAttr(title, toLoc, ctx, true);
        if (ctx.needsLoc) File.WriteAllText(sitemapDir + ctx.name + ".loc.ts", genLocTS(toLoc, sitemapLocs, null));
        //sitemap
        var locLine = ctx.needsLoc ? string.Format("import ll from './{0}.loc';\r\n", ctx.name) : null;
        //var importLineLoc = "import { IMetaNode } from 'rw-course'; import { $l, toGlobId } from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const l = ll[globId];\r\n";
        //var importLineNoLoc = "import { IMetaNode } from 'rw-course';\r\n";
        var importLine = !ctx.needsLoc ?  null : "import { $l, toGlobId } from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const l = ll[globId];\r\n";
        //var importLine = ctx.needsLoc ? importLineLoc : importLineNoLoc;
        var importEx = "import {0} from '{1}.meta';\r\n";
        var exportLine = "export default sitemap;";
        var cnt = 0;
        var exs = root.DescendantsAndSelf().Where(el => el.AttributeValue("type") == "ex").Select(el => el.AttributeValue("url")).Where(url => correctSitemaUrl(url)).ToDictionary(url => url, url => "ex$" + cnt++);
        var imports = exs.Select(kv => string.Format(importEx, kv.Value, kv.Key.Substring(1))).DefaultIfEmpty().Aggregate((r, i) => r + i);
        var sb = new StringBuilder();
        genSitemapCode(sb, root, exs);
        var code = "const sitemap: DCourse.IMetaNode = " + sb.ToString() + ";\r\n";
        var res = locLine + importLine + imports + code + exportLine;
        File.WriteAllText(sitemapDir + ctx.name + ".ts", res);
      }
      var allcnt = 0;
      var oldEADir = oldea.ToDictionary(url => url, url => "root$" + allcnt++);
      var allImport = "import {0} from '{1}';\r\n";
      allImport = oldEADir.Select(kv => string.Format(allImport, kv.Value, kv.Key)).DefaultIfEmpty().Aggregate((r, i) => r + i);
      var all1 = "const allOldEA = { childs: [" + oldEADir.Values.Aggregate((r,i) => r + ", " + i) + "]};\r\n";
      var all2 = "export default allOldEA;";
      var all = allImport + all1 + all2;
      File.WriteAllText(@"d:\rw\data\lm\oldea\index.ts", all);
    }
    static void genSitemapCode(StringBuilder sb, XElement root, Dictionary<string, string> exs) {
      var type = root.AttributeValue("type"); var url = root.AttributeValue("url");
      if (type == "ex") { sb.Append(exs[url]); return; }
      if (root.Parent.AttributeValue("type") == "grammarRoot") type = "modGrammar";
      var title = root.AttributeValue("title");
      if (!title.StartsWith("@")) title = "'" + HttpUtility.JavaScriptStringEncode(title) + "'"; else title = title.Substring(2, title.Length - 4);
      sb.AppendFormat("{{ title: {0}, url: '{1}', {2}childs: [", title, url, type==null ? null : string.Format("flag: '{0}', ", type)); //sb.Append(title); sb.Append(", url:'"); sb.Append(url); sb.AppendLine("', childs: [");
      var first = true;
      foreach (var el in root.Elements()) {
        if (!correctSitemaUrl(el.AttributeValue("url"))) continue;
        if (first) first = false; else sb.AppendLine(",");
        genSitemapCode(sb, el, exs);
      }
      sb.Append("]}");
    }

    static bool correctSitemaUrl(string url) { return url.IndexOf("t1a_ab_l6_b2c1") < 0 && url.IndexOf("t1b_ab_l9_g6a") < 0 && url.IndexOf("t2b_ab_l10_e5") < 0 && url.IndexOf("t2b_ab_l11_a6") < 0 && url.IndexOf("novyeslova") < 0 && url.IndexOf("russian3/lesson2/chaptera/pocemu") < 0;  }

    public static void toTsxDir(string srcDir, string destDir, bool isInstr) {
      var files = Directory.EnumerateFiles(srcDir, "*.xml", SearchOption.AllDirectories).Select(f => f.ToLower()).Where(f => !f.EndsWith(@"\meta.xml")).ToArray();
      var allLocData = initLocXml();
      var errors = new List<string>();

      foreach (var fn in files) {
        if (fn.IndexOf("novyeslova") >= 0) //missing localization of used words
          continue;
        var relPath = fn.Substring(srcDir.Length);
        var destPath = destDir + relPath.Replace(".xml", ".tsx");
        var destLocPath = destDir + relPath.Replace(".xml", ".loc.ts");
        var destMetaPath = destDir + relPath.Replace(".xml", ".meta.ts");
        try {
          //if (destPath != @"d:\rw\data\lm\oldea\english2\grammar\sec03\g02.tsx") continue;
          var root = XElement.Load(fn);

          var ctx = new context { isInstr = isInstr, name = Path.GetFileNameWithoutExtension(fn) };

          //page TSX
          var toLoc = new Dictionary<string, string>();
          //string titleLocKey;
          var s = genPageTSX(root, ctx, toLoc); //, out url, out titleLocKey);
          lib.AdjustFileDir(destPath); File.WriteAllText(destPath, s);

          //loc TS
          if (isInstr) ctx.url = "/instrs";
          if (ctx.needsLoc) {
            var url = ctx.url.Substring(1).Replace('/', '-');
            var locData = allLocData.ContainsKey(url) ? allLocData[url] : null;
            s = genLocTS(toLoc, locData, ctx.isInstr ? null : allLocData["sitemap"]);
            File.WriteAllText(destLocPath, s);
          }
          if (ctx.needsMeta) {
            s = genMetaTS(ctx);
            File.WriteAllText(destMetaPath, s);
          }
        } catch (Exception exp) {
          //File.WriteAllText(destPath, exp.Message);
          errors.Add(destPath + " " + exp.Message);
        }
      }
      //File.WriteAllLines(@"d:\temp\images.txt", images.OrderBy(s => s));
      if (errors.Count > 0) throw new Exception(errors.Aggregate((r, i) => r + "\r\n" + i));
    }

    static string prefixDot(string url) {
      return url.StartsWith(".") ? url : "./" + url;
    }

    static string relImageUrl(string url, string imageUrl) {
      var modName = prefixDot(VirtualPathUtility.MakeRelative(url, imageUrl));
      var idx = modName.LastIndexOf('.');
      return modName.Substring(0, idx) + "." + modName.Substring(idx + 1);
    }

    static string genMetaTS(context ctx) {
      var line1 = ctx.needsLocInMeta ? "import ll from './{0}.loc';\r\n" : null;
      //var line2Loc = "import {{ IMetaNode }} from 'rw-course'; import {{ $l, toGlobId }} from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const l = ll[globId]; const meta: IMetaNode = {{ title: $l(l.{0}), url: globId }}; export default meta;\r\n";
      //var line2NoLoc = "import {{ IMetaNode }} from 'rw-course'; import {{ toGlobId }} from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const meta: IMetaNode = {{ title: '{0}', url: globId, flag: 'ex' }}; export default meta;\r\n";
      var line2Loc = "import {{ $l, toGlobId }} from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const l = ll[globId]; const meta: DCourse.IMetaNode = {{ title: $l(l.{0}), url: globId }}; export default meta;\r\n";
      var line2NoLoc = "import {{ toGlobId }} from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName); const meta: DCourse.IMetaNode = {{ title: '{0}', url: globId, flag: 'ex' }}; export default meta;\r\n";
      line1 = line1 == null ? null : string.Format(line1, ctx.name);
      var line2 = ctx.needsLocInMeta ? string.Format(line2Loc, ctx.titleLocKey) : string.Format(line2NoLoc, HttpUtility.JavaScriptStringEncode(ctx.noLocTitle));
      var res = line1 + line2;
      return res;
    }


    static string genLocTS(Dictionary<string, string> toLoc, Dictionary<string, Dictionary<string, string>> locData, Dictionary<string, Dictionary<string, string>> sitemapLoc) {
      var line1 = "import { toGlobId } from 'rw-lib/loc'; declare const __moduleName: string; const globId = toGlobId(__moduleName);\r\n";
      var itemStart = "const {0}: DLoc.ILocItem = {{\r\n";
      var item = "  '{0}': '{1}',\r\n";
      var itemEnd = "};\r\n";
      var lineRes = "const res = {{ [globId]: {{ {0} }} }};\r\n";
      var lineLast = "export default res;";

      Func<string, string, string> genItem = (Key, Value) => string.Format(item, Key, HttpUtility.JavaScriptStringEncode(Value));
      var items = toLoc.Select(e => {
        //var itStart = string.Format(itemStart, e.Key == titleLocKey ? "title" : e.Key);
        var itStart = string.Format(itemStart, e.Key);
        var val = locData != null && locData.ContainsKey(e.Key) ? locData[e.Key] : (sitemapLoc != null && sitemapLoc.ContainsKey(e.Key) ? sitemapLoc[e.Key] : null);
        var it = (val == null ? genItem("en-gb", e.Value) : val.Where(ee => ee.Key != "vi-vi").Select(ee => genItem(ee.Key, ee.Value)).Aggregate((r, i) => r + i));
        return itStart + it + itemEnd;
      }).DefaultIfEmpty().Aggregate((r, i) => r + i);

      lineRes = string.Format(lineRes, toLoc.Keys.Aggregate((r, i) => r + ", " + i));

      var res = line1 + items + lineRes + lineLast;
      return res;

    }

    static Dictionary<string, Dictionary<string, Dictionary<string, string>>> initLocXml() {
      var res = XElement.Load(@"d:\rw\data-src\ea.loc").Elements().ToDictionary(e => e.Name.LocalName == "sitemap" ? "sitemap" : "lm-oldea-" + e.Name.LocalName, e => e.Elements().ToDictionary(ee => ee.Name.LocalName, ee => ee.Elements().ToDictionary(eee => eee.Name.LocalName, eee => eee.Value)));
      res.Add("instrs", initInstrLocXml());
      return res;
    }
    static Dictionary<string, Dictionary<string, string>> initInstrLocXml() {
      var instrLocs = new Dictionary<string, Dictionary<string, string>>();
      var instrs = XElement.Load(@"d:\rw\data-src\instr.loc");
      foreach (var lngEl in instrs.Elements()) foreach (var nameEl in lngEl.Elements()) {
          if (nameEl.Value == "TODO") continue;
          var name = nameEl.Attribute("lang").Value.ToLower();
          if (!instrLocs.ContainsKey(name)) instrLocs[name] = new Dictionary<string, string>();
          var nm = instrLocs[name];
          nm[lngEl.Name.LocalName.Replace('_', '-')] = nameEl.Value;
        }
      return instrLocs;
    }

    static string genPageTSX(XElement root, context ctx, Dictionary<string, string> toLoc) { //, out string url, out string titleLocKey) {
      var body = root.Element("body");
      HashSet<string> allCrsTags = new HashSet<string>();
      var images = new Dictionary<string, string>();
      if (body == null) body = root;
      else {
        ctx.url = body.AttributeValue("url");
        if (!ctx.isInstr) ctx.needsMeta = true;
        lib.normalizeXml(root, blankCode);
        var temp = root.Element("head"); if (temp != null) temp = temp.Element("title");
        if (temp != null) body.Add(new XAttribute("title", temp.Value));
        //27.12.16: 
        var attr = body.Attribute("externals"); if (attr != null) attr.Remove();
      }
      //remove tag with id="alwaysVisiblePanel"
      foreach (var el in body.DescendantsAndSelf().OfType<XElement>().Where(e => e.AttributeValue("id") == "alwaysVisiblePanel").ToArray()) el.Remove();

      foreach (var el in body.DescendantsAndSelf()) {
        string tagName; bool isTag;
        if (el.Name.LocalName == "img") {
          var src = el.Attribute("src");
          if (src == null) continue;
          src.Remove();
          el.Name = "Img";
          var imgUrl = relImageUrl(ctx.url, src.Value.ToLower());
          string imgId;
          if (!images.TryGetValue(imgUrl, out imgId)) images[imgUrl] = imgId = "$img_" + images.Count();
          el.Add(new XAttribute("imgData", "@{" + imgId + "}@"));
          tagName = "Img";
          isTag = true;
        } else {
          tagName = lib.toCammelCase(el.Name.LocalName);
          isTag = tags.Contains(tagName);
        }
        if (isTag) {
          el.Name = lib.typeName(tagName);
          if (el.Name == "Body") el.Name = "Page";
          allCrsTags.Add(el.Name.LocalName);
        }
        //uprava atributu
        foreach (var attr in el.Attributes().ToArray()) {
          var oldName = attr.Name.LocalName;
          //all
          switch (oldName) {
            case "class": renameAttr(attr, "className"); break;
            case "colspan": wrapExpression(renameAttr(attr, "colSpan")); break;
            case "rowspan": wrapExpression(renameAttr(attr, "rowSpan")); break;
            case "maxlength": wrapExpression(renameAttr(attr, "maxLength")); break;
            case "style": wrapExpression(attr, parseStyle(attr.Value)); break;
            case "disabled": wrapExpression(attr, "true"); break;
          }
          //course components only
          if (!isTag) continue;
          if (oldName == "order") attr.Remove();
          if (oldName == "url") attr.Remove();
          var newName = lib.toCammelCase(oldName);
          var fullName = tagName + "." + newName;
          var newAttr = newName != oldName ? renameAttr(attr, newName) : attr;
          if (numProps.Contains(newName) || numProps.Contains(fullName)) wrapExpression(newAttr);
          if (boolProps.Contains(newName) || boolProps.Contains(fullName)) wrapExpression(newAttr);
          string enumType;
          if (enumProps.TryGetValue(newName, out enumType) || enumProps.TryGetValue(fullName, out enumType)) {
            newAttr.Value = lib.toCammelCase(newAttr.Value);
            //wrapExpression(newAttr, enumType + "." + lib.toCammelCase(newAttr.Value));
            //allCrsTags.Add(enumType);
          }
          if (newName == "evalGroup") {
            var parts = newAttr.Value.Split('-');
            if (parts.Length > 1) {
              if (parts.Contains("and")) el.Add(new XAttribute("evalAnd", "@{true}@"));
              if (parts.Contains("exchangeable")) el.Add(new XAttribute("evalExchangeable", "@{true}@"));
              newAttr.Value = parts.First(v => v != "and" && v != "exchangeable");
            }
          }
        }
      }

      //start: collapse CDATA do cdata atributu
      List<string> cdatas = new List<string>();
      foreach (var cd in body.DescendantNodes().OfType<XCData>().ToArray()) {
        cdatas.Add(cd.Value);
        cd.Parent.Add(new XAttribute("cdata", "~{" + (cdatas.Count - 1).ToString() + "}~"));
        cd.Remove();
      }

      //lokalizace textu
      foreach (var el in body.DescendantNodes().OfType<XText>()) el.Value = localizeForTsx(el.Value, true, toLoc, ctx, false);

      //localize attributes
      foreach (var att in body.Descendants("PairingItem").Select(e => e.Attribute("right"))) localizeAttr(att, toLoc, ctx, false);
      localizeAttr(body.Attribute("instrTitle"), toLoc, ctx, false);
      localizeAttr(body.Attribute("title"), toLoc, ctx, true);

      //napln priznaky dle vysledku lokalizace
      if (ctx.isInstr) {
        if (toLoc.Count > 0) ctx.needsLocInPage = true;
      } else {
        if (ctx.titleLocKey != null) ctx.needsLocInMeta = true; //je lokalizovan titulek => je potreba include loc do meta souboru
        if (ctx.titleLocKey == null && toLoc.Count > 0 || ctx.titleLocKey != null && toLoc.Count > 1) ctx.needsLocInPage = true; //je lokalizovano i neco mimo titulku => je potreba include loc do Page souboru
      }

      //instructions
      string[] instrs = null;
      var instrStr = body.AttributeValue("instrBody");
      if (!string.IsNullOrEmpty(instrStr)) {
        instrs = instrStr.Split('|').Select(s => s.Trim()).ToArray();
        body.Attribute("instrBody").Value = "@{" + (instrs.Length == 1 ? instrs[0] : "[" + instrs.Aggregate((r, i) => r + ", " + i) + "]") + "}@";
      }

      //see also
      string[] seeAlso = null;
      var seeAlsoStr = body.AttributeValue("seeAlsoStr");
      if (!string.IsNullOrEmpty(seeAlsoStr)) {
        seeAlso = seeAlsoStr.Split('#').Select(s => s.Split('|')[0]).Distinct().ToArray();
        body.Attribute("seeAlsoStr").Value = "@{" + (seeAlso.Length == 1 ? "$see_0" : "[" + seeAlso.Select((s, idx) => "$see_" + idx.ToString()).Aggregate((r, i) => r + ", " + i) + "]") + "}@";
      }

      //vyhod XML namespace
      foreach (var attr in body.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.LocalName == "noNamespaceSchemaLocation").ToArray()) attr.Remove();

      //xml to string
      StringBuilder sb = new StringBuilder();
      using (var wr = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true })) {//, NewLineHandling = NewLineHandling.Replace, NewLineChars = "\n" });
        body.Save(wr); wr.Flush();
      }

      //finish: enum, bool, number value from string}
      sb.Replace("=\"@{true}@\"", null).Replace("\"@{", "{").Replace("}@\"", "}");
      //finish: expand CDATA in cdata={``} atributu
      var res = sb.ToString(); sb = null;
      foreach (var m in lib.regExItem.Parse(res, cdataRx)) {
        if (sb == null) sb = new StringBuilder();
        if (!m.IsMatch) { sb.Append(m.Value); continue; }
        var cd = cdatas[int.Parse(m.Value.Substring(3, m.Value.Length - 6))];
        sb.Append("{`"); sb.Append(cd.Replace("\\", "\\\\")); sb.Append("`}");
      }
      if (sb != null) res = sb.ToString();

      var line1 = "import {{ {0} }} from 'rw-course';\r\n";
      var line2 = ctx.needsLocInPage ? "import ll from './{0}.loc';\r\n" : null;
      var line3 = ctx.needsMeta ? "import meta from './{0}.meta';\r\n" : null;
      var lineInstr = "import {0} from 'rw-instr/{0}';\r\n";
      var lineSeeAlso = "import {0} from '{1}';\r\n";
      var lineImg = "import {0} from '{1}';\r\n";
      //var lineConst = "import React from 'react'; import { $l, toGlobId } from 'rw-lib/loc'; declare const __moduleName: string; const l = ll[toGlobId(__moduleName)]; export default () => \r\n\r\n";
      var lineConstLoc = "import React from 'react'; import { $l } from 'rw-lib/loc'; const l = ll[meta.url]; export default () => \r\n\r\n";
      var lineConstLocNoMeta = "import React from 'react'; import { $l, toGlobId } from 'rw-lib/loc'; declare const __moduleName: string; const l = ll[toGlobId(__moduleName)]; export default () => \r\n\r\n";
      var lineConstNoLoc = "import React from 'react'; export default () => \r\n\r\n";
      var lineConst = ctx.needsLocInPage ? (ctx.needsMeta ? lineConstLoc : lineConstLocNoMeta) : lineConstNoLoc;

      line1 = string.Format(line1, allCrsTags.Aggregate((r, i) => r + ", " + i));
      line2 = line2 == null ? null : string.Format(line2, ctx.name);
      line3 = line3 == null ? null : string.Format(line3, ctx.name);
      lineInstr = instrs == null ? null : instrs.Select(ins => string.Format(lineInstr, ins)).Aggregate((r, i) => r + " " + i);
      lineImg = images.Select(kv => string.Format(lineImg, kv.Value, kv.Key)).DefaultIfEmpty().Aggregate((r, i) => r + "" + i);
      lineSeeAlso = seeAlso == null ? null : seeAlso.Select((url, idx) => {
        var pg = ctx.url.Replace("/lm/oldea/", "/"); var crs = pg.Split('/')[1];
        var s = "/" + crs + url.Replace("/lm/oldea/", "/");
        var su = string.Format(lineSeeAlso, "$see_" + idx, prefixDot(VirtualPathUtility.MakeRelative(pg, s + ".meta")));
        return su;
      }).DefaultIfEmpty().Aggregate((r, i) => r + "" + i);

      res = line1 + line2 + line3 + lineInstr + lineImg + lineSeeAlso + lineConst + res;
      return res;
      //      //prefix:
      //      string prefix = @"import React from 'react'; import course, {{{0}}} from 'rw-course'; import {{ $l }} from 'rw-lib/loc'; import l from './{1}-loc'; {2} {3} export default () => /*
      //*********** START MARKUP HERE: */
      //";
      //      prefix = string.Format(prefix, allCrsTags.Aggregate((r, i) => r + ", " + i), name, instrStr, imagesScript);
      //      //return
      //      return prefix + res;
    }
    const string blankCode = "~blank~";
    static Regex cdataRx = new Regex("\"~\\{\\d+\\}~\"");
    static XAttribute renameAttr(XAttribute attr, string newName) {
      var res = new XAttribute(newName, attr.Value);
      attr.Parent.Add(res);
      attr.Remove();
      return res;
    }
    static HashSet<string> numProps = new HashSet<string>(new string[] { "width", "scoreWeight", "limitRecommend", "limitMin", "limitMax", "numberOfRows", "begPos", "endPos", "numberOfPhrases", "phraseIdx"/*, "order"*/ });
    static HashSet<string> boolProps = new HashSet<string>(new string[] { "isPassive", "scoreAsRatio", "gapFillLike", "caseSensitive",
      "readOnly", "skipEvaluation", "leftRandom", "recordInDialog", "singleAttempt", "isStriped", "hidden", "passive", "correct", "random", "isOldToNew",
      "radioButton.correctValue","radioButton.initValue","checkItem.correctValue","checkBox.correctValue"
    });
    static Dictionary<string, string> enumProps = new Dictionary<string, string> {
      {"textType","CheckItemTexts" },{"checkItem.initValue","threeStateBool" },{"checkBox.initValue","threeStateBool" },{"leftWidth","pairingLeftWidth" },{"dialogSize","modalSize" },{"icon","listIcon" },
      { "color","colors" },{"actorId","IconIds" },{"textId","CheckItemTexts" },
      { "macroTable.inlineType","inlineControlTypes" },{ "macroList.inlineType","inlineControlTypes" },{ "inlineTag.inlineType","inlineElementTypes" },{"smartElement.inlineType","smartElementTypes" },
      {"offering.mode","offeringDropDownMode" },{"smartOffering.mode","smartOfferingMode" },
    };
    static HashSet<string> tags = new HashSet<string>(lib.allTypes);

    //mj. nahradi '{' v textu by {'{'} 
    static string localizeForTsx(string text, bool plainText, Dictionary<string, string> toLoc, context ctx, bool isTitle) {
      StringBuilder sb = null;
      foreach (var m in lib.regExItem.Parse(text, lib.localizePartsRegex)) {
        if (sb == null) sb = new StringBuilder();
        if (!m.IsMatch) { sb.Append(replaceBrackets(m.Value).Replace(blankCode, "{' '}")); continue; }
        var parts = m.Value.Substring(2, m.Value.Length - 4).Split('|');

        var parts0 = normalizeTradosName(parts[0]);
        toLoc[parts0] = parts[1];
        ctx.needsLoc = true;
        if (isTitle) ctx.titleLocKey = parts0;
        var txt = string.Format("{{$l(l.{0})}}", parts0);
        //var txt = string.Format("{{$loc('{0}','{1}')}}", parts[0], HttpUtility.JavaScriptStringEncode(parts[1]));
        sb.Append(plainText ? txt : "@" + txt + "@");
      }
      if (isTitle && ctx.titleLocKey == null) ctx.noLocTitle = text;
      return sb == null ? text : sb.ToString();
    }
    static void localizeAttr(XAttribute attr, Dictionary<string, string> toLoc, context ctx, bool isTitle) {
      if (attr == null) return;
      attr.Value = localizeForTsx(attr.Value, false, toLoc, ctx, isTitle);
      if (ctx.needsMeta && isTitle) attr.Value = "@{meta.title}@";
    }

    //musi odpovidat normalizeTradosName v D:\rw\convert-old-solution\OldToNewViewer\Main.cs
    static string normalizeTradosName(string name) {
      var res = name.ToLower().Replace("-", null).Split('.')[0];
      if (char.IsDigit(res[0])) res = "_" + res;
      else if (res.IndexOf(',') > 0) res = "error_2";
      return res;
    }

    static string replaceBrackets(string s) { return s.Split('{').Select(r => r.Replace("}", "{'}'}")).Join("{'{'}"); }

    static void wrapExpression(XAttribute attr, string expr = null) { attr.Value = "@{" + (expr != null ? expr : attr.Value) + "}@"; }

    static string parseStyle(string st) {
      var parts = st.Split(';').Where(p => p.Length > 0).Select(s => s.Split(':')).Select(p => new { name = p[0].Trim(), value = p[1].Trim() });
      return "{" + parts.Select(nv => lib.toCammelCase(nv.name) + ": '" + nv.value + "'").Aggregate((r, i) => r + ", " + i) + "}";
    }

  }

}

public static class lib {
  public struct regExItem {
    public regExItem(bool isMatch, string val, Match match) { IsMatch = isMatch; Value = val; this.match = match; }
    public bool IsMatch; public string Value; public Match match;
    public static IEnumerable<regExItem> Parse(string s, Regex ex) {
      int pos = 0;
      foreach (Match match in ex.Matches(s)) {
        if (pos < match.Index)
          yield return new regExItem(false, s.Substring(pos, match.Index - pos), null);
        yield return new regExItem(true, s.Substring(match.Index, match.Length), match);
        pos = match.Index + match.Length;
      }
      if (pos < s.Length)
        yield return new regExItem(false, s.Substring(pos, s.Length - pos), null);
    }
  }

  public static Regex localizePartsRegex = new Regex("{{.*?}}", RegexOptions.Singleline);

  public static string Join<T>(this IEnumerable<T> source, string delim = ",") {
    return source.Select(s => s.ToString()).Aggregate((r, i) => r + delim + i);
  }

  public static void AdjustFileDir(string fileName) {
    string dir = Path.GetDirectoryName(fileName);
    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
  }

  public static string toCammelCase(string nm) {
    if (string.IsNullOrEmpty(nm)) return nm;
    List<char> res = new List<char>(); var toUpper = false;
    foreach (var ch in nm) {
      if (ch == '-')
        toUpper = true;
      else if (toUpper) {
        res.Add(char.ToUpper(ch)); toUpper = false;
      } else
        res.Add(ch);
    }
    return new string(res.ToArray());
  }

  public static string typeName(string tn) {
    if (tn.StartsWith("_")) tn = tn.Substring(1);
    return (tn == "node" ? "Dummy" : char.ToUpper(tn[0]) + tn.Substring(1));
  }

  public static void normalizeXml(XElement root, string blankText = " ") {
    root.DescendantNodes().OfType<XComment>().Remove();
    //15.12.2015: k cemu je tento kod? K if (txts == null) return snad nikdy nedojde
    //var txts = root.DescendantNodes().OfType<XText>().Where(t => t.Value.IndexOf('`') >= 0).ToArray();
    //if (txts == null) return;
    foreach (var txt in root.DescendantNodes().OfType<XText>().Where(t => !(t is XCData))) {
      //leva cast textu, ktery je prvni child inline parenta nebo za inline elementem.
      var prevEl = txt.PreviousNode as XElement; var left = prevEl == null ? txt.Parent : prevEl; var leftInline = isInline(left);
      //prava cast textu, ktery je posledni child inline parenta nebo nasledovaneho inlene elementem.
      var rightEl = txt.NextNode as XElement; var right = rightEl == null ? txt.Parent : rightEl; var rightInline = isInline(right);
      //nahrad pocatecni ci koncove mezery budto "" nebo " " ("" pokud obsahuji `)
      txt.Value = blanks.Replace(txt.Value, m => trimNormalize(m.Groups["lblanks"].Value, m.Groups["cont"].Value, m.Groups["rblanks"].Value, leftInline, rightInline, blankText));
    }
    //var emptyTexts = root.DescendantNodes().OfType<XText>().Where(t => string.IsNullOrEmpty(t.Value)).ToArray();
    //var str = root.ToString(SaveOptions.DisableFormatting);
    //emptyTexts.Remove();
    //str = root.ToString(SaveOptions.DisableFormatting);
    root.DescendantNodes().OfType<XText>().Where(t => string.IsNullOrEmpty(t.Value)).Remove();
  }
  static string trimNormalize(string lblank, string cont, string rblank, bool leftInline, bool rightInline, string blankText) {
    if (!string.IsNullOrEmpty(lblank)) lblank = leftInline && lblank.IndexOf('`') < 0 ? blankText : "";
    if (!string.IsNullOrEmpty(rblank)) rblank = rightInline && rblank.IndexOf('`') < 0 ? blankText : "";
    return lblank + cont + rblank;
  }
  static bool isInline(XElement el) { return inlines.Contains(el.Name.LocalName) || (el.Name.LocalName == "html-tag" && inlines.Contains(el.AttributeValue("tag-name"))); }
  static HashSet<string> inlines = new HashSet<string>() {
      "drag-target", "gap-fill", "smart-element", "smart-tag", "word-selection", 
      //https://developer.mozilla.org/en-US/docs/Web/HTML/Inline_elemente
      "b", "big", "i", "small", "tt", "abbr", "acronym", "cite", "code", "dfn", "em", "kbd", "strong", "samp", "var", "u", "s",
      "a", "bdo", /*"br", 15.12.2015 - neni inline*/ "img", "map", "object", "q", "script", "span", "sub", "sup", "button", "input", "label", "select", "textarea",
    };
  static Regex blanks = new Regex(@"^(?<lblanks>(\s|`)*)(?<cont>.*?)(?<rblanks>(\s|`)*)$");
  public static string AttributeValue(this XElement els, XName name) {
    return AttributeValue(els, name, null);
  }
  static string AttributeValue(this XElement els, XName name, string defaultValue) {
    XAttribute attr = els.Attribute(name);
    return attr == null ? defaultValue : attr.Value;
  }

  public static string[] allTypes = new string[] {
    "docExample",
    "docDescr",

      "tag",
      "evalControl",
      "body",
      "headerProp",
      "macro",
      "humanEval",

      //vyhodnoceni
      "evalButton",//obsolete
      "dropDown",
      "edit",
      "gapFill",
      "radioButton",
      "checkLow",
      "checkItem",
      "checkBox",
      "pairingItem",
      "pairing",
      "singleChoice",
      "wordSelection",
      "wordMultiSelection",
      "wordOrdering",
      "sentenceOrdering",
      "sentenceOrderingItem",
      "extension",

      "writing",
      "recording",

      //bez vyhodnoceni
      "list",
      "listGroup",
      "twoColumn",
      "panel",
      "node",
      "offering",

      //zvuk
      "urlTag",
      "mediaTag",
      "mediaBigMark",
      "mediaPlayer",
      "mediaVideo",
      "mediaText",

      //kvuli (jen) dokumentaci?
      "_sndFile",
      "cutDialog",
      "cutText",
      "phrase",
      "replica",
      "include",
      "includeText",
      "includeDialog",
      "phraseReplace",


      //macro Templates. V JS nejsou viditelna
      "macroTemplate",
      "macroTrueFalse",
      "macroSingleChoices",
      "macroPairing",
      "macroTable",
      "macroListWordOrdering",
      "macroList",
      "macroIconList",
      "macroArticle",
      "macroVocabulary",
      "macroVideo",
      "inlineTag",

      "smartTag",
      "smartElementLow",
      "smartElement",
      "smartOffering",
      "smartPairing",
    };

}

