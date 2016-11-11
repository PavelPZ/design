import * as fs from "fs";
import { allLangs, LangType } from '../old2new/all-langs';

//************** znaky a smer zapisu
interface IChars {
  exemplarCharacters?: string;
  auxiliary?: string;
  characterOrder?: string;
  lineOrder?: string;
}
let res: { [lang: string]: IChars; } = {};
allLangs.forEach(l => {
  let fn = `../../node_modules/cldr-data/main/${l}/characters.json`;
  let lres: IChars = res[l] = {};
  if (fs.existsSync(fn)) {
    var json = JSON.parse(fs.readFileSync(fn, "utf8"))['main'][l]['characters'];
    lres.exemplarCharacters = json['exemplarCharacters'];
    lres.auxiliary = json['auxiliary'];
  }
  fn = `../../node_modules/cldr-data/main/${l}/layout.json`;
  if (fs.existsSync(fn)) {
    var json = JSON.parse(fs.readFileSync(fn, "utf8"))['main'][l]['layout']['orientation'];
    lres.characterOrder = json['characterOrder'];
    lres.lineOrder = json['lineOrder'];
  }
});

fs.writeFileSync("characters.json", JSON.stringify(res, null, 2));

//************** nazvy jazyku
let langNames: { [lang: string]: { [lang: string]: string; }; } = {};
allLangs.forEach(l => {
  let fn = `../../node_modules/cldr-data/main/${l}/languages.json`;
  let act = langNames[l] = {};
  if (!fs.existsSync(fn)) return;
  var json = JSON.parse(fs.readFileSync(fn, "utf8"))['main'][l]['localeDisplayNames']['languages'];
  allLangs.forEach(p => act[p] = json[p]);
});

fs.writeFileSync("languages.json", JSON.stringify(langNames, null, 2));
