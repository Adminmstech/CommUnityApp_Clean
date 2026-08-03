# Task TODO

## SpinGame Create.cshtml JS bugfix
- [x] Add null-safe Razor config helpers (`firstConfig`, `isEditMode`, serialized config payload)
- [x] Replace direct `Model.Configs[0]` date bindings with null-safe bindings
- [x] Fix invalid tab selector interpolation bug in jQuery validate `invalidHandler`
- [x] Fix edit-mode JS initialization to use safe Razor literals/JSON
- [x] Normalize `ConfigIsActive` hidden value to `'true'/'false'`
- [x] Smooth section image edit binding (clear old hidden image when new file selected)
- [ ] Run build check for Razor/JS integration safety

## GameController PlaySpinGame response update
- [ ] Update PlaySpinGame success response to include full spin game image URL
- [ ] Return redeem-code/store-offer friendly fields in response
- [ ] Keep backward compatibility with existing response fields
- [ ] Run build check for API changes
