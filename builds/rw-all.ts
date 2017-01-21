var Builder = require('systemjs-builder');

var builder = new Builder('../rw', '../rw/jspm.config.js');

builder.config({
  paths: {
    "config": "../rw/app-config.js" //rewrite D:\rw\rw\jspm.config.js, wrong "config": "./app-config.js"
  }
});

//https://github.com/systemjs/builder/blob/master/docs/api.md#builderbundletree-outfile-options
builder
  //.trace('rw-router/test.js')
  .bundle('rw-router/**/* + rw-lib/**/* + rw-redux/**/* + rw-course/**/* + rw-instr/**/* - rw-router/test - rw-redux/test - rw-course/examples/**/* - rw-course/test', '../rw/rw-all.js', 
  { minify: false, sourceMaps: true, sourceMapContents:true  })
  .then(bundle => {
    debugger;
    console.log('Build complete');
  })
  .catch(err => {
    console.log('Build error');
    console.log(err);
  });