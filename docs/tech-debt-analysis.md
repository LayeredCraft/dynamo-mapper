# Technical Debt Analysis Report: DynamoMapper

**Generated**: 2026-01-21
**Version**: 1.0.0

---

## 0) Scope and Assumptions

### Repository Shape
| Category | Count | Details |
|----------|-------|---------|
| Solutions | 1 | `LayeredCraft.DynamoMapper.slnx` |
| Source Projects | 2 | Generators (netstandard2.0), Runtime (netstandard2.0) |
| Test Projects | 3 | Multi-targeted: net8.0, net9.0, net10.0 |
| Example Projects | 4 | net10.0 examples |

### Target Frameworks
- **Generators**: `netstandard2.0` (Roslyn source generator compatibility)
- **Runtime**: `netstandard2.0` (broad .NET compatibility)
- **Tests**: Multi-targeted (`net8.0`, `net9.0`, `net10.0`)

### Quality Tooling Present
| Tool | Status | Notes |
|------|--------|-------|
| `.editorconfig` | Present | Basic formatting rules |
| `Directory.Build.props` | Present | Centralized build settings, SourceLink |
| `Directory.Packages.props` | Present | Central package management |
| `global.json` | Present | SDK 10.0.102 pinned |
| CI/CD | Present | GitHub Actions (build, PR, docs) |
| Analyzers | Partial | `EnforceExtendedAnalyzerRules` on generator only |
| Code Coverage | Missing | No coverage tooling configured |
| Dependency Scanning | Missing | No `dotnet list package --vulnerable` in CI |

---

## 1) Technical Debt Inventory

### 1.1 Code Debt

#### Duplicated Code

| What | Where | Lines | Impact |
|------|-------|-------|--------|
| Numeric extension boilerplate | `NumericAttributeValueExtensions.cs` | ~400 | TryGet/Get/Set repeated for int, long, float, double, decimal |
| DateTime extension boilerplate | `DateTimeAttributeValueExtensions.cs` | ~350 | Same pattern for DateTime, DateTimeOffset, TimeSpan |
| Collection From/To rendering | `PropertyMappingCodeRenderer.cs` | ~200 | List/Map From/To methods have ~60% similarity |
| Nested object rendering | `PropertyMappingCodeRenderer.cs` | ~150 | Mapper-based vs inline patterns overlap |

**Estimated Duplication**: 400-500 lines (8-10% of source code)

**Hotspots**:
- `src/LayeredCraft.DynamoMapper.Runtime/AttributeValueExtensions/` - 3,274 lines with repetitive patterns
- `PropertyMappingCodeRenderer.cs` - 851 lines with similar rendering methods

---

#### Complex Code

| File | Lines | Complexity | Issues |
|------|-------|------------|--------|
| `PropertyMappingCodeRenderer.cs` | 851 | High (~12 CC) | 15+ rendering methods, deep switch expressions |
| `NestedObjectTypeAnalyzer.cs` | 412 | Moderate-High (~8 CC) | Sequential null checks, multiple return paths |
| `TypeMappingStrategyResolver.cs` | 407 | Moderate (~7 CC) | 70+ line resolution chain |
| `CollectionAttributeValueExtensions.cs` | 927 | Moderate | Extensive but well-organized |

**Methods exceeding 50 lines**: 12
**Methods exceeding 100 lines**: 3

---

#### Poor Structure

| Issue | Location | Impact |
|-------|----------|--------|
| String-based code generation | `PropertyMappingCodeRenderer.cs` | ~600 lines of manual concatenation; no compile-time validation |
| Custom bounded cache | `BoundedCacheWithFactory.cs` | Reinvents the wheel (80 lines) |
| Duplicate constructor selection | `ConstructorSelector.cs:63,76` | `SelectConstructorWithMostParameters` called twice |

**Boundary Violations**: None detected - clean separation between Generator/Runtime

---

#### Correctness & Maintainability Debt

| Issue | Count | Risk |
|-------|-------|------|
| Async blocking patterns | 0 | None (appropriate for generators) |
| Missing cancellation tokens | 0 | Appropriate |
| Exception handling | Functional | Uses `DiagnosticResult<T>` pattern - excellent |
| Build warnings | 40 | CS8618 (nullability), CS8601 in examples |

---

### 1.2 Architecture Debt

| Issue | Severity | Notes |
|-------|----------|-------|
| Monolithic renderer | Medium | `PropertyMappingCodeRenderer` has too many responsibilities |
| Manual string rendering | Medium | No AST-based code generation |
| Extension-heavy design | Low | Limits polymorphism but acceptable for this domain |

