var jspm = require('jspm');

jspm.setPackagePath('../rw');

const doBuild = (source:string, fileName:string) => {
  var builder = new jspm.Builder();

  builder.config({
    paths: {
      "config": "../rw/app-config.js" //rewrite D:\rw\rw\jspm.config.js, wrong "config": "./app-config.js"
    }
  });

  return builder
    //.trace(source)
    .bundle(source, fileName + '.js', { minify: false })
    .then(bundle => console.log(`Build ${fileName} complete`))
    .catch(err => {
      console.log(`Build ${fileName} error`);
      console.log(err);
    });
}

//doBuild('react + react-dom + react-redux + redux + redux-logger', '../rw/react-all');
doBuild('(rw-course/**/*.js + rw-instr/**/*.js + rw-lib/**/*.js + rw-router/**/*.js + rw-redux/**/*.js) - react-all.js', '../rw/code-all');



