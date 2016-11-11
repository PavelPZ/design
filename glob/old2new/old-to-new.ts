﻿import * as fs from "fs";
import { Langs } from "./oldLangsEnum";

//prevraceny glob-names
var nwIds = JSON.parse(fs.readFileSync('glob-names.json', "utf8"));
var nwNames = {};
for (var p in nwIds) { nwNames[nwIds[p]] = p; }

//old to new codes
var old = JSON.parse(fs.readFileSync('old-names.json', "utf8"));
var repair = JSON.parse(fs.readFileSync('repair.json', "utf8"));
var done = {};
var todo = {};
for (var p in old) {
  var id: string = nwNames[p];
  var oldId = old[p];
  if (!id) { todo[p] = done[oldId] = nwNames[repair[p]]; continue; }
  var code: string = old[p].split('_')[0];
  if (code.length != 2) { todo[p] = done[oldId] = nwNames[repair[p]]; continue; }
  if (code != id) { todo[p] = done[oldId] = nwNames[repair[p]]; continue; }
  done[oldId] = code;
}
done['pt_pt'] = 'pt-PT';
//done['en_gb'] = 'en-GB';
done['en_gb'] = 'en';
done['es_es'] = 'es';
//Cina: http://stackoverflow.com/questions/4892372/language-codes-for-simplified-chinese-and-traditional-chinese
//Brazil, british: http://cldr.unicode.org/translation/default-content
//existence languages
let notExist = [];
let exist: Array<string> = [];
let existOld: Array<string> = [];
for (var p in done) {
  let newCode = done[p];
  var fn = `..\\..\\node_modules\\cldr-data\\main\\${newCode}\\languages.json`;
  if (!fs.existsSync(fn)) notExist.push(`${p}=${newCode}=${nwIds[newCode]}`); else {
    exist.push(newCode); existOld.push(p);
  }
}

//JScript kod pro vsechny jazyky
var doneByNumber: Array<string> = [];
for (var p in done) if (existOld.indexOf(p)>=0) doneByNumber[Langs[p]] = done[p];
exist = exist.sort();
let jsCode = `//file generated by D:/rw/design/glob/old2new/old-to-new.ts
export type LangType = ${exist.map(l => '"' + l + '"').join(' | ')};
export const allLangs: Array<LangType> = [${exist.map(l => '"' + l + '"').join(', ')}];
//e.g. oldToNew[Langs.cs_cz] = 'cs' 
export const oldToNew: Array<LangType> = ${JSON.stringify(doneByNumber)};
`;
fs.writeFileSync("../../../rw-lib/glob/all-langs.ts", jsCode);
fs.writeFileSync("all-langs.ts", jsCode);

fs.writeFileSync("notExist.txt", notExist.join('\r\n'));
fs.writeFileSync("old2new.json", JSON.stringify(done));
fs.writeFileSync("todo.json", JSON.stringify(todo));
