import * as gulp from 'gulp';
import * as fs from 'fs';
import * as path from 'path';
import * as util from 'util';
//import * as rename from 'gulp-rename';
import * as argv from 'yargs';
import * as sizeOf from 'image-size';
//import * as runSequence from 'run-sequence'; //incoming GULP 4.0 can run in series!!!!
import * as through from 'through2';

//const imageDataURI = require('gulp-image-data-uri');
const gulpIgnore = require('gulp-ignore');
const clean = require('gulp-clean');
const DataURI = require('datauri.template');

const destFn = (srcPath: string): string => {
  const cfg: IConfig = argv.argv;
  if (!cfg.destPath) cfg.destPath = cfg.basicPath;
  return srcPath.replace(cfg.basicPath, cfg.destPath);
};

interface IConfig {
  basicPath: string; //bitmap SourceBuffer dir
  destPath: string; //bitmap dest dir
  //path: string; //path, relative to source and dest dir
  forceReplace: "true"; //true => dont check file date (in create) and file existence (in delete)
}

function pipeTest() {
  return through.obj(function (file, enc, callback) {
    //this.push(file);
    file['xxx'] = file['xxx'] + 'yyy';
    return callback(null, file);
  });
}

gulp.task('test', cb => {
  return gulp
    .src('d:/temp/gulp/*.png') //
    .pipe(pipeTest())
    .pipe(pipeTest())
    .pipe<NodeJS.ReadWriteStream>(gulp.dest(file => {
      return file.base;
    }));
});
//************** image-url
//gulp.task('image-url', cb => runSequence('delete-image-url', 'create-image-url', cb)); //https://www.npmjs.com/package/run-sequence

gulp.task('create-image-url', cb => {
  console.log('START create-image-url');
  const cfg: IConfig = argv.argv;
  const srcDir = cfg.basicPath;
  const size = { value: { width: -1, height: -1, type: '', origPath: '', id: '' } };
  return gulp
    .src([srcDir + '**/*.png', srcDir + '**/*.jpg', srcDir + '**/*.gif', srcDir + '**/*.bmp']) //
    .pipe<NodeJS.ReadWriteStream>(gulpIgnore.include(fn => { //gulpIgnore.include called due to side efect on fn.path
      const tsFn = destFn(fn.path) + '.ts';
      if (cfg.forceReplace !== "true" && fs.existsSync(tsFn)) {
        const imgStat = fs.statSync(fn.path); const tsStat = fs.statSync(tsFn);
        if (tsStat.size > 10 && imgStat.mtime.getTime() < tsStat.mtime.getTime()) return false;
      }
      //console.log('CREATE: ' + fn.path);
      return true;
    }))
    .pipe(dataUri())
    .pipe<NodeJS.ReadWriteStream>(gulp.dest(file => destFn(file.base))); // the dir of output files 
});

gulp.task('delete-image-url', cb => {
  console.log('START delete-image-url');
  const cfg: IConfig = argv.argv;
  if (!cfg.destPath) cfg.destPath = cfg.basicPath;
  const path = cfg.destPath;
  return gulp
    .src([path + '**/*.png.ts', path + '**/*.jpg.ts', path + '**/*.gif.ts', path + '**/*.bmp.ts'])
    .pipe<NodeJS.ReadWriteStream>(gulpIgnore.include(fn => {
      if (cfg.forceReplace !== "true") {
        const bpm = fn.path.substr(0, fn.path.length - 3).replace(cfg.destPath, cfg.basicPath);
        if (fs.existsSync(bpm)) return false;
      }
      //console.log('DELETE: ' + fn.path);
      return true;
    }))
    .pipe<NodeJS.ReadWriteStream>(clean({ force: true }));
});


function dataUri() {
  return through.obj(function (file, enc, callback) {
    const dataURI = new DataURI();
    const cfg: IConfig = argv.argv;
    dataURI.format(path.basename(file.path), file.contents);
    const ext = path.extname(file.path);
    const origPath = file.path.replace(/\\/g, '/');
    let sz; try { sz = sizeOf(file.path); } catch (err) {
      console.log('Cannot get image size: ' + file.path);
      sz = { width: -1, height: -1 }
    }
    const fileContents = 'const image = ' + JSON.stringify({
      id: origPath.substr(cfg.basicPath.length - 1),
      url: dataURI.content,
      width: sz.width,
      height: sz.height,
      type: ext,
      origPath: origPath
    }, null, 2) + ';\r\nexport default image;';
    file.contents = new Buffer(fileContents);
    file.path = destFn(file.path) + '.ts'; //path.join(path.dirname(file.path), basename + '.css');
    this.push(file);
    return callback();
  });
};

    //.pipe(pipeTest())
    //.pipe<NodeJS.ReadWriteStream>(gulpIgnore.include(fn => { //gulpIgnore.include called due to side efect on fn.path
    //  const tsFn = destFn(fn.path, cfg) + '.ts';
    //  //console.log(tsFn);
    //  if (cfg.forceReplace !== "true" && fs.existsSync(tsFn)) {
    //    const imgStat = fs.statSync(fn.path); const tsStat = fs.statSync(tsFn);
    //    if (tsStat.size > 10 && imgStat.mtime.getTime() < tsStat.mtime.getTime()) return false;
    //  }
    //  //console.log('CREATE: ' + fn.path + '.ts');
    //  size.value.origPath = fn.path.replace(/\\/g, '/');
    //  size.value.type = path.extname(fn.path);
    //  size.value.id = size.value.origPath.substr(cfg.basicPath.length - 1);
    //  try {
    //    const sz = sizeOf(fn.path);
    //    size.value.width = sz.width; size.value.height = sz.height;
    //  } catch (error) {
    //    console.log('Picture Error: ' + fn.path);
    //    size.value.width = -1; size.value.height = -1;
    //  }
    //  return true;
    //}))
    //.pipe<NodeJS.ReadWriteStream>(imageDataURI({ template: { file: './image-url-template.templ', variables: size } })) //generate .TS file by means of template and size
    //.pipe<NodeJS.ReadWriteStream>(rename(pth => { pth.basename = pth.basename + size.value.type; pth.extname = '.ts'; })) //rename new file
