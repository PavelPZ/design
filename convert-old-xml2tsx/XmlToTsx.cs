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
    //public static void toTsxAll() {
    //  toTsxDir(@"d:\rw\data-src\lm\oldea", @"d:\rw\data\lm\oldea");
    //}

    public static void toTsxDir(string srcDir, string destDir, bool isInstr) {
      List<string> errors = new List<string>();
      var files = Directory.EnumerateFiles(srcDir, "*.xml", SearchOption.AllDirectories).Select(f => f.ToLower()).Where(f => !f.EndsWith(@"\meta.xml")).ToArray();
      //var allLocTags = new HashSet<string>();
      Dictionary<string, string> loc = null; Dictionary<string, Dictionary<string, string>> instrLocs = null;
      if (isInstr) {
        loc = new Dictionary<string, string>();
        instrLocs = initInstrLocXml();
      }
      //foreach (var fn in files.Where(f => f.Contains("les08\\g04"))) {
      foreach (var fn in files) {
        var relPath = fn.Substring(srcDir.Length);
        var destPath = destDir + relPath.Replace(".xml", ".tsx");
        var destLocPath = destDir + relPath.Replace(".xml", "-loc.ts");
        try {
          //var xml = XElement.Load(fn);
          //foreach (var prop in xml.DescendantsAndSelf().SelectMany(el => el.Attributes().Where(a => a.Value.StartsWith("{{")).Select(a => el.Name.LocalName + "." + a.Name.LocalName))) allLocTags.Add(prop);
          //continue;
          if (isInstr) loc.Clear();
          var s = toTsx(XElement.Load(fn), Path.GetFileNameWithoutExtension(fn), loc);
          lib.AdjustFileDir(destPath); File.WriteAllText(destPath, s);
          //create LOC file for instr
          var locFn = fn.Replace(".xml", ".loc");
          if (isInstr) createInstrLocXml(loc, instrLocs).Save(locFn);
          s = toLoc(XElement.Load(locFn));
          File.WriteAllText(destLocPath, s);
        } catch (Exception exp) {
          File.WriteAllText(destPath, exp.Message);
          errors.Add(destPath);
        }
      }
      //File.WriteAllLines(@"d:\temp\allproptags.txt", allLocTags);
      if (errors.Count > 0) throw new Exception(errors.Aggregate((r, i) => r + "\r\n" + i));
    }

    static string toLoc(XElement xml) {
      if (!xml.Elements().Any()) return "export default {};";
      return "import { ILocItem } from \"rw-lib/loc\";\r\n" +
      xml.Elements().Select(e => {
        return
          string.Format("const {0}: ILocItem = {{\r\n", e.Name.LocalName.ToLower()) +
          e.Elements().Where(ee => ee.Name.LocalName != "vi-vi").Select(ee => string.Format("  \"{0}\": \"{1}\",", ee.Name.LocalName, HttpUtility.JavaScriptStringEncode(ee.Value))).Aggregate((r, i) => r + "\r\n" + i) + "\r\n" +
          "};";
      }).DefaultIfEmpty().Aggregate((r, i) => r + "\r\n" + i) + "\r\n" +
      "export default { " + xml.Elements().Select(e => e.Name.LocalName.ToLower()).Aggregate((r, i) => r + ", " + i) + " };";
    }

    static XElement createInstrLocXml(Dictionary<string, string> loc, Dictionary<string, Dictionary<string, string>> instrLocs) {
      return new XElement("loc", loc.Keys.Select(n => {
        return new XElement(n, !instrLocs.ContainsKey(n)  
          ? new XElement("en-gb", loc[n]) as Object
          : instrLocs[n].Select(nv => new XElement(nv.Key, nv.Value)));
      }));
    }
    static Dictionary<string, Dictionary<string, string>> initInstrLocXml() {
      var instrLocs = new Dictionary<string, Dictionary<string, string>>();
      var instrs = XElement.Load(@"d:\rw\data-src\instr\instr.loc-all");
      foreach (var lngEl in instrs.Elements()) foreach (var nameEl in lngEl.Elements()) {
          if (nameEl.Value == "TODO") continue;
          var name = nameEl.Attribute("lang").Value.ToLower();
          if (!instrLocs.ContainsKey(name)) instrLocs[name] = new Dictionary<string, string>();
          var nm = instrLocs[name];
          nm[lngEl.Name.LocalName.Replace('_', '-')] = nameEl.Value;
        }
      return instrLocs;
    }

    static string toTsx(XElement xml, string name, Dictionary<string, string> toLoc) {
      var body = xml.Element("body");
      HashSet<string> allCrsTags = new HashSet<string>(); // { "$rc", "$loc" };
      if (body == null) body = xml;
      else {
        lib.normalizeXml(xml, blankCode);
        var temp = xml.Element("head"); if (temp != null) temp = temp.Element("title");
        if (temp != null) body.Add(new XAttribute("title", temp.Value));
        //27.12.16: 
        var attr = body.Attribute("externals"); if (attr != null) attr.Remove();
      }
      //remove tag with id="alwaysVisiblePanel"
      foreach (var el in body.DescendantsAndSelf().OfType<XElement>().Where(e => e.AttributeValue("id") == "alwaysVisiblePanel").ToArray()) el.Remove();

      foreach (var el in body.DescendantsAndSelf()) {
        var tagName = lib.toCammelCase(el.Name.LocalName);
        var isTag = tags.Contains(tagName);
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
          var newName = lib.toCammelCase(oldName);
          var fullName = tagName + "." + newName;
          var newAttr = newName != oldName ? renameAttr(attr, newName) : attr;
          if (numProps.Contains(newName) || numProps.Contains(fullName)) wrapExpression(newAttr);
          if (boolProps.Contains(newName) || boolProps.Contains(fullName)) wrapExpression(newAttr);
          string enumType;
          if (enumProps.TryGetValue(newName, out enumType) || enumProps.TryGetValue(fullName, out enumType))
            wrapExpression(newAttr, "course." + enumType + "." + lib.toCammelCase(newAttr.Value));
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
      foreach (var el in body.DescendantNodes().OfType<XText>()) el.Value = localizeForTsx(el.Value, true, toLoc);
      
      //localize attributes
      foreach (var att in body.Descendants("PairingItem").Select(e => e.Attribute("right"))) localizeAttr(att, toLoc);
      localizeAttr(body.Attribute("instrTitle"), toLoc); localizeAttr(body.Attribute("title"), toLoc);

      //instructions
      var instrStr = body.AttributeValue("instrBody");
      if (!string.IsNullOrEmpty(instrStr)) {
        string[] instrs = instrStr == null ? new string[0] : instrStr.Split('|').Select(s => s.Trim()).ToArray();
        body.Attribute("instrBody").Value = "@{" + (instrs.Length == 1 ? instrs[0] : "[" + instrs.Aggregate((r, i) => r + ", " + i) + "]") + "}@";
        instrStr = instrs.Select(ins => string.Format("import {0} from \"rw-instr/{0}\"; ", ins)).Aggregate((r, i) => r + " " + i);
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

      //prefix:
      string prefix = @"import React from 'react'; import course, {{{0}}} from 'rw-course'; import {{ $l }} from 'rw-lib/loc'; import l from './{1}-loc'; {2} export default () => /*
*********** START MARKUP HERE: */
";
      prefix = string.Format(prefix, allCrsTags.Aggregate((r, i) => r + ", " + i), name, instrStr);
      //return
      return prefix + res;
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
    static string localizeForTsx(string text, bool plainText, Dictionary<string, string> toLoc) {
      StringBuilder sb = null;
      foreach (var m in lib.regExItem.Parse(text, lib.localizePartsRegex)) {
        if (sb == null) sb = new StringBuilder();
        if (!m.IsMatch) { sb.Append(replaceBrackets(m.Value).Replace(blankCode, "{' '}")); continue; }
        var parts = m.Value.Substring(2, m.Value.Length - 4).Split('|');

        //for grammar: some name contains -.
        var parts0 = (parts[0].IndexOf('-') >= 0 ? parts[0].Replace("-", null) : parts[0]).ToLower();
        //d:\rw\data-src\lm\oldea\spanish1\grammar\les03\g01.xml
        if (char.IsDigit(parts0[0])) parts0 = "error_1";
        //D:\rw\data\lm\oldea\spanish1\grammar\les08\g04.tsx
        if (parts0.IndexOf(',') > 0) parts0 = "error_2";

        if (toLoc != null) toLoc[parts0] = parts[1];
        var txt = string.Format("{{$l(l.{0})}}", parts0);
        //var txt = string.Format("{{$loc('{0}','{1}')}}", parts[0], HttpUtility.JavaScriptStringEncode(parts[1]));
        sb.Append(plainText ? txt : "@" + txt + "@");
      }
      return sb == null ? text : sb.ToString();
    }
    static void localizeAttr(XAttribute attr, Dictionary<string, string> toLoc) {
      if (attr == null) return;
      attr.Value = localizeForTsx(attr.Value, false, toLoc);
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