**Positive Patterns**:
- Clean Generator/Runtime separation
- Functional error handling with `DiagnosticResult<T>`
- Incremental generation with proper tracking names
- Snapshot testing for generated code validation

---

### 1.3 Technology Debt

| Category | Status | Details |
|----------|--------|---------|
| Outdated packages | None | All packages up to date |
| Vulnerable packages | None | No known vulnerabilities |
| SDK pinning | Good | `global.json` pins SDK 10.0.102 |
| Internal dependency | Warning | `LayeredCraft.SourceGeneratorTools` 0.1.0-beta.10 (not found on NuGet) |

---

### 1.4 Testing Debt

| Metric | Value | Target | Gap |
|--------|-------|--------|-----|
| Total tests | 324 | - | - |
| Test code lines | 6,317 | - | - |
| Test:source ratio | ~0.7:1 | 1:1 | Low for library |
| Snapshot tests | 70+ | - | Excellent |
| Unit test coverage | Unknown | 80% | Need instrumentation |

**Coverage Gaps**:
- No unit tests for `TypeMappingStrategyResolver` logic
- No tests for diagnostic generation (error paths)
- Limited error path testing
- No performance/benchmark tests
- TestKit project has zero tests (exit code 8)

---

### 1.5 Documentation Debt

| Item | Status | Notes |
|------|--------|-------|
| README | Present | Good quick start |
| CLAUDE.md | Present | Comprehensive dev guide |
| MkDocs site | Present | Full documentation |
| API docs | Enabled | `GenerateDocumentationFile=true` |
| In-code comments | Variable | PropertyMappingCodeRenderer well-documented; extensions sparse |

---

### 1.6 Build, CI, and Infrastructure Debt

| Issue | Severity | Details |
|-------|----------|---------|
| Build warnings | Medium | 40 warnings (mostly nullability) |
| No coverage gate | Medium | CI doesn't track/enforce coverage |
| No SBOM generation | Low | No dependency manifest |
| No vulnerability scanning in CI | Medium | `dotnet list package --vulnerable` not in pipeline |
| Formatting enforcement | Missing | No `dotnet format` check in CI |
| Analyzer enforcement | Partial | Only on generator project |

**Build/Test Metrics**:
- Build time: ~49 seconds
- Test time (net10.0): ~6.5 seconds
- Test pass rate: 100% (324/324)

---

## 2) Impact Assessment

### Development Velocity Impact

```
Debt Item: Duplicated Extension Methods
Locations: 8 AttributeValueExtension files (~400 lines)
Time Impact:
- Bug fix overhead: 2-4 hours (fix in 5 places instead of 1)
- Feature change overhead: 4-8 hours (add new type requires 100+ lines)
Monthly impact: 4 hours (estimated 1 change/month)
Annual Cost: 48 hours x $150/hour = $7,200
```

```
Debt Item: Monolithic PropertyMappingCodeRenderer
Locations: 1 file, 851 lines, 15+ methods
Time Impact:
- Bug fix overhead: 1-2 hours (navigation complexity)
- Feature change overhead: 2-4 hours (understanding code flow)
Monthly impact: 2 hours
Annual Cost: 24 hours x $150/hour = $3,600
```

```
Debt Item: Build Warnings (40)
Locations: Primarily examples/DynamoMapper.Nested
Time Impact:
- Code review overhead: 0.5 hours/PR (noise masking real issues)
- New contributor confusion: 1 hour
Monthly impact: 2 hours
Annual Cost: 24 hours x $150/hour = $3,600
```

### Risk Assessment

| Debt Item | Risk Level | Justification |
|-----------|------------|---------------|
| Missing vulnerability scanning | **High** | Security risk if dependencies have CVEs |
| No code coverage tracking | **Medium** | Can't measure test effectiveness |
| Build warnings | **Medium** | Real issues hidden in noise |
| Duplicated extension code | **Medium** | Maintenance burden, inconsistency risk |
| Monolithic renderer | **Medium** | Testability, maintainability |
| Missing formatting enforcement | **Low** | Cosmetic inconsistency |

---

## 3) Debt Metrics Dashboard

```yaml
Metrics:
  complexity:
    current_avg: unknown
    target_avg: 10
    methods_above_threshold:
      cc_10_plus: 3
      cc_20_plus: 1
    action: "Add cyclomatic complexity analyzer"

  duplication:
    percentage: "8-10%"
    target: "5%"
    hotspots:
      - "AttributeValueExtensions/*.cs": "~400 lines"
      - "PropertyMappingCodeRenderer.cs": "~200 lines"

  testing:
    unit_coverage: unknown
    integration_coverage: unknown
    target: "80% / 50%"
    total_tests: 324
    test_runtime_net10: "6.5s"
    critical_paths_untested:
      - "TypeMappingStrategyResolver logic"
      - "Diagnostic generation"
      - "Error paths"

  dependency_health:
    outdated_major: 0
    vulnerabilities:
      critical: 0
      high: 0
      medium: 0
    internal_beta_deps: 1

  build_health:
    build_time_seconds: 49
    test_time_seconds: 6.5
    warnings: 40
    flaky_tests: 0
```

