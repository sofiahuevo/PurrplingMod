require('child_process').exec('git describe --tags', function (err, stdout) {
    let [tag, commits, hash] = stdout.split('-');
    let version = /[0-9]+\.[0-9]+\.[0-9]+/.exec(tag)[0];

    const fs = require('fs');

    let rawdata = fs.readFileSync('manifest.json');
    let manifest = JSON.parse(rawdata.toString().trim());
    let date = new Date()
    let stamp = `${date.getFullYear()}${date.getMonth()}${date.getDate()}`;
    let newVersion = `${version}-nightbuild.${commits || 0}.${hash || stamp}`;

    console.log("Current version in manifest:", manifest['Version'])
    console.log("Update to version:", newVersion);

    manifest['Version'] = newVersion;

    fs.writeFileSync('manifest.json', JSON.stringify(manifest, null, 2));

    console.log("Done! Don't forget 'git checkout HEAD manifest.json' after you published nightbuild and continue development and commiting!");
});
