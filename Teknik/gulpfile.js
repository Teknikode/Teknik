/// <binding ProjectOpened='watch' />
"use strict";

var gulp = require('gulp');
var rimraf = require("rimraf");
var concat = require("gulp-concat");
var cssmin = require("gulp-cssmin");
var merge = require('merge-stream');
var del = require("del");
var replace = require("gulp-replace");
var rename = require("gulp-rename");
var git = require("git-rev-sync");

var uglifyes = require('uglify-es');
var composer = require('gulp-uglify/composer');
var uglify = composer(uglifyes, console);

var eslint = require('gulp-eslint');

var bundleconfig = require("./bundleconfig.json");

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/
};

var assets = [
    // Library JS Files
    { './node_modules/bootbox/bootbox.js': 'lib/bootbox/js' },
    { './node_modules/bootstrap/dist/js/bootstrap.js': 'lib/bootstrap/js' },
    { './node_modules/bootstrap-select/dist/js/bootstrap-select.js': 'lib/bootstrap/js' },
    { './node_modules/bootstrap-switch/dist/js/bootstrap-switch.js': 'lib/bootstrap/js' },
    { './node_modules/crypto-js/*.js': 'lib/crypto-js/js' },
    { './node_modules/dropzone/dist/dropzone.js': 'lib/dropzone/js' },
    { './node_modules/file-saver/FileSaver.js': 'lib/file-saver/js' },
    { './node_modules/filesize/lib/filesize.js': 'lib/filesize/js' },
    { './node_modules/highcharts/js/highcharts.js': 'lib/highcharts/js' },
    { './node_modules/jquery/dist/jquery.js': 'lib/jquery/js' },
    { './node_modules/block-ui/jquery.BlockUI.js': 'lib/jquery/js' },
    { './node_modules/jquery-validation/dist/jquery.validate.js': 'lib/jquery/js' },
    { './node_modules/marked/lib/marked.js': 'lib/marked/js' },
    { './node_modules/sanitize-html/dist/sanitize-html.js': 'lib/sanitize-html/js' },
    { './node_modules/underscore/underscore.js': 'lib/underscore/js' },

    // App JS Files
    { './Scripts/**/*': 'js/app' },

    // Library CSS Files
    { './node_modules/bootstrap/dist/css/bootstrap.css': 'lib/bootstrap/css' },
    { './node_modules/bootstrap/dist/css/bootstrap.css.map': 'lib/bootstrap/css' },
    { './node_modules/bootstrap-select/dist/css/bootstrap-select.css': 'lib/bootstrap/css' },
    { './node_modules/bootstrap-switch/dist/css/bootstrap3/bootstrap-switch.css': 'lib/bootstrap/css' },
    { './node_modules/dropzone/dist/dropzone.css': 'lib/dropzone/css' },
    { './node_modules/font-awesome/css/font-awesome.css': 'lib/font-awesome/css' },
    { './node_modules/highcharts/css/highcharts.css': 'lib/highcharts/css' },
    { './node_modules/highlight.js/styles/*': 'lib/highlight/css' },

    // App CSS Files
    { './Content/**/*': 'css/app' },

    // Fonts
    { './node_modules/bootstrap/dist/fonts/*': 'lib/bootstrap/fonts' },
    { './node_modules/bootstrap/dist/fonts/*': 'fonts' },
    { './node_modules/font-awesome/fonts/*': 'lib/font-awesome/fonts' },
    { './node_modules/font-awesome/fonts/*': 'fonts' },

    // Images
    { './Images/favicon.ico': 'images' },
    { './Images/logo-black.svg': 'images' },
    { './Images/logo-blue.svg': 'images' }
];

gulp.task("eslint", function () {
    return gulp.src(['./Scripts/**/*.js', '!Scripts/lib/**'])
        .pipe(eslint())
        .pipe(eslint.formatEach('compact', process.stderr));
});

gulp.task("clean", function (cb) {
    return rimraf("./wwwroot/*", cb);
});

gulp.task('copy-assets', function () {
    var streams = [];
    for (var asset in assets) {
        for (var item in assets[asset]) {
            streams.push(gulp.src(item).pipe(gulp.dest('./wwwroot/' + assets[asset][item])));
        }
    }
});

gulp.task("load-bundle", function () {
    bundleconfig = require("./bundleconfig.json");
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("min:js", function () {
    var tasks = getBundles(".js").map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:css", function () {
    var tasks = getBundles(".css").map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("update-version", function () {
    return gulp.src('./App_Data/version.template.json')
        .pipe(replace('{{git_ver}}', git.tag()))
        .pipe(replace('{{git_hash}}', git.long()))
        .pipe(rename('version.json'))
        .pipe(gulp.dest('./App_Data'));
});

gulp.task("watch", function () {
    // Watch Source Files
    assets.forEach(function (src) {
        for (var key in src) {
            gulp.watch(key, ["copy-assets"]);
        }
    });

    // Watch Bundle File Itself
    gulp.watch('./bundleconfig.json', ["load-bundle", "min"]);

    // Watch Bundles
    getBundles(".js").forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:js"]);
    });

    getBundles(".css").forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:css"]);
    });
});

function getBundles(extension) {
    return bundleconfig.filter(function (bundle) {
        return new RegExp(`${extension}$`).test(bundle.outputFileName);
    });
}