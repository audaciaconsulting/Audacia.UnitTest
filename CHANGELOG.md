# Changelog

## 1.0.0 - 2024-12-08
Initial creation of the `Audacia.UnitTest` repo.

### Added
- Audacia.UnitTest.Dependency

### Changed
- Migrated Audacia.Seed v2.0.4 to Audacia.UnitTest.Seed v1

## 1.0.1-alpha - 2025-03-27
`Audacia.UnitTest` improvements.

### Added
- License file
- `Audacia.UnitTest.Dependency.Http`
- Dependency chain and extensions
- Example project utilities and tests
- Dependency track pipeline
- Named options snapshot

### Changed
- Documentation improvements
- Version updates

## 1.0.1 - 2026-09-30

### Added

- Visibility of automatic choices - lists the concrete class or blueprint the builder chose for each
  dependency supplied automatically
- `Audacia.UnitTest.Dependency.Azure` - blueprints for Azure Service Bus, Storage Queue and Blob Storage clients
- Customisation of the `TestTargetBuilder` to allow a test target to be built with a specific dependency, or a specific dependency to be built with a specific dependency, and so on recursively.

### Changed

- Improved `TestTargetBuilder` dependency resolution, and the wording of checks that reject a mock wrapper
- Packages are signed and publishable on release, and dependency versions have been updated
- Code analysis is fully enabled and its warnings resolved