### What's Currently Hurting Most?
1. **40 build warnings** create noise that can mask real issues
2. **No coverage tracking** means unknown test effectiveness
3. **Duplicated extension code** multiplies maintenance effort

### Measurement Plan
- **Monthly**: Run `dotnet list package --vulnerable` and track warning count
- **Quarterly**: Review complexity metrics, duplication percentage
- **Per-PR**: Track test count delta, coverage delta (once instrumented)

---

## 4) Prioritized Remediation Roadmap

### Quick Wins (This Sprint) - High Value, Low Effort

| # | Item | Effort | Savings | Risk |
|---|------|--------|---------|------|
| 1 | Fix nullability warnings in examples | 2h | 2h/month | Low |
| 2 | Add `dotnet format --verify-no-changes` to CI | 1h | 1h/month | Low |
| 3 | Add vulnerability scanning to CI | 1h | Security | Low |
| 4 | Add `required` modifier to example models | 1h | Cleaner examples | Low |
| 5 | Remove duplicate constructor selection call | 0.5h | Code clarity | Low |

### Medium-Term (1-3 Months)

| # | Item | Effort | Impact |
|---|------|--------|--------|
| 1 | Add code coverage tooling (Coverlet) | 4h | Visibility into test gaps |
| 2 | Extract shared logic in Runtime extensions | 8h | ~30% reduction in extension code |
| 3 | Split PropertyMappingCodeRenderer into focused classes | 6h | Testability, maintainability |
| 4 | Add unit tests for TypeMappingStrategyResolver | 4h | Cover critical logic |
| 5 | Add error path tests for diagnostics | 4h | Validate error handling |

### Long-Term (Quarter 2-4)

| # | Item | Effort | Impact |
|---|------|--------|--------|
| 1 | Consider Roslyn SyntaxFactory for code generation | 16h | Compile-time validation of generated code |
| 2 | Add benchmark tests for generated mappers | 8h | Performance regression detection |
| 3 | Implement analyzer enforcement across all projects | 4h | Consistent quality |
| 4 | Add architectural decision records (ADRs) | 4h | Design documentation |

---

## 5) Quick Wins (This Sprint) - Detailed

### 1. Fix Nullability Warnings in Examples
**Effort**: 2 hours
**Files**: `examples/DynamoMapper.Nested/*.cs`
**Action**: Add `required` modifier to non-nullable properties or make nullable
**Risk**: Low - examples only
**Savings**: Eliminates ~30 warnings, cleaner CI output

### 2. Add Format Check to CI
**Effort**: 1 hour
**Files**: `.github/workflows/pr-build.yaml`
**Action**: Add step: `dotnet format --verify-no-changes`
**Risk**: Low - fails fast on formatting issues
**Savings**: Consistent code style, reduced review friction

### 3. Add Vulnerability Scanning to CI
**Effort**: 1 hour
**Files**: `.github/workflows/build.yaml`
**Action**: Add step: `dotnet list package --vulnerable --include-transitive`
**Risk**: Low - informational initially
**Savings**: Early detection of security issues

### 4. Add Required Modifier to Example Models
**Effort**: 1 hour
**Files**: `examples/DynamoMapper.Nested/*.cs`
**Action**: Change `public string Id { get; set; }` to `public required string Id { get; set; }`
**Risk**: Low
**Savings**: Eliminates CS8618 warnings, demonstrates modern C#

### 5. Remove Duplicate Constructor Selection
**Effort**: 0.5 hours
**File**: `ConstructorSelector.cs:63,76`
**Action**: Refactor to single call
**Risk**: Low
**Savings**: Code clarity

---

## 6) Implementation Guide (Safe Refactoring Playbook)

### Refactoring Extension Method Duplication

**Current State**: Each numeric type (int, long, float, double, decimal) has 4 methods (~100 lines each):
- `TryGetXxx`
- `GetXxx`
- `TryGetNullableXxx`
- `GetNullableXxx`
- `SetXxx`

**Target State**: Generic helper with type-specific parsing delegates

