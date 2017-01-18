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
  .bundle('rw-router/test.js', '../rw/bundle.js', { minify: true })
  .then(bundle => {
    debugger;
    console.log('Build complete');
  })
  .catch(err => {
    console.log('Build error');
    console.log(err);
  });