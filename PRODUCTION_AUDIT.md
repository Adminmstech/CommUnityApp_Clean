# CommUnityApp Production Audit

Audit date: 2026-07-27
Recheck date: 2026-07-27

Scope reviewed:
- Solution/project setup: `CommUnityApp.sln`, app projects, package references, appsettings, startup pipeline.
- Backend: MVC controllers, API controllers under `CommUnityApp/Services`, repositories under `Infrastructure/Repositories`, models under `Application/Models`, selected domain entities.
- Frontend/views: Razor areas, shared layouts, CDN usage, static uploaded images and saved URL patterns.
- Verification commands: release build and unit tests.

Current production verdict: not production-ready.

The project now builds in Release and the current unit tests pass, but it is still not production-level. The remaining blockers are committed credentials, no enforceable authorization model in source, weak server-side validation, runtime `NotImplementedException` paths, unsafe upload/image handling, committed runtime upload assets, vulnerable/incompatible packages, warning debt, and missing production operations controls.

## Recheck Summary

1. Release build passes.
   - Command: `dotnet build CommUnityApp.sln -c Release -m:1 -v:m`
   - Result: passed.
   - Remaining warnings: 65.
   - Important warning categories: high-severity package advisory, legacy .NET Framework packages, nullable warnings, duplicate usings, obsolete Firebase credential API, MVC partial deadlock warning, Windows-only QR/image APIs.

2. Unit tests pass.
   - Command: `dotnet test CommUnityApp.UnitTests/CommUnityApp.UnitTests.csproj -c Release -m:1 -v:m`
   - Result: 8 passed, 0 failed, 0 skipped.
   - Remaining concern: coverage is very small compared with the number of controllers, repositories, auth flows, upload paths, payment flows, event flows, auction flows, and game flows.

3. Current working tree is dirty.
   - Modified: `Application/CommUnityApp.Application.csproj`, `Application/Interfaces/ISpinGameRepository.cs`, `Application/Models/SpinGameModels.cs`, `CommUnityApp.UnitTests/SpinGameRepositoryTests.cs`, `CommUnityApp/CommUnityApp.csproj`, `Infrastructure/Repositories/SpinGameRepository.cs`.
   - Untracked: `Application/Models/GameSpin.cs`, `Application/Models/SpinGame.cs`, `PRODUCTION_AUDIT.md`.
   - Treat those existing code changes as user/project work. Do not revert them without explicit approval.

4. Earlier parallel build/test lock was transient.
   - A parallel `dotnet build` plus `dotnet test` run failed with `CS2012` because `Domain/obj/Release/net8.0/CommUnityApp.Domain.dll` was locked.
   - Re-running the release build by itself passed.

## Blocking Runtime Bugs

1. Email flows can still crash after database actions.
   - Evidence: `Infrastructure/Services/EmailService.cs:42` and `Infrastructure/Services/EmailService.cs:127` throw `NotImplementedException`.
   - Risk: user creation and password reset success paths can fail after the database operation succeeds.
   - Production fix: implement `SendPasswordResetSuccessEmailAsync` and `SendWelcomeEmailAsync`, and make email sending retryable/non-blocking where appropriate.

2. Repository methods still throw at runtime.
   - Evidence: `Infrastructure/Repositories/AuctionRepository.cs:28`, `:33`, `:38`, `:43`, `:99`; `Infrastructure/Repositories/UserRepository.cs:25`, `:30`, `:35`, `:40`, `:104`; `Infrastructure/Repositories/CommunityRepository.cs:676`, `:702`.
   - Risk: any caller using generic repository methods can hit runtime crashes.
   - Production fix: implement the methods or remove them from public interfaces/contracts.

## Security Findings

1. Production secrets are committed.
   - Evidence: `CommUnityApp/appsettings.json:35` contains SQL Server host/user/password; `:39` contains JWT key; `:47` contains SMTP Gmail app password.
   - Production fix: rotate the database password, SMTP password, and JWT signing key immediately. Move all secrets to environment variables or a secret store.

2. Swagger is enabled by production config.
   - Evidence: `CommUnityApp/appsettings.json:64`, `CommUnityApp/Program.cs:161`.
   - Risk: public endpoint discovery.
   - Production fix: disable outside development or protect with admin auth/IP allow-list.

3. No source-level authorization attributes were found.
   - Evidence: targeted scan found no `[Authorize]` or `[AllowAnonymous]` in C# source.
   - Risk: API endpoints for users, wallets, communications, community posts, auctions, products, rewards, services, and deletes can be called directly unless stored procedures independently block them.
   - Production fix: require authorization globally, explicitly mark login/public read endpoints anonymous, and enforce role/resource ownership checks.

4. JWT tokens are issued but JWT bearer auth is not configured.
   - Evidence: `UserController.cs` creates JWT tokens; `Program.cs` only configures cookie authentication.
   - Risk: APIs cannot reliably authenticate bearer tokens.
   - Production fix: add `AddJwtBearer`, validate issuer/audience/signing key, and add API authorization policies.

