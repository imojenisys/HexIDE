### Added

- **Archive text previews reach language servers** (#1118). Entries use opaque `archive-entry:` URIs carrying their container and escaped name segments, with diagnostics shown in their read-only tabs. Reload refreshes the session; closing the tab closes it. Automation lists each entry's URI, protocol inspection records it, and pseudonymised exports hide its names.