```csharp
// Helper pattern
private static bool TryGetNumeric<T>(
    this Dictionary<string, AttributeValue> dict,
    string key,
    out T value,
    Func<string, T> parser,
    T defaultValue,
    Requiredness requiredness = Requiredness.InferFromNullability)
{
    value = defaultValue;
    if (!dict.TryGetValue(key, requiredness, out var attr))
        return false;
    var str = attr!.GetString(DynamoKind.N);
    value = str.Length == 0 ? defaultValue : parser(str);
    return true;
}
```

**Steps**:
1. Add characterization tests for current behavior
2. Extract generic helper methods
3. Refactor one type (int) to use helpers
4. Verify tests pass
5. Refactor remaining types
6. Remove duplicated code

### Refactoring PropertyMappingCodeRenderer

**Current State**: 851 lines, 15+ methods handling different rendering scenarios

**Target State**: Strategy pattern with focused renderers

```
PropertyMappingCodeRenderer (coordinator)
├── ScalarPropertyRenderer
├── NestedObjectRenderer
│   ├── MapperBasedRenderer
│   └── InlineRenderer
└── NestedCollectionRenderer
    ├── ListRenderer
    └── MapRenderer
```

**Steps**:
1. Add unit tests for each rendering scenario
2. Extract `IPropertyRenderer` interface
3. Create `ScalarPropertyRenderer` (move simple logic)
4. Create `NestedObjectRenderer` (extract nested object methods)
5. Create `NestedCollectionRenderer` (extract collection methods)
6. Replace switch expressions with polymorphic dispatch
7. Verify snapshot tests still pass

### Safe Refactoring Checklist

- [ ] Identify behavior and invariants (read existing tests)
- [ ] Add characterization tests if coverage is low
- [ ] Extract method/class in small steps
- [ ] Run tests after each step
- [ ] Verify snapshot tests unchanged
- [ ] Remove dead code after migration
- [ ] Update documentation

---

## 7) Prevention Plan

### Automated Quality Gates

| Gate | Tool | Threshold | Blocking? |
|------|------|-----------|-----------|
| Build warnings | `dotnet build -warnaserror` | 0 | Yes (PR) |
| Formatting | `dotnet format --verify-no-changes` | Clean | Yes (PR) |
| Vulnerabilities | `dotnet list package --vulnerable` | 0 critical/high | Yes (PR) |
| Test pass | `dotnet test` | 100% | Yes (PR) |
| Coverage | Coverlet | 80% (future) | Warning |

### Recommended CI Additions

```yaml
# Add to pr-build.yaml
- name: Check formatting
  run: dotnet format --verify-no-changes

- name: Check vulnerabilities
  run: dotnet list package --vulnerable --include-transitive

- name: Build with warnings as errors
  run: dotnet build -warnaserror
```

### Debt Budget

| Category | Allowed Monthly Increase | Mandatory Reduction |
|----------|-------------------------|---------------------|
| Build warnings | 0 new | Reduce by 10/month until 0 |
| Complexity hotspots | 0 new | 1 refactor/quarter |
| Duplication | 0% increase | Address during feature work |
| Test coverage | No decrease | Increase 2%/quarter |

---

## 8) ROI Projections

### Investment Summary

| Phase | Effort (hours) | Cost (@$150/hr) |
|-------|----------------|-----------------|
| Quick Wins | 6 | $900 |
| Medium-Term | 26 | $3,900 |
| Long-Term | 32 | $4,800 |
| **Total** | **64** | **$9,600** |

### Expected Savings

| Category | Monthly Savings | Annual Savings |
|----------|-----------------|----------------|
| Reduced maintenance (duplication) | 4 hours | $7,200 |
| Faster navigation (complexity) | 2 hours | $3,600 |
| Cleaner builds (warnings) | 2 hours | $3,600 |
| Security (vulnerability scanning) | Risk mitigation | Priceless |
| **Total** | **8 hours** | **$14,400** |

### Break-Even Analysis

- **Total Investment**: $9,600 (64 hours)
- **Monthly Savings**: $1,200 (8 hours)
- **Break-Even Month**: **Month 8**
- **12-Month ROI**: **50%** ($14,400 - $9,600 = $4,800 net)

### Assumptions
- Blended developer rate: $150/hour
- Conservative 1 change/month to extension methods
- 2 PRs/month affected by warning noise
- Security incident cost not included (would increase ROI significantly)

---

## Summary

**Overall Health**: Good foundation with manageable debt

**Top 3 Priorities**:
1. Fix build warnings (40) - immediate cleanup
2. Add CI quality gates (formatting, vulnerabilities)
3. Reduce extension method duplication

**Strengths**:
- Clean architecture with good separation
- Excellent use of functional error handling
- Comprehensive snapshot testing
- Modern tooling (central package management, SDK pinning)

**Key Risks**:
- No vulnerability scanning in CI
- Unknown test coverage
- Duplicated maintenance patterns