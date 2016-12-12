import * as fs from "fs";

const basicPath = '../../design-rt/';

let classMap = JSON.parse(fs.readFileSync(basicPath + 'class-map.json', "utf8")); 

for (var p in classMap) {
  if (p == '') continue;
  let fn = 'react-toolbox/' + p + '.json';
  console.log(fn);
  console.log(JSON.stringify(classMap[p],null,2));
  fs.writeFileSync(fn, JSON.stringify(classMap[p]));
}