5. CSRF protection is missing on MVC mutation forms.
   - Evidence: no `ValidateAntiForgeryToken`, `AutoValidateAntiforgeryToken`, or `AddAntiforgery` usage found in source scan.
   - Risk: admin/business/community form posts can be forged from another site.
   - Production fix: enable global anti-forgery for MVC and add tokens to forms/AJAX.

6. Cookie settings need hardening.
   - Evidence: `Program.cs` cookie auth sets `HttpOnly` but not a production `SecurePolicy` or explicit `SameSite`; community login writes one-year `CommunityId` and `CommunityName` cookies at `Areas/Community/Controllers/AccountController.cs:37-51` and `:88-102`.
   - Production fix: use short-lived auth cookies, `CookieSecurePolicy.Always`, intentional `SameSite`, server-side sign-out, and avoid duplicating identity values in custom cookies.

7. Sensitive exception messages are returned to clients.
   - Evidence: many controllers return `ResultMessage = ex.Message` or `BadRequest(ex.Message)`, including `UserController`, `CommunityController`, `AuctionController`, `BusinessController`, `ProductController`, `SmartQuizController`, and others.
   - Production fix: return generic errors with correlation IDs; log detailed exceptions server-side.

8. Password handling is inconsistent and partly plaintext-style.
   - Evidence: repositories pass `@Password` directly for create/login paths; `VolunteerRepository.cs` uses `SELECT * FROM Users WHERE Email=@Email AND Password=@Password AND IsActive=1`.
   - Production fix: standardize on salted password hashes for all account types, remove plaintext comparisons, force reset for legacy accounts, and stop emailing reusable passwords.

## Validation Findings

1. Request/model validation is mostly absent.
   - Evidence: targeted scan found no `[Required]`, `[EmailAddress]`, `[StringLength]`, `[Range]`, or `IValidatableObject` in application/domain/controller source.
   - Production fix: introduce dedicated request DTOs with required fields, lengths, email/phone/URL validation, numeric ranges, date ordering, and valid enum/status checks.

2. Some login paths dereference null request/model values.
   - Evidence: `HomeController.cs:67` and `CommunityController.cs:50` access request properties before null checks.
   - Production fix: validate null first and use model binding validation consistently.

3. Public endpoints trust user-provided IDs.
   - Examples: `GetUserWalletTransactions(Guid userId)`, `GetMyRequestedItems(Guid userId)`, `GetCommunityPostsByUser(Guid userId)`, `DeleteCommunityPost(int postId)`.
   - Risk: insecure direct object reference.
   - Production fix: derive IDs from authenticated claims and verify resource ownership in service/repository/database.

4. Business rules are not consistently enforced before database calls.
   - Areas: auctions, products, services, rewards, charity items, quiz/spin games, ticket bookings, wallet transactions.
   - Production fix: validate price/coin/quantity ranges, date windows, status transitions, max lengths, and required ownership before repository calls.

## Image, File, And Saved URL Findings

1. Runtime/user-uploaded files are committed.
   - Evidence: many files under `CommUnityApp/wwwroot/Uploads`, `CommUnityApp/Uploads`, `ProfilePics`, `CommunityLogos`, QR folders, product/promotion/event images.
   - Risk: repo bloat, accidental PII exposure, unstable deployments, hard-to-reproduce storage state.
   - Production fix: move uploads to object storage or mounted storage, add gitignore rules, and keep only intentional seed/demo assets.

2. Upload validation is weak and inconsistent.
   - Evidence: several paths save uploads/base64 after using only `Path.GetExtension`, `Convert.FromBase64String`, or raw `WriteAllBytes`.
   - Examples: `CommunityController.cs:750`, `:944`; `EventController.cs:55`, `:390`; `JobController.cs:156`; `SmartQuizRepository.cs:348`, `:393`; product/service/campaign upload paths.
   - Production fix: enforce size limits, MIME checks, magic-byte validation, allowed extensions, image re-encoding, document scanning, and request-size limits.

3. Base64 image decoding can consume excessive memory.
   - Evidence: multiple endpoints decode base64 into byte arrays before robust size checks.
   - Production fix: reject oversized requests early, use streaming uploads, and enforce max dimensions and per-file limits.

4. Saved URL/path casing is inconsistent.
   - Evidence: both `/uploads/...` and `/Uploads/...` appear, and both `wwwroot/uploads` and `wwwroot/Uploads` are used.
   - Risk: works on Windows but can break on Linux/container hosts.
   - Production fix: normalize all stored URLs and physical folders to one casing, then migrate existing data/files.

5. Public URLs are built from incoming request host.
   - Evidence: many endpoints build `baseUrl = $"{Request.Scheme}://{Request.Host}"`.
   - Risk: wrong links behind reverse proxies and possible host-header abuse.
   - Production fix: use configured canonical public base URL plus proper forwarded header configuration.

6. Hard-coded production and localhost URLs remain.
   - Evidence: `AuctionController.cs:531-532`, `EventRepository.cs:736`, `EventRepository.cs:792`.
   - Production fix: use typed options from configuration for all generated links.

