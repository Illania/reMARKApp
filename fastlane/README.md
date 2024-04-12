fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## iOS

### ios generate_certificates

```sh
[bundle exec] fastlane ios generate_certificates
```

Generate certificates

### ios build_release

```sh
[bundle exec] fastlane ios build_release
```

Build iOS app release

### ios release_beta

```sh
[bundle exec] fastlane ios release_beta
```

Upload to Testflight

### ios release

```sh
[bundle exec] fastlane ios release
```

Upload to AppStore

----


## Android

### android build_release

```sh
[bundle exec] fastlane android build_release
```

Build Android app release

### android upload_internal_beta

```sh
[bundle exec] fastlane android upload_internal_beta
```

Upload internal beta release to Confluence Android Installers page

### android release_beta

```sh
[bundle exec] fastlane android release_beta
```

Upload beta release to Google Play beta

### android release

```sh
[bundle exec] fastlane android release
```

Upload release to Google Play

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
