//diskuze: https://github.com/globalizejs/globalize/issues/590
//online tool: http://johnnyreilly.github.io/globalize-so-what-cha-want

import * as fs from "fs";
import * as compiler from "globalize-compiler"; ////http://stackoverflow.com/questions/32142529/how-to-access-culture-data-in-globalize-js-v1-0-0
import * as extend from "extend";
import * as formaters from "../../rw-lib/glob/formaters";
import { allLangs } from './old2new/all-langs';
import * as cldr from 'cldrjs';
import * as Globalize from 'globalize';

//let cldr: cldr.CldrFactory = require('cldrjs');
//let globalize: GlobalizeStatic = require("globalize");
let cldrData = require("cldr-data"); ////d:\rw\design\node_modules\cldr-data\index.js

//let allLangs = ['cs'];

//************ hlavni funkce na generaci d:\rw\rw-lib\glob\*.js souboru s formatovacimi funkcemi
//pouziti napr. 
//  createGlob('..\\..\\temp\\');
export function compileRuntime(relDir: string/*adresar, relativne k self, pro .JS soubory*/) {
  console.log('START createGlob');
  let all = cldrData.all(); //naladuje vsechna JSON z d:\rw\design\node_modules\cldr-data\, main a supplemental
  Globalize.load(all); //umisti je do globalize
  //globalize.loadMessages({
  //  cs: {
  //    plain: 'My plain text message',
  //    complex: '{count, plural, one {one message} other {{formattedCount} messages}} remaining'
  //  }
  //})
  //***************** know-how

  allLangs.forEach(loc => {
    var glob = new Globalize(loc); //vybere urcitou lokalizaci

    let formattedCount = glob.dateFormatter({ date: "full" });
    //console.log(dateFormater(new Date()));

    //pro kazdy jazyk, formater a parametr formateru vztvori lokalizacni funkci
    var allForms: Array<formaters.getFormatterFnc> = [];
    for (var p in formaters.formaterFncs) allForms.push(formaters.formaterFncs[p]);
    let js = compiler.compile(allForms.map(f => f(glob))); //vlastni generace .JS

    //let js2 = compiler.compile([new Globalize('en').messageFormatter('*')]); //vlastni generace .J

    fs.writeFileSync(`../../rw-lib/glob/locale-data/${loc}.js`, js);
  });
  console.log('END createGlob');
}

function knowHow() {
  //po castech:
  let main = cldrData.entireMainFor('cs');
  let supls = cldrData.entireSupplemental();
  Globalize.load(supls);
  Globalize.load(main);
  console.log(cldrData.availableLocales);

  cldr.load(supls);
  cldr.load(main);
  var cldr = new cldr("cs");
  var c = new cldr.CldrStatic("cs");
}

//************************* cldr-data.json, jsou moc velka

var basicPath = 'd:\\rw\\design\\node_modules\\cldr-data\\';

interface IData {
  main?: { [loc: string]: {}; },
  supplemental?: {}
}

function fileListAll(locale: string): Array<string> {
  return [
    `supplemental/likelySubtags.json`,
    `main/${locale}/numbers.json`,
    `supplemental/numberingSystems.json`,
    `supplemental/plurals.json`,
    `supplemental/ordinals.json`,
    `main/${locale}/currencies.json`,
    `supplemental/currencyData.json`,
    `main/${locale}/ca-gregorian.json`,
    `main/${locale}/timeZoneNames.json`,
    `supplemental/timeData.json`,
    `supplemental/weekData.json`,
    `main/${locale}/dateFields.json`,
    `main/${locale}/units.json`
  ];
}

function fileListStd(locale: string): Array<string> { //number, date, plural, message, relativeTime
  return [
    `supplemental/likelySubtags.json`,
    `main/${locale}/numbers.json`,
    `supplemental/numberingSystems.json`,
    `main/${locale}/ca-gregorian.json`,
    `main/${locale}/timeZoneNames.json`,
    `supplemental/timeData.json`,
    `supplemental/weekData.json`,
    `supplemental/plurals.json`,
    `supplemental/ordinals.json`,
    `main/${locale}/dateFields.json`,
    //`main/${locale}/currencies.json`,
    //`supplemental/currencyData.json`,
    //`main/${locale}/units.json`
  ];
}

function fileListMin(locale: string): Array<string> { //number, date, message
  return [
    `supplemental/likelySubtags.json`,
    `main/${locale}/numbers.json`,
    `supplemental/numberingSystems.json`,
    `main/${locale}/ca-gregorian.json`,
    `main/${locale}/timeZoneNames.json`,
    `supplemental/timeData.json`,
    `supplemental/weekData.json`,
    //`supplemental/plurals.json`,
    //`supplemental/ordinals.json`,
    //`main/${locale}/dateFields.json`,
    //`main/${locale}/currencies.json`,
    //`supplemental/currencyData.json`,
    //`main/${locale}/units.json`
  ];
}

function files(list: Array<string>): Object {
  var res = {};
  list.forEach(f => {
    let fn = (basicPath + f).replace(/\//g, '\\');
    console.log(fn);
    let s = fs.readFileSync(fn, "utf8");
    let obj = JSON.parse(s) as IData;
    res = extend(true, res, obj);
  });
  return res;
}

export function test() {
  var res = files(fileListAll('cs'));
  fs.writeFile('cldr-cs-all.json', JSON.stringify(res, null, '  '), err => console.log(err));
  var res = files(fileListStd('cs'));
  fs.writeFile('cldr-cs-std.json', JSON.stringify(res, null, '  '), err => console.log(err));
  var res = files(fileListMin('cs'));
  fs.writeFile('cldr-cs-min.json', JSON.stringify(res, null, '  '), err => console.log(err));
}