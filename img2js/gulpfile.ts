import * as gulp from 'gulp';
import * as fs from 'fs';
import * as util from 'util';
import * as rename from 'gulp-rename';
import * as argv from 'yargs';
import * as sizeOf from 'image-size';
import * as runSequence from 'run-sequence'; //incoming GULP 4.0 can run in series!!!!

const imageDataURI = require('gulp-image-data-uri');
const gulpIgnore = require('gulp-ignore');
const clean = require('gulp-clean');

//************** image-url
gulp.task('image-url', cb => runSequence('delete-image-url', 'create-image-url', cb)); //https://www.npmjs.com/package/run-sequence

gulp.task('create-image-url', cb => {
  console.log('START create-image-url');
  const path = argv.argv.basicPath + argv.argv.path;
  let size = { value: { width: -1, height: -1, type: '', origPath: '', id: '' } };
  return gulp
    .src([path + '**/*.png', path + '**/*.jpg', path + '**/*.gif', path + '**/*.bmp'])
    .pipe<NodeJS.ReadWriteStream>(gulpIgnore.include(fn => { //gulpIgnore.include called due to side efect on fn.path
      const tsFn = fn.path + '.ts';
      //console.log(tsFn);
      if (fs.existsSync(tsFn)) {
        const imgStat = fs.statSync(fn.path); const tsStat = fs.statSync(tsFn);
        if (tsStat.size>10 && imgStat.mtime.getTime() < tsStat.mtime.getTime()) return false;
      }
      console.log('CREATE: ' + fn.path + '.ts');
      size.value = { ...sizeOf(fn.path), origPath: fn.path.replace(/\\/g, '/'), id: '' };
      size.value.id = size.value.origPath.substr(argv.argv.basicPath.length - 1);
      return true;
    })) 
    .pipe<NodeJS.ReadWriteStream>(imageDataURI({ template: { file: './image-url-template.templ', variables: size } })) //generate .TS file by means of template and size
    .pipe<NodeJS.ReadWriteStream>(rename(pth => { pth.basename = pth.basename + '.' + size.value.type; pth.extname = '.ts'; })) //rename new file
    .pipe<NodeJS.ReadWriteStream>(gulp.dest(file => file.base)); // the dir of output files 
});

gulp.task('delete-image-url', cb => {
  console.log('START delete-image-url');
  const path = argv.argv.basicPath + argv.argv.path;
  return gulp
    .src([path + '**/*.png.ts', path + '**/*.jpg.ts', path + '**/*.gif.ts', path + '**/*.bmp.ts'])
    .pipe<NodeJS.ReadWriteStream>(gulpIgnore.include(fn => {
      const bpm = fn.path.substr(0, fn.path.length - 3); if (fs.existsSync(bpm)) return false; 
      //console.log(bpm);
      console.log('DELETE: ' + fn.path);
      return true;
    }))
    .pipe<NodeJS.ReadWriteStream>(clean());
});