## Startup And Configuration

1. Duplicate registrations remain.
   - Evidence: `Program.cs` registers session twice, maps controllers twice, and registers `IDapperWrapper` twice.
   - Production fix: remove duplicates for clarity and predictable startup.

2. Serilog config is present but not wired.
   - Evidence: `appsettings.json` contains `Serilog`; no `UseSerilog` found.
   - Production fix: wire Serilog or remove stale config.

3. Firebase startup behavior is ambiguous.
   - Evidence: missing/invalid Firebase credentials write to stderr and app continues.
   - Production fix: fail fast if Firebase is required, or expose degraded status via health checks if optional.

4. `AllowedHosts` is unrestricted.
   - Evidence: `CommUnityApp/appsettings.json:33` has `AllowedHosts: "*"`.
   - Production fix: restrict to production host names.

## Data And Repository Risks

1. Stored procedures are required but not versioned in source.
   - Risk: application behavior cannot be reproduced from code alone.
   - Production fix: add DB migration scripts, stored procedure source, and integration tests against a disposable database.

2. Nullable warning debt remains.
   - Evidence: release build reports many nullable warnings across models, repositories, controllers, and views.
   - Production fix: fix nullability annotations and null handling; later enforce warnings-as-errors.

3. `SELECT *` is still used.
   - Evidence: `AuctionRepository.cs:338`, `BrandGameRepository.cs:99`, `:107`, `:114`, `QuizGameRepository.cs:239`, `:245`, `:265`, `:271`, `:277`, `SpinGameRepository.cs:343`, `:351`, `:359`, `VolunteerRepository.cs:29`.
   - Production fix: select explicit columns to reduce over-fetching and schema-drift risk.

4. File and database writes are not transactionally coordinated.
   - Risk: orphaned files or database rows when one half of a workflow fails.
   - Production fix: use pending/finalized file states, cleanup jobs, or compensating deletes.

## Frontend/View Production Notes

1. Many views depend on public CDNs.
   - Evidence: Bootstrap, Font Awesome, SweetAlert, DataTables, jQuery, Google Fonts, Unsplash, placeholder image URLs.
   - Production fix: pin versions with SRI or self-host critical assets.

2. Inline page CSS/scripts are widespread.
   - Risk: harder caching, minification, and CSP adoption.
   - Production fix: move shared code into bundled/versioned assets.

3. MVC partial warning exists.
   - Evidence: build warning `MVC1000` in `Areas/Business/Views/QuizGame/Create.cshtml`.
   - Production fix: use `<partial>` tag helper or `PartialAsync`.

## Package And Platform Risks

1. Known vulnerable package.
   - Evidence: build reports `NU1903` for `Microsoft.OpenApi 2.4.1`, high-severity advisory `GHSA-v5pm-xwqc-g5wc`.
   - Production fix: update `Microsoft.OpenApi` and compatible Swagger packages.

2. Legacy SignalR/Owin packages target .NET Framework.
   - Evidence: repeated `NU1701` warnings for `Microsoft.AspNet.SignalR.Core`, `Microsoft.Owin`, `Microsoft.Owin.Security`, and `Owin`.
   - Production fix: remove legacy packages and use ASP.NET Core-compatible SignalR packages.

3. Package/runtime versions are mixed.
   - Evidence: app targets `net8.0`, references EF Core `9.0.12`, old SignalR packages, and local SDK 10 is being used.
   - Production fix: align package versions with the intended runtime and add `global.json`.

4. Windows-only QR/image APIs may fail on Linux.
   - Evidence: build reports `CA1416` for QR/image generation in `CommUnityApp/Services/EventController.cs`.
   - Production fix: use cross-platform QR/image generation or document Windows-only hosting.

## Operational Readiness Gaps

1. No health checks found.
   - Add DB, SMTP, Firebase, Stripe, storage, and background worker health checks.

2. No rate limiting found.
   - Add rate limits to login, OTP, password reset, registration, file upload, bidding, and messaging endpoints.

3. No centralized API error contract.
   - Add middleware/filter for standardized error responses and correlation IDs.

4. No background job strategy found.
   - Needed for email retries, push notifications, image cleanup, auction status transitions, QR generation, and ticket workflows.

5. No deployment-safe upload storage strategy.
   - Local `wwwroot` writes are fragile in scaled/cloud deployments.

## Suggested Production Fix Order

1. Rotate and remove committed secrets.
2. Add authentication and authorization globally for MVC and APIs.
3. Add anti-forgery protection to MVC mutation forms.
4. Implement missing email methods and remove active `NotImplementedException` paths.
5. Add request DTO validation for all create/update/login/payment/upload endpoints.
6. Harden uploads and move runtime files to external storage.
7. Remove committed runtime uploads from source control after backing them up.
8. Disable or protect Swagger in production.
9. Update vulnerable and incompatible packages.
10. Fix production-impacting warnings: package, platform, nullable, MVC partial, obsolete Firebase API.
11. Add database migrations/stored procedure source and integration tests.
12. Add health checks, rate limiting, structured logging, and deployment documentation.

