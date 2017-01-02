import * as gulp from 'gulp';
import * as autoprefixer from 'gulp-autoprefixer';
const atomizer = require('gulp-atomizer');
//const autoprefixer = require('gulp-autoprefixer');

//************** atomizer
gulp.task('atomizer', () => {
  console.log('start');
  var rwPath = '../../rw/';
  return gulp.src([rwPath + 'rw-gui-rt/**/*.tsx', rwPath + 'rw-gui-rt/**/*.ts', rwPath + 'index.html'])
    .pipe(atomizer({
      // the filename of your output file.
      outfile: 'app.css',
      acssConfig: require('./atomizer-config.json')
    }))
    .pipe(autoprefixer({
      browsers: ['last 3 versions'],
      cascade: false
    }))
    .pipe(gulp.dest(rwPath + 'rw-gui-rt')); // the dir of output files
});

