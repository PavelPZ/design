import * as fs from "fs";

interface fetchSrc {
  address: string;
  name: string;
  source: string;
  metadata: { deps: Array<string>; format: string; };
}
export interface ISystem {
  import(id: string): any;
  get(name: string): any;
  delete(name: string); //remove module from SystemJS
  normalize(name: string, parentName?: string): any;
  normalizeSync(name: string, parentName?: string): string;
  fetch(load: fetchSrc): any;
  instantiate(load: fetchSrc): any;
  translate(load: fetchSrc): any;
  defined: {[name:string]:any;};
  config(cfg:any);
}


var SystemJS: ISystem = require('systemjs');

SystemJS.config({
  paths: {
    "npm:": "../../rw/rw/jspm_packages/npm/",
    //"rw-course/examples/*": "../../rw/rw/rw-course/examples/*.js",
    "config": "./app-config.js"
  },
  map: {
    "react": "npm:react@15.4.2/dist/react.js",
    "react-redux": "npm:react-redux@4.4.6/dist/react-redux.js",
    "redux": "npm:redux@3.6.0/dist/redux.js",
    "redux-logger": "npm:redux-logger@2.7.4/dist/index.js",
    "react-dom": "npm:react-dom@15.4.2/dist/react-dom-server.js",
  },
  packages: {
    "rw-lib": {
      "defaultExtension": "js"
    },
    "rw-gui-rt": {
      "defaultExtension": "js"
    },
    "rw-router": {
      "main": "index.js",
      "defaultExtension": "js"
    },
    "rw-redux": {
      "main": "index.js",
      "defaultExtension": "js"
    },
    "rw-instr": {
      "defaultExtension": "js"
    },
    "design": {
      "defaultExtension": "js"
    },
    "rw-course": {
      "main": "index.js",
      "defaultExtension": "js"
    }
  }  
});


SystemJS.import('./builds/rw.js').then(m => {
  const name = SystemJS.normalizeSync('design/test.js');
  const allMods:Array<string> = [];
  for(let p in SystemJS.defined) allMods.push(p);
  fs.writeFileSync('debug.log', allMods.join('\r\n'));
  SystemJS.import(name).then (m => {
    debugger;
  }).catch(err => {
    debugger;
  });
  console.log('done');
}).catch(err => {
  debugger;
});