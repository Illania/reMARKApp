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

### ios entitlements

```sh
[bundle exec] fastlane ios entitlements
```

Update entitlements

### ios build_release

```sh
[bundle exec] fastlane ios build_release
```

Build release

### ios bump_ios

```sh
[bundle exec] fastlane ios bump_ios
```

bump ios version and build number

### ios bump_android

```sh
[bundle exec] fastlane ios bump_android
```

bump android version

### ios restore

```sh
[bundle exec] fastlane ios restore
```

restore nuget

### ios msbuild_ios

```sh
[bundle exec] fastlane ios msbuild_ios
```

msbuild ios

### ios msbuild_android

```sh
[bundle exec] fastlane ios msbuild_android
```

msbuild android

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

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
