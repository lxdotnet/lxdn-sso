module.exports = function (grunt) {
    require('load-grunt-tasks')(grunt);

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        browserify: {
          'dist/app.js': ['app/app.js']
        },
        copy: {
            main: {
              expand: true,
              cwd: '/',
              src: '**',
              dest: 'dist',
            },
        }
    });

    grunt.registerTask('default', ['copy', 'browserify', 'watch']);
};