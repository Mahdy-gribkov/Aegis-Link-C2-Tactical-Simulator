# Dependency Manifest

## Project Aegis-Link | Tactical C2 Interface
**Maintainer:** Mahdy Gribkov  
**Target Framework:** .NET 6.0 LTS (Long-Term Support)

---

## Core Dependencies

### .NET 6.0 LTS
- **Version:** 6.0.400+
- **License:** MIT License
- **Purpose:** Long-term support runtime for tactical field deployment
- **Rationale:** Ensures 3+ years of security patches and stability for air-gapped environments

### Microsoft.Data.Sqlite
- **Version:** 6.0.0
- **License:** MIT License
- **Purpose:** Embedded database for mission event logging
- **Rationale:** Zero-configuration persistence layer with no external database server required

### Dapper
- **Version:** 2.0.123
- **License:** Apache License 2.0
- **Purpose:** Micro-ORM for high-performance SQL operations
- **Rationale:** Minimal overhead for tactical black-box logging with sub-millisecond query execution

---

## Test Dependencies

### xUnit
- **Version:** 2.4.1
- **License:** Apache License 2.0
- **Purpose:** Unit testing framework for protocol validation

### Microsoft.NET.Test.Sdk
- **Version:** 16.11.0
- **License:** MIT License
- **Purpose:** Test execution infrastructure

### xunit.runner.visualstudio
- **Version:** 2.4.3
- **License:** Apache License 2.0
- **Purpose:** Visual Studio test adapter

### coverlet.collector
- **Version:** 3.1.2
- **License:** MIT License
- **Purpose:** Code coverage analysis

---

## License Compliance

All dependencies are licensed under permissive open-source licenses (MIT, Apache 2.0) suitable for commercial and defense applications. No GPL or copyleft restrictions apply.

---

**Version Pinning Policy:** All dependencies are strictly pinned to 6.x.x baseline versions to prevent drift in air-gapped tactical environments and ensure reproducible builds across all deployment targets.
