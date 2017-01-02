//https://docs.microsoft.com/en-us/aspnet/core/client-side/using-gulp
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

//var gulp = require('gulp');
import * as gulp from 'gulp';
var acss = require('gulp-atomizer'); 

//gulp.task('xxx', function () {
//  //https://markgoodyear.com/2015/06/using-es6-with-gulp/
//  //http://frontendinsights.com/es6-syntax-in-the-gulp-file-by-using-babel/
//  //https://github.com/isaacs/node-glob
//  //https://gulp.readme.io/docs/gulpsrcglobs-options
//  //http://engineroom.teamwork.com/10-things-to-know-about-gulp/
//  gulp.src('rw-gui-rt/**/*.tsx').pipe({ on: function (fn) { console.log(fn) }});
//});

//gulp.task('concat', ['xxx']);


gulp.task('default', () => {
  return gulp.src('./rw-gui-rt/**/*.tsx')
    .pipe(acss())
    .pipe(gulp.dest('./rw-gui-rt/test-styles.css')); 
});

