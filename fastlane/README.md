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

### ios certificates

```sh
[bundle exec] fastlane ios certificates
```

Get certificates

### ios generate_new_certificates

```sh
[bundle exec] fastlane ios generate_new_certificates
```

Generate new certificates

### ios build_beta

```sh
[bundle exec] fastlane ios build_beta
```

Build beta version

### ios upload_beta

```sh
[bundle exec] fastlane ios upload_beta
```

Upload beta to Testflight

### ios bump_droid

```sh
[bundle exec] fastlane ios bump_droid
```

Bump Android version

### ios build_droid

```sh
[bundle exec] fastlane ios build_droid
```

Build Android app

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